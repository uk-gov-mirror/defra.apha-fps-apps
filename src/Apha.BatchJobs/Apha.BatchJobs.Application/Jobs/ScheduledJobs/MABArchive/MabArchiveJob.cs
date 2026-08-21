using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Ports;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces.MabArchive;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;

/// <summary>
/// MABArchive scheduled batch job.
/// Loads FPS data into the MABArchive schema within PostgreSQL database. The FPS year(s)
/// processed are resolved from fps.tblyearmaster (Open/Planned status), never from the
/// system date - see docs/mabarchive-year-selection-processing-spec.md.
/// Runs weekly on weekdays at 8:00 PM UTC.
///
/// Lock lifecycle and failure notification are owned exclusively by JobOrchestrator: this job
/// must not acquire/release the distributed lock, and must not send its own failure notification
/// - JobOrchestrator sends one, best-effort, once retries are exhausted.
///
/// Totals rebuild, archive delete/load, and project refresh all run within a single transaction
/// managed by <see cref="IMabArchiveTransactionManager"/>, so the whole Open+Planned cycle
/// commits or rolls back atomically.
/// </summary>
public sealed class MabArchiveJob : IBatchJob
{
    private readonly IMabArchiveTransactionManager _transactionManager;
    private readonly IMabArchiveYearSelectionRepository _yearSelectionRepository;
    private readonly IFpsTotalsRepository _totalsRepository;
    private readonly IMabArchiveYearRepository _yearRepository;
    private readonly IExecutionYearContext _executionYearContext;
    private readonly ICorrelationContextAccessor _correlationService;
    private readonly ILogger<MabArchiveJob> _logger;
    private readonly MabArchiveSettings _settings;

    public string Name => "MABArchive";
    public string IdempotencyStrategy => "YearScopedRebuildWithDeterministicOrdering";
    public string? ScheduleExpression => "cron(0 20 ? * MON-FRI *)";
    public string? ScheduleDescription => "Weekdays (Monday to Friday) at 8:00 PM UTC";
    public int? MaxExecutionSeconds => null;

    public MabArchiveJob(
        IMabArchiveTransactionManager transactionManager,
        IMabArchiveYearSelectionRepository yearSelectionRepository,
        IFpsTotalsRepository totalsRepository,
        IMabArchiveYearRepository yearRepository,
        IExecutionYearContext executionYearContext,
        ICorrelationContextAccessor correlationService,
        ILogger<MabArchiveJob> logger,
        IOptions<MabArchiveSettings> settings)
    {
        _transactionManager = transactionManager ?? throw new ArgumentNullException(nameof(transactionManager));
        _yearSelectionRepository = yearSelectionRepository ?? throw new ArgumentNullException(nameof(yearSelectionRepository));
        _totalsRepository = totalsRepository ?? throw new ArgumentNullException(nameof(totalsRepository));
        _yearRepository = yearRepository ?? throw new ArgumentNullException(nameof(yearRepository));
        _executionYearContext = executionYearContext ?? throw new ArgumentNullException(nameof(executionYearContext));
        _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? new MabArchiveSettings();
    }

    /// <summary>
    /// Executes the MABArchive load job.
    /// Lock acquisition/release and failure notification are handled by JobOrchestrator.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        var jobExecutionId = _correlationService.GetCorrelationId() ?? _correlationService.GenerateCorrelationId();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["JobExecutionId"] = jobExecutionId,
            ["JobName"] = Name
        });

        _logger.LogInformation("===========================================");
        _logger.LogInformation("MABArchive Job - Starting");
        _logger.LogInformation("===========================================");
        _logger.LogInformation("JobExecutionId: {JobExecutionId} | Timestamp: {StartTime:yyyy-MM-dd HH:mm:ss.fff} | ProcessId: {ProcessId}",
            jobExecutionId, startedAt, Environment.ProcessId);

        try
        {
            var context = await _yearSelectionRepository.GetProcessableYearsAsync(cancellationToken);
            _logger.LogInformation(
                "Execution context resolved from fps.tblyearmaster | OpenYear={OpenYear} | PlannedYear={PlannedYear}",
                context.OpenYear,
                context.PlannedYear);

            using var orchestrationScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = jobExecutionId,
                ["OpenYear"] = context.OpenYear,
                ["PlannedYear"] = context.PlannedYear ?? (object)"none"
            });

            _logger.LogInformation(
                "MABArchive orchestration start | OpenYear={OpenYear} | PlannedYear={PlannedYear} | SelectionSource=fps.tblyearmaster",
                context.OpenYear,
                context.PlannedYear);

            using var loadStep = _logger.BeginScope(new Dictionary<string, object?> { ["StepName"] = "ExecuteLoad" });

            await _transactionManager.ExecuteAsync(async ct =>
            {
                _logger.LogInformation(
                    "Starting MABArchive full processing | FpsYear={FpsYear} | YearStatus=Open",
                    context.OpenYear);
                await ExecuteFullYearCycleAsync(context.OpenYear, ct);
                _logger.LogInformation(
                    "Completed MABArchive full processing | FpsYear={FpsYear}",
                    context.OpenYear);

                if (context.PlannedYear.HasValue)
                {
                    _logger.LogInformation(
                        "Starting MABArchive project-only processing | FpsYear={FpsYear} | YearStatus=Planned",
                        context.PlannedYear.Value);
                    await ExecuteProjectOnlyRefreshAsync(context.PlannedYear.Value, ct);
                    _logger.LogInformation(
                        "Completed MABArchive project-only processing | FpsYear={FpsYear}",
                        context.PlannedYear.Value);
                }

                _logger.LogInformation("MABArchive orchestration completed successfully");
            }, cancellationToken);

            var duration = DateTime.UtcNow - startedAt;
            _logger.LogInformation("===========================================");
            _logger.LogInformation(
                "MABArchive Job - Completed Successfully | JobExecutionId={JobExecutionId} | Duration={DurationSeconds}s",
                jobExecutionId,
                (int)duration.TotalSeconds);
            _logger.LogInformation("===========================================");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "MABArchive job execution was interrupted | JobExecutionId={JobExecutionId}", jobExecutionId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MABArchive job failed with unhandled exception | JobExecutionId={JobExecutionId}", jobExecutionId);
            throw;
        }
    }

    private async Task ExecuteFullYearCycleAsync(int year, CancellationToken cancellationToken)
    {
        _executionYearContext.FpsYear = year;
        _executionYearContext.YearSource = "MABArchive.OpenYearFullCycle";

        _logger.LogInformation("Executing full cycle for Open year {Year}", year);

        var totalsRows = await _totalsRepository.RebuildSourceTotalsAsync(year, cancellationToken);
        _logger.LogInformation("Rebuilt source totals for year {Year} | RowsInserted={RowsInserted}", year, totalsRows);

        var deletedRows = await _yearRepository.DeleteYearDataAsync(year, cancellationToken);
        _logger.LogInformation("Deleted archive data for year {Year} | RowsDeleted={RowsDeleted}", year, deletedRows);

        var loadedRows = await _yearRepository.LoadYearDataAsync(year, cancellationToken);
        _logger.LogInformation("Loaded archive data for year {Year} | RowsLoaded={RowsLoaded}", year, loadedRows);
    }

    private async Task ExecuteProjectOnlyRefreshAsync(int year, CancellationToken cancellationToken)
    {
        _executionYearContext.FpsYear = year;
        _executionYearContext.YearSource = "MABArchive.PlannedYearProjectRefresh";

        _logger.LogInformation("Executing project-only refresh for Planned year {Year}", year);

        var refreshedRows = await _yearRepository.RefreshProjectsOnlyAsync(year, cancellationToken);
        _logger.LogInformation("Refreshed projects for year {Year} | RowsRefreshed={RowsRefreshed}", year, refreshedRows);
    }
}
