using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries.Execution;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;

/// <summary>
/// RecreateSummaries batch job. Rebuilds monthly FPS summary/calculation data by executing
/// 14 ordered SQL steps and optionally refreshing period snapshot tables when the period is
/// unlocked, all inside one transaction owned by this job.
///
/// Replaces the legacy SQL Server <c>sp_RecreateSummaries</c> orchestration procedure.
///
/// Lock lifecycle, retry, and final status are owned exclusively by <see cref="JobOrchestrator"/>.
/// This job must not acquire or release the distributed lock, and performs no heartbeat or lock
/// renewal of its own â€” that is a generic capability to be designed separately.
/// </summary>

public sealed class RecreateSummaryJob : IBatchJob
{
    private readonly IRecreateSummariesExecutionRunner _executionRunner;
    private readonly IRecreateSummariesStepCatalog _stepCatalog;
    private readonly IRecreateSummariesContext _jobContext;
    private readonly ICorrelationContextAccessor _correlationService;
    private readonly ILogger<RecreateSummaryJob> _logger;

    /// <summary>Canonical job name.</summary>
    public string Name => "RecreateSummary";

    /// <summary>
    /// Idempotency strategy: full delete-and-rebuild per month with a single wrapping transaction.
    /// </summary>
    public string IdempotencyStrategy => "DeleteAndRebuildWithSingleTransaction";

    /// <summary>
    /// RecreateSummaries is a manually triggered job â€” no schedule expression.
    /// </summary>
    public string? ScheduleExpression => null;

    /// <summary>Human-readable schedule description.</summary>
    public string? ScheduleDescription => "Manually triggered per FPS period month";

    /// <summary>Maximum execution timeout: 60 minutes.</summary>
    public int? MaxExecutionSeconds => 3600;

    /// <summary>
    /// Initializes a new instance of <see cref="RecreateSummaryJob"/>.
    /// </summary>
    public RecreateSummaryJob(
        IRecreateSummariesExecutionRunner executionRunner,
        IRecreateSummariesStepCatalog stepCatalog,
        IRecreateSummariesContext jobContext,
        ICorrelationContextAccessor correlationService,
        ILogger<RecreateSummaryJob> logger)
    {
        _executionRunner = executionRunner ?? throw new ArgumentNullException(nameof(executionRunner));
        _stepCatalog = stepCatalog ?? throw new ArgumentNullException(nameof(stepCatalog));
        _jobContext = jobContext ?? throw new ArgumentNullException(nameof(jobContext));
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
            var results = await _executionRunner.ExecuteAsync(
                jobExecutionId,
                _jobContext.Month,
                _jobContext.Year,
                _jobContext.TriggeredBy,
                _stepCatalog,
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
}
