using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;

/// <summary>
/// MABArchive scheduled batch job handler.
/// Loads FPS data into the MABArchive schema within PostgreSQL database. The FPS year(s)
/// processed are resolved from fps.tblyearmaster (Open/Planned status), never from the
/// system date - see docs/mabarchive-year-selection-processing-spec.md.
/// Runs weekly on weekdays at 8:00 PM UTC.
///
/// Lock lifecycle is owned exclusively by JobOrchestrator. This handler must not
/// acquire or release the distributed lock.
/// </summary>

public sealed class MabArchiveJobHandler : IBatchJob
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MabArchiveJobHandler> _logger;
    private readonly ICorrelationService _correlationService;
    private readonly MabArchiveSettings _settings;

    public string Name => "MABArchive";
    public string IdempotencyStrategy => "YearScopedRebuildWithDeterministicOrdering";
    public string? ScheduleExpression => "cron(0 20 ? * MON-FRI *)";
    public string? ScheduleDescription => "Weekdays (Monday to Friday) at 8:00 PM UTC";
    public int? MaxExecutionSeconds => null;

    public MabArchiveJobHandler(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IServiceProvider serviceProvider,
        ICorrelationService correlationService,
        ILogger<MabArchiveJobHandler> logger,
        IOptions<MabArchiveSettings> settings)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? new MabArchiveSettings();
    }

    /// <summary>
    /// Executes the MABArchive load job.
    /// Lock acquisition and release are handled by JobOrchestrator before this is called.
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

        await using var dbContext = _dbContextFactory.CreateDbContext();

        try
        {
            var orchestrator = _serviceProvider.GetRequiredService<MabArchiveLoadOrchestrator>();
            var context = await orchestrator.ResolveExecutionContextAsync(cancellationToken);
            _logger.LogInformation(
                "Execution context resolved from fps.tblyearmaster | OpenYear={OpenYear} | PlannedYear={PlannedYear}",
                context.OpenYear,
                context.PlannedYear);

            // Transaction wrapper using the provided context
            async Task TransactionWrapper(Func<Task> action)
            {
                var executionStrategy = dbContext.Database.CreateExecutionStrategy();
                await executionStrategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                    await action();
                    await transaction.CommitAsync(cancellationToken);
                });
            }

            using var loadStep = _logger.BeginScope(new Dictionary<string, object?> { ["StepName"] = "ExecuteLoad" });
            await orchestrator.ExecuteAsync(
                jobExecutionId,
                context,
                TransactionWrapper,
                cancellationToken);

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
}
