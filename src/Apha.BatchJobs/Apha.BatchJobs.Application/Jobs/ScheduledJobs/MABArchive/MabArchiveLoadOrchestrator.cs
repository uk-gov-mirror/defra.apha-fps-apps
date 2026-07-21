using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;

/// <summary>
/// Orchestrator for MABArchive load operations.
/// Manages year determination, transaction lifecycle, and step sequencing.
/// </summary>
public sealed class MabArchiveLoadOrchestrator
{
    private readonly IReloadFpsTotalsService _totalsService;
    private readonly IMyFpsYearlyDataService _dataService;
    private readonly IExecutionYearContext _executionYearContext;
    private readonly IMabArchiveYearSelectionService _yearSelectionService;
    private readonly IEmailNotificationService _notificationService;
    private readonly ILogger<MabArchiveLoadOrchestrator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MabArchiveLoadOrchestrator"/> class.
    /// </summary>
    /// <param name="totalsService">Service for rebuilding FPS source totals.</param>
    /// <param name="dataService">Service for archive delete/load/refresh operations.</param>
    /// <param name="yearSelectionService">Service that resolves Open/Planned years from fps.tblyearmaster.</param>
    /// <param name="notificationService">Service used to send failure notifications.</param>
    /// <param name="logger">Logger instance.</param>
    public MabArchiveLoadOrchestrator(
        IReloadFpsTotalsService totalsService,
        IMyFpsYearlyDataService dataService,
        IExecutionYearContext executionYearContext,
        IMabArchiveYearSelectionService yearSelectionService,
        IEmailNotificationService notificationService,
        ILogger<MabArchiveLoadOrchestrator> logger)
    {
        _totalsService = totalsService ?? throw new ArgumentNullException(nameof(totalsService));
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _executionYearContext = executionYearContext ?? throw new ArgumentNullException(nameof(executionYearContext));
        _yearSelectionService = yearSelectionService ?? throw new ArgumentNullException(nameof(yearSelectionService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resolves the execution context from fps.tblyearmaster (Open/Planned years).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resolved execution context for this run.</returns>
    public Task<MabArchiveExecutionContext> ResolveExecutionContextAsync(CancellationToken cancellationToken)
        => _yearSelectionService.GetProcessableYearsAsync(cancellationToken);

    /// <summary>
    /// Executes the MABArchive load orchestration within a single transaction.
    /// </summary>
    /// <param name="correlationId">Correlation identifier for this execution.</param>
    /// <param name="context">Computed execution context for year/month branching.</param>
    /// <param name="transactionWrapper">Transaction wrapper delegate used to execute all work atomically.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task ExecuteAsync(
        string correlationId,
        MabArchiveExecutionContext context,
        Func<Func<Task>, Task> transactionWrapper,
        CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["OpenYear"] = context.OpenYear,
            ["PlannedYear"] = context.PlannedYear ?? (object)"none"
        });

        _logger.LogInformation(
            "MABArchive orchestration start | OpenYear={OpenYear} | PlannedYear={PlannedYear} | SelectionSource=fps.tblyearmaster",
            context.OpenYear,
            context.PlannedYear);

        try
        {
            await transactionWrapper(async () =>
            {
                _logger.LogInformation(
                    "Starting MABArchive full processing | FpsYear={FpsYear} | YearStatus=Open",
                    context.OpenYear);
                await ExecuteFullYearCycleAsync(context.OpenYear, cancellationToken);
                _logger.LogInformation(
                    "Completed MABArchive full processing | FpsYear={FpsYear}",
                    context.OpenYear);

                if (context.PlannedYear.HasValue)
                {
                    _logger.LogInformation(
                        "Starting MABArchive project-only processing | FpsYear={FpsYear} | YearStatus=Planned",
                        context.PlannedYear.Value);
                    await ExecuteProjectOnlyRefreshAsync(context.PlannedYear.Value, cancellationToken);
                    _logger.LogInformation(
                        "Completed MABArchive project-only processing | FpsYear={FpsYear}",
                        context.PlannedYear.Value);
                }

                _logger.LogInformation("MABArchive orchestration completed successfully");
            });
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "MABArchive orchestration cancelled | CorrelationId={CorrelationId}", correlationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MABArchive orchestration failed | CorrelationId={CorrelationId}", correlationId);

            // Send failure notification
            try
            {
                await _notificationService.SendFailureNotificationAsync(
                    correlationId,
                    "MABArchive",
                    ex.Message,
                    DateTime.UtcNow,
                    CancellationToken.None);
            }
            catch (Exception notificationEx)
            {
                _logger.LogWarning(notificationEx, "Failed to send failure notification for CorrelationId={CorrelationId}", correlationId);
            }

            throw;
        }
    }

    private async Task ExecuteFullYearCycleAsync(int year, CancellationToken cancellationToken)
    {
        _executionYearContext.FpsYear = year;
        _executionYearContext.YearSource = "MABArchive.OpenYearFullCycle";

        _logger.LogInformation("Executing full cycle for Open year {Year}", year);

        var totalsRows = await _totalsService.RebuildSourceTotalsAsync(year, cancellationToken);
        _logger.LogInformation("Rebuilt source totals for year {Year} | RowsInserted={RowsInserted}", year, totalsRows);

        var deletedRows = await _dataService.DeleteYearDataAsync(year, cancellationToken);
        _logger.LogInformation("Deleted archive data for year {Year} | RowsDeleted={RowsDeleted}", year, deletedRows);

        var loadedRows = await _dataService.LoadYearDataAsync(year, cancellationToken);
        _logger.LogInformation("Loaded archive data for year {Year} | RowsLoaded={RowsLoaded}", year, loadedRows);
    }

    private async Task ExecuteProjectOnlyRefreshAsync(int year, CancellationToken cancellationToken)
    {
        _executionYearContext.FpsYear = year;
        _executionYearContext.YearSource = "MABArchive.PlannedYearProjectRefresh";

        _logger.LogInformation("Executing project-only refresh for Planned year {Year}", year);

        var refreshedRows = await _dataService.RefreshProjectsOnlyAsync(year, cancellationToken);
        _logger.LogInformation("Refreshed projects for year {Year} | RowsRefreshed={RowsRefreshed}", year, refreshedRows);
    }
}
