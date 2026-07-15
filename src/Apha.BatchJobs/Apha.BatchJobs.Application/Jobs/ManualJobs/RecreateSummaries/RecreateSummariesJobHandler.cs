using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;

/// <summary>
/// RecreateSummaries scheduled batch job handler.
/// Rebuilds monthly FPS summary/calculation data by executing 14 ordered SQL steps
/// and optionally refreshing period snapshot tables when the period is unlocked.
///
/// Replaces the legacy SQL Server <c>sp_RecreateSummaries</c> orchestration procedure.
///
/// Lock lifecycle is owned exclusively by <see cref="JobOrchestrator"/>.
/// This handler must not acquire or release the distributed lock.
/// </summary>

public sealed class RecreateSummariesJobHandler : IBatchJob
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IRecreateSummariesStepCatalog _stepCatalog;
    private readonly IRecreateSummariesContext _jobContext;
    private readonly IJobExecutionRepository? _executionRepository;
    private readonly IBatchLockRepository? _lockRepository;
    private readonly ICorrelationService _correlationService;
    private readonly ILogger<RecreateSummariesJobHandler> _logger;

    /// <summary>Canonical job name.</summary>
    public string Name => "RecreateSummary";

    /// <summary>
    /// Idempotency strategy: full delete-and-rebuild per month with a single wrapping transaction.
    /// </summary>
    public string IdempotencyStrategy => "DeleteAndRebuildWithSingleTransaction";

    /// <summary>
    /// RecreateSummaries is a manually triggered job — no schedule expression.
    /// </summary>
    public string? ScheduleExpression => null;

    /// <summary>Human-readable schedule description.</summary>
    public string? ScheduleDescription => "Manually triggered per FPS period month";

    /// <summary>Maximum execution timeout: 60 minutes.</summary>
    public int? MaxExecutionSeconds => 3600;

    /// <summary>
    /// Initializes a new instance of <see cref="RecreateSummariesJobHandler"/>.
    /// </summary>
    public RecreateSummariesJobHandler(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IRecreateSummariesStepCatalog stepCatalog,
        IRecreateSummariesContext jobContext,
        IJobExecutionRepository? executionRepository,
        IBatchLockRepository? lockRepository,
        ICorrelationService correlationService,
        ILogger<RecreateSummariesJobHandler> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _stepCatalog = stepCatalog ?? throw new ArgumentNullException(nameof(stepCatalog));
        _jobContext = jobContext ?? throw new ArgumentNullException(nameof(jobContext));
        _executionRepository = executionRepository;
        _lockRepository = lockRepository;
        _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        var jobExecutionId = _correlationService.GetCorrelationId() ?? _correlationService.GenerateCorrelationId();

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["JobExecutionId"] = jobExecutionId,
            ["JobName"] = Name,
            ["Month"] = _jobContext.Month,
            ["Year"] = _jobContext.Year,
            ["TriggeredBy"] = _jobContext.TriggeredBy
        });

        _logger.LogInformation("===========================================");
        _logger.LogInformation("RecreateSummaries Job - Starting");
        _logger.LogInformation("===========================================");
        _logger.LogInformation(
            "JobExecutionId: {JobExecutionId} | Month: {Month} | Year: {Year} | TriggeredBy: {TriggeredBy} | Timestamp: {StartTime:yyyy-MM-dd HH:mm:ss.fff}",
            jobExecutionId, _jobContext.Month, _jobContext.Year, _jobContext.TriggeredBy, startedAt);

        try
        {
            var results = await ExecuteStepsAsync(
                jobExecutionId,
                _jobContext.Month,
                _jobContext.Year,
                _jobContext.TriggeredBy,
                cancellationToken);

            var duration = DateTime.UtcNow - startedAt;

            _logger.LogInformation("===========================================");
            _logger.LogInformation(
                "RecreateSummaries Job - Completed Successfully | JobExecutionId={JobExecutionId} | Month={Month} | Year={Year} | Steps={StepCount} | Duration={DurationSeconds}s",
                jobExecutionId, _jobContext.Month, _jobContext.Year, results.Count, (int)duration.TotalSeconds);
            _logger.LogInformation("===========================================");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "RecreateSummaries job execution was interrupted | JobExecutionId={JobExecutionId}", jobExecutionId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RecreateSummaries job failed | JobExecutionId={JobExecutionId} | Month={Month} | Year={Year}", jobExecutionId, _jobContext.Month, _jobContext.Year);
            throw;
        }
    }

    /// <summary>
    /// Executes steps 1–14 in order, reads the period-lock flag, and
    /// conditionally executes steps 15–17, all within one transaction.
    /// Includes step-based heartbeat checks and slow-step detection.
    /// </summary>
    private async Task<IReadOnlyList<StepResult>> ExecuteStepsAsync(
        string jobExecutionId,
        int month,
        int year,
        string triggeredBy,
        CancellationToken cancellationToken)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        IReadOnlyList<StepResult>? completedResults = null;
        var executionStrategy = context.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            var results = new List<StepResult>();

            // Ensure retries start from a clean tracking graph.
            context.ChangeTracker.Clear();

            var npgsqlConnection = (NpgsqlConnection)context.Database.GetDbConnection();
            if (npgsqlConnection.State != System.Data.ConnectionState.Open)
                await context.Database.OpenConnectionAsync(cancellationToken);

            var executionContext = new RecreateSummariesExecutionContext(context, npgsqlConnection, year);

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation("[{JobExecutionId}] RecreateSummaries implementation: DotNetLinq", jobExecutionId);

                // --- Steps 1–14 (mandatory, ordered) ---
                var mandatorySteps = _stepCatalog.BuildMandatorySteps(month, year, triggeredBy);

                foreach (var step in mandatorySteps)
                {
                    // Step-based heartbeat: touch execution and renew lock before starting step
                    await TouchHeartbeatAsync(jobExecutionId, cancellationToken);

                    using var stepScope = _logger.BeginScope(new Dictionary<string, object?> { ["StepName"] = step.StepName });
                    _logger.LogInformation(
                        "[{JobExecutionId}] Executing step: {StepName}", jobExecutionId, step.StepName);

                    var result = await step.ExecuteAsync(executionContext, cancellationToken);
                    results.Add(result);

                    var stepDurationMs = (int)(result.EndTime - result.StartTime).TotalMilliseconds;
                    _logger.LogInformation(
                        "[{JobExecutionId}] Step {StepName} -> {Status} | RowsAffected={Rows} | Duration={Ms}ms",
                        jobExecutionId, result.StepName, result.Status, result.RowsAffected,
                        stepDurationMs);

                    // Warn if step exceeded 2 minutes (slow-step detection)
                    if (stepDurationMs > 120_000)
                    {
                        _logger.LogInformation(
                            "[{JobExecutionId}] SLOW STEP DETECTED | StepName={StepName} | Duration={Ms}ms | RowsAffected={Rows}",
                            jobExecutionId, result.StepName, stepDurationMs, result.RowsAffected);
                    }

                    if (result.Status == Domain.Enums.StepStatus.Failed)
                    {
                        _logger.LogError(
                            "[{JobExecutionId}] Step {StepName} failed: {Error}. Rolling back.",
                            jobExecutionId, result.StepName, result.ErrorMessage);

                        await SafeRollbackAsync(transaction, jobExecutionId);
                        context.ChangeTracker.Clear();
                        throw new InvalidOperationException(
                            $"RecreateSummaries step '{result.StepName}' failed: {result.ErrorMessage}");
                    }
                }

                // --- Period-lock check (Phase 6) ---
                var periodLocked = await GetPeriodLockedAsync(context, month, year, cancellationToken);

                _logger.LogInformation(
                    "[{JobExecutionId}] Period lock check | Month={Month} | Year={Year} | PeriodLocked={PeriodLocked}",
                    jobExecutionId, month, year, periodLocked);

                if (periodLocked == 0)
                {
                    // Steps 15–17: conditional refresh when period is not locked
                    var refreshSteps = _stepCatalog.BuildRefreshSteps(month);

                    foreach (var step in refreshSteps)
                    {
                        // Step-based heartbeat: touch execution and renew lock before starting step
                        await TouchHeartbeatAsync(jobExecutionId, cancellationToken);

                        using var stepScope = _logger.BeginScope(new Dictionary<string, object?> { ["StepName"] = step.StepName });
                        _logger.LogInformation(
                            "[{JobExecutionId}] Executing refresh step: {StepName}", jobExecutionId, step.StepName);

                        var result = await step.ExecuteAsync(executionContext, cancellationToken);
                        results.Add(result);

                        var stepDurationMs = (int)(result.EndTime - result.StartTime).TotalMilliseconds;
                        _logger.LogInformation(
                            "[{JobExecutionId}] Step {StepName} -> {Status} | RowsAffected={Rows} | Duration={Ms}ms",
                            jobExecutionId, result.StepName, result.Status, result.RowsAffected,
                            stepDurationMs);

                        // Warn if step exceeded 2 minutes (slow-step detection)
                        if (stepDurationMs > 120_000)
                        {
                            _logger.LogInformation(
                                "[{JobExecutionId}] SLOW STEP DETECTED | StepName={StepName} | Duration={Ms}ms | RowsAffected={Rows}",
                                jobExecutionId, result.StepName, stepDurationMs, result.RowsAffected);
                        }

                        if (result.Status == Domain.Enums.StepStatus.Failed)
                        {
                            _logger.LogError(
                                "[{JobExecutionId}] Refresh step {StepName} failed: {Error}. Rolling back.",
                                jobExecutionId, result.StepName, result.ErrorMessage);

                            await SafeRollbackAsync(transaction, jobExecutionId);
                            context.ChangeTracker.Clear();
                            throw new InvalidOperationException(
                                $"RecreateSummaries refresh step '{result.StepName}' failed: {result.ErrorMessage}");
                        }
                    }
                }
                else
                {
                    // Period is locked — skip refresh steps, record as Skipped
                    foreach (var stepName in _stepCatalog.BuildRefreshSteps(month).Select(step => step.StepName))
                    {
                        var skipped = new StepResult(stepName, 0, DateTime.UtcNow, DateTime.UtcNow,
                            StepStatus.Skipped, "Period is locked");
                        results.Add(skipped);
                        _logger.LogInformation("[{JobExecutionId}] Step {StepName} skipped - period is locked.", jobExecutionId, stepName);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
                context.ChangeTracker.Clear();

                _logger.LogInformation("[{JobExecutionId}] Transaction committed. All steps completed.", jobExecutionId);
                completedResults = results;
            }
            catch (Exception) when (results.Count > 0 &&
                                    results[^1].Status != Domain.Enums.StepStatus.Failed)
            {
                // Unexpected exception outside a step failure — attempt rollback
                await SafeRollbackAsync(transaction, jobExecutionId);

                // Prevent replay of tracked Added/Modified entities by other repositories sharing this scoped context.
                context.ChangeTracker.Clear();
                throw;
            }
        });

        return completedResults ?? Array.Empty<StepResult>();
    }

    /// <summary>
    /// Step-based heartbeat: touches execution metadata and renews lock before each step.
    /// Logs success/failure to correlate with step execution for timeout debugging.
    /// </summary>
    private async Task TouchHeartbeatAsync(string jobExecutionId, CancellationToken cancellationToken)
    {
        try
        {
            if (_executionRepository is null || !Guid.TryParse(jobExecutionId, out var jobId))
            {
                throw new InvalidOperationException($"Heartbeat execution repository unavailable or job execution id invalid for job '{Name}' ({jobExecutionId}).");
            }

            var execution = await _executionRepository.GetExecutionByJobExecutionIdAsync(jobId, cancellationToken);

            if (execution is null || execution.JobQueueId == Guid.Empty)
                throw new InvalidOperationException($"Heartbeat execution row missing for job '{Name}' ({jobExecutionId}).");

            var touched = await _executionRepository.TouchRunningExecutionAsync(
                execution.JobQueueId, cancellationToken);

            if (!touched)
                throw new InvalidOperationException($"Heartbeat execution touch failed because job '{Name}' is not in Running state ({jobExecutionId}, JobQueueId={execution.JobQueueId}).");

            if (_lockRepository is null)
                throw new InvalidOperationException($"Heartbeat lock repository is unavailable for job '{Name}' ({jobExecutionId}).");

            // Unlimited timeout (0 seconds) means lock does not expire.
            var renewed = await _lockRepository.TryRenewLockAsync(
                Name, execution.JobQueueId, 0, cancellationToken);

            if (!renewed)
                throw new InvalidOperationException($"Heartbeat lock renewal returned false for job '{Name}' ({jobExecutionId}, JobQueueId={execution.JobQueueId}).");

            _logger.LogDebug(
                "[{JobExecutionId}] Heartbeat: execution touched & lock renewed | JobName={JobName} | JobQueueId={JobQueueId}",
                jobExecutionId, Name, execution.JobQueueId);
        }
        catch (OperationCanceledException)
        {
            // Cancellation during heartbeat is expected during shutdown
            _logger.LogInformation(
                "[{JobExecutionId}] Heartbeat cancelled | JobName={JobName}",
                jobExecutionId, Name);
            throw;
        }
    }

    private async Task<int> GetPeriodLockedAsync(
        BatchJobsDbContext context,
        int month,
        int year,
        CancellationToken cancellationToken)
    {
        var periodLocked = await context.RsTblPeriod
            .AsNoTracking()
            .Where(p => p.EndPeriod == month && p.FpsYear == year)
            .Select(p => p.PeriodLocked)
            .FirstOrDefaultAsync(cancellationToken);

        return periodLocked ?? 1;
    }

    private async Task SafeRollbackAsync(IDbContextTransaction transaction, string jobExecutionId)
    {
        try
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }
        catch (Exception rollbackEx)
        {
            _logger.LogError(rollbackEx, "[{JobExecutionId}] Rollback failed.", jobExecutionId);
            throw new InvalidOperationException($"Rollback failed for RecreateSummaries job ({jobExecutionId}).", rollbackEx);
        }
    }
}