using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Apha.BatchJobs.Application;

/// <summary>
/// Implements the full execution lifecycle for a batch job:
/// generate JobQueueId -> acquire lock -> record start -> execute -> record result -> release lock.
/// </summary>
public sealed class JobOrchestrator : IJobOrchestrator
{
    private readonly IBatchJobFactory _factory;
    private readonly IBatchLockRepository _lockRepository;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobOrchestrator> _logger;
    private readonly int _lockTimeoutSeconds;
    private readonly int _retryAttempts;
    private readonly int _retryDelaySeconds;
    private readonly int _maxRetryDurationSeconds;
    private readonly int _defaultJobTimeoutSeconds;
    private readonly int _cancellationPollIntervalSeconds;
    private readonly Dictionary<string, int> _jobTimeoutOverridesSeconds;

    /// <summary>Default lock timeout in seconds when configuration is missing/invalid.</summary>
    private const int DefaultLockTimeoutSeconds = 3600;

    /// <summary>Default maximum retry duration in seconds.</summary>
    private const int DefaultMaxRetryDurationSeconds = 300;

    /// <summary>Default cancellation monitor poll interval in seconds.</summary>
    private const int DefaultCancellationPollIntervalSeconds = 2;

    /// <summary>
    /// Initializes a new instance of <see cref="JobOrchestrator"/>.
    /// </summary>
    public JobOrchestrator(
        IBatchJobFactory factory,
        IBatchLockRepository lockRepository,
        IJobExecutionRepository executionRepository,
        IServiceScopeFactory scopeFactory,
        IOptions<BatchJobSettings> settings,
        ILogger<JobOrchestrator> logger)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _lockRepository = lockRepository ?? throw new ArgumentNullException(nameof(lockRepository));
        _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _lockTimeoutSeconds = settings?.Value.LockTimeoutSeconds > 0
            ? settings.Value.LockTimeoutSeconds
            : DefaultLockTimeoutSeconds;
        _retryAttempts = settings?.Value.RetryAttempts >= 0
            ? settings.Value.RetryAttempts
            : 0;
        _retryDelaySeconds = settings?.Value.RetryDelaySeconds >= 0
            ? settings.Value.RetryDelaySeconds
            : 1;
        _maxRetryDurationSeconds = settings?.Value.MaxRetryDurationSeconds > 0
            ? settings.Value.MaxRetryDurationSeconds
            : DefaultMaxRetryDurationSeconds;
        _defaultJobTimeoutSeconds = settings?.Value.JobTimeout > 0
            ? settings.Value.JobTimeout
            : DefaultLockTimeoutSeconds;
        _cancellationPollIntervalSeconds = settings?.Value.CancellationPollIntervalSeconds > 0
            ? settings.Value.CancellationPollIntervalSeconds
            : DefaultCancellationPollIntervalSeconds;
        _jobTimeoutOverridesSeconds = settings?.Value.JobTimeoutOverridesSeconds is { Count: > 0 }
            ? settings.Value.JobTimeoutOverridesSeconds
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && kv.Value > 0)
                .ToDictionary(kv => kv.Key.Trim(), kv => kv.Value, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<JobExecutionResult> RunAsync(
        string jobName,
        RunMode runMode,
        Guid jobExecutionId,
        string userId,
        DateTime? requestedAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        var startedAt = DateTime.UtcNow;
        
        // Attempt to acquire lock using a temporary jobQueueId; fetch real one after lock succeeds
        var tempJobQueueId = Guid.NewGuid();
        
        _logger.LogInformation("Acquiring execution lock for '{JobName}' | JobExecutionId={JobExecutionId}...", jobName, jobExecutionId);
        var lockAcquired = await _lockRepository.TryAcquireLockAsync(
            jobName, tempJobQueueId, _lockTimeoutSeconds, cancellationToken);

        if (!lockAcquired)
        {
            _logger.LogError(
                "Job '{JobName}' is already running (lock held by another process). Cannot start execution | JobExecutionId={JobExecutionId}",
                jobName, jobExecutionId);
            throw new InvalidOperationException(
                $"Job '{jobName}' is already running and cannot accept another execution at this time.");
        }

        // Fetch the Initiated record created by API layer
        var existingExecution = await _executionRepository.GetExecutionByJobExecutionIdAsync(jobExecutionId, cancellationToken);
        if (existingExecution == null)
        {
            _logger.LogError(
                "No Initiated record found for JobExecutionId={JobExecutionId}. Worker cannot proceed without pre-created record from API.",
                jobExecutionId);
            throw new InvalidOperationException(
                $"No Initiated job record found for execution {jobExecutionId}. This indicates the API did not properly create the job record.");
        }

        var jobQueueId = existingExecution.JobQueueId;
        
        using var runScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["JobExecutionId"] = jobExecutionId,
            ["JobQueueId"] = jobQueueId,
            ["JobName"] = jobName,
            ["RunMode"] = runMode.ToString(),
            ["UserId"] = userId
        });

        _logger.LogInformation("Lock acquired for '{JobName}' | JobQueueId={JobQueueId} | Mode={RunMode}", jobName, jobQueueId, runMode);

        // Step 2 — Create execution record (Started)
        var record = new JobExecutionRecord
        {
            ExecutionId = 0,   // DB assigns real ID on insert
            JobName = jobName,
            JobExecutionId = jobExecutionId,
            JobQueueId = jobQueueId,
            UserId = userId,
            JobType = JobType.Unknown,
            RunMode = runMode,
            Status = JobStatus.Running,
            StartedAt = startedAt,
            RequestedAtUtc = requestedAtUtc
        };

        int executionId = 0;
        IDisposable? executionScope = null;
        try
        {
            executionId = await _executionRepository.CreateExecutionRecordAsync(record, cancellationToken);
            record.ExecutionId = executionId;
            if (executionId > 0)
            {
                executionScope = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["ExecutionId"] = executionId
                });
                _logger.LogInformation("Execution record created | ExecutionId={ExecutionId}", executionId);
            }
            else
            {
                _logger.LogInformation("Execution record created | JobQueueId={JobQueueId}", jobQueueId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not write execution start record — continuing without tracking");
        }

        // Step 3 — Execute the job
        IBatchJob? job = null;
        Exception? jobException = null;
        var retryStartedAt = DateTime.UtcNow;

        try
        {
            job = _factory.Create(jobName);
            var runtimeTimeoutSeconds = ResolveRuntimeTimeoutSeconds(job);

            _logger.LogInformation(
                "Runtime timeout policy resolved | JobName={JobName} | RuntimeTimeoutSeconds={RuntimeTimeoutSeconds}",
                jobName,
                runtimeTimeoutSeconds?.ToString() ?? "none");

            var totalAttempts = _retryAttempts + 1;
            for (var attempt = 1; attempt <= totalAttempts; attempt++)
            {
                using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                if (runtimeTimeoutSeconds.HasValue)
                {
                    attemptCts.CancelAfter(TimeSpan.FromSeconds(runtimeTimeoutSeconds.Value));
                }

                var cancellationObserved = false;
                var monitorTask = MonitorCancellationAsync(
                    jobExecutionId,
                    jobName,
                    userId,
                    attempt,
                    totalAttempts,
                    attemptCts,
                    () => cancellationObserved = true,
                    cancellationToken);

                var attemptToken = attemptCts.Token;

                try
                {
                    await ThrowIfCancellationRequestedAsync(jobExecutionId, jobName, attempt, totalAttempts, cancellationToken);

                    _logger.LogInformation(
                        "Executing job '{JobName}' | Attempt={Attempt}/{TotalAttempts}",
                        jobName,
                        attempt,
                        totalAttempts);

                    await job.ExecuteAsync(attemptToken);
                    _logger.LogInformation(
                        "Job '{JobName}' completed successfully | Attempt={Attempt}/{TotalAttempts}",
                        jobName,
                        attempt,
                        totalAttempts);

                    jobException = null;
                    break;
                }
                catch (OperationCanceledException ex)
                {
                    if (cancellationObserved)
                    {
                        jobException = ex;
                        _logger.LogWarning(ex,
                            "Job '{JobName}' was cancelled via durable request | Attempt={Attempt}/{TotalAttempts}",
                            jobName,
                            attempt,
                            totalAttempts);
                        break;
                    }

                    if (attemptCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                    {
                        var timeoutException = new TimeoutException(
                            $"Job '{jobName}' exceeded runtime timeout of {runtimeTimeoutSeconds} seconds.",
                            ex);
                        jobException = timeoutException;
                        _logger.LogError(timeoutException,
                            "Job '{JobName}' exceeded runtime timeout and was stopped | Attempt={Attempt}/{TotalAttempts} | RuntimeTimeoutSeconds={RuntimeTimeoutSeconds}",
                            jobName,
                            attempt,
                            totalAttempts,
                            runtimeTimeoutSeconds);
                        break;
                    }

                    jobException = ex;
                    _logger.LogWarning(ex,
                        "Job '{JobName}' was cancelled | Attempt={Attempt}/{TotalAttempts}",
                        jobName,
                        attempt,
                        totalAttempts);
                    break;
                }
                catch (Exception ex)
                {
                    jobException = ex;

                    // Classify whether this exception is retryable (with logging)
                    var isRetryable = IsRetryable(ex);
                    var exceptionClassification = isRetryable ? "TransientRetryable" : "NonRetryable";
                    _logger.LogInformation(
                        "Job exception classification | Attempt={Attempt}/{TotalAttempts} | ExceptionType={ExceptionType} | Classification={ExceptionClassification}",
                        attempt,
                        totalAttempts,
                        ex.GetType().Name,
                        exceptionClassification);

                    // Non-retryable: config, validation, and business-rule errors must not be retried.
                    if (!isRetryable)
                    {
                        _logger.LogError(ex,
                            "Job '{JobName}' failed with non-retryable exception | Attempt={Attempt}/{TotalAttempts} | ExceptionType={ExceptionType} | ErrorMessage={ErrorMessage} | JobQueueId={JobQueueId}",
                            jobName, attempt, totalAttempts, ex.GetType().Name, ex.Message, jobQueueId);
                        break;
                    }

                    var canRetry = attempt < totalAttempts;

                    if (!canRetry)
                    {
                        _logger.LogError(ex,
                            "Job '{JobName}' failed after retries exhausted | Attempt={Attempt}/{TotalAttempts} | ExceptionType={ExceptionType} | ErrorMessage={ErrorMessage} | JobQueueId={JobQueueId}",
                            jobName, attempt, totalAttempts, ex.GetType().Name, ex.Message, jobQueueId);
                        break;
                    }

                    await ThrowIfCancellationRequestedAsync(jobExecutionId, jobName, attempt, totalAttempts, cancellationToken);

                    // Check if total retry duration would be exceeded
                    var elapsedRetrySeconds = (DateTime.UtcNow - retryStartedAt).TotalSeconds;
                    if (elapsedRetrySeconds >= _maxRetryDurationSeconds)
                    {
                        _logger.LogError(ex,
                            "Job '{JobName}' retry duration capped | Attempt={Attempt}/{TotalAttempts} | ElapsedRetrySeconds={ElapsedSeconds} | MaxRetrySeconds={MaxSeconds} | JobQueueId={JobQueueId}",
                            jobName, attempt, totalAttempts, (int)elapsedRetrySeconds, _maxRetryDurationSeconds, jobQueueId);
                        break;
                    }

                    // Calculate retry delay with jitter
                    var basedelaySeconds = _retryDelaySeconds;
                    var jitterSeconds = new Random().Next(0, Math.Max(1, basedelaySeconds / 2)); // Up to 50% jitter
                    var finalDelaySeconds = basedelaySeconds + jitterSeconds;

                    _logger.LogWarning(ex,
                        "Job '{JobName}' failed | Attempt={Attempt}/{TotalAttempts} | ExceptionType={ExceptionType} | Classification={ExceptionClassification} | Retrying after {RetryDelaySeconds}s (+{JitterSeconds}s jitter) | JobQueueId={JobQueueId}",
                        jobName,
                        attempt,
                        totalAttempts,
                        ex.GetType().Name,
                        exceptionClassification,
                        basedelaySeconds,
                        jitterSeconds,
                        jobQueueId);

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(finalDelaySeconds), cancellationToken);
                    }
                    catch (OperationCanceledException cancelDelayEx)
                    {
                        jobException = cancelDelayEx;
                        _logger.LogWarning(cancelDelayEx,
                            "Retry delay cancelled for '{JobName}' | Attempt={Attempt}/{TotalAttempts}",
                            jobName,
                            attempt,
                            totalAttempts);
                        break;
                    }
                }
                finally
                {
                    attemptCts.Cancel();
                    try
                    {
                        await monitorTask;
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when attempt lifecycle ends.
                    }
                }
            }
        }
        finally
        {
            executionScope?.Dispose();

            // Step 4 — Update execution record (Completed or Failed)
            var completedAt = DateTime.UtcNow;
            var duration = completedAt - startedAt;
            var finalStatus = jobException switch
            {
                null => JobStatus.Completed,
                OperationCanceledException => JobStatus.Cancelled,
                _ => JobStatus.Failed
            };

            record.Status = finalStatus;
            record.CompletedAt = completedAt;
            record.DurationSeconds = (int)duration.TotalSeconds;
            record.ErrorMessage = jobException?.Message;
            record.StackTrace = jobException?.StackTrace;

            try
            {
                await _executionRepository.UpdateExecutionRecordAsync(record, CancellationToken.None);
                _logger.LogInformation(
                    "Execution record updated | Status={Status} | Duration={DurationSeconds}s",
                    finalStatus, record.DurationSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not write execution completion record — job result may not be persisted");
            }

            // Step 5 — Release lock (always)
            try
            {
                await _lockRepository.ReleaseLockAsync(jobName, jobQueueId, CancellationToken.None);
                _logger.LogInformation("Lock released for '{JobName}' | JobQueueId={JobQueueId}", jobName, jobQueueId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not release lock for '{JobName}' | JobQueueId={JobQueueId} — lock will expire after {Timeout}s",
                    jobName, jobQueueId, _lockTimeoutSeconds);
            }
        }

        var finalDuration = DateTime.UtcNow - startedAt;
        var status = jobException switch
        {
            null => JobStatus.Completed,
            OperationCanceledException => JobStatus.Cancelled,
            _ => JobStatus.Failed
        };

        _logger.LogInformation(
            "--- Orchestrator: '{JobName}' finished | Status={Status} | Duration={Duration:mm\\:ss\\.fff} | JobQueueId={JobQueueId}",
            jobName, status, finalDuration, jobQueueId);

        if (jobException is OperationCanceledException cancelEx)
            throw cancelEx;

        if (jobException != null)
            throw jobException;

        return new JobExecutionResult(jobQueueId, jobName, status, finalDuration, executionId);
    }

    private async Task ThrowIfCancellationRequestedAsync(
        Guid jobExecutionId,
        string jobName,
        int attempt,
        int totalAttempts,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var scope = _scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IJobExecutionRepository>();

        var cancellationRequested = await repo.IsCancellationRequestedAsync(jobExecutionId, cancellationToken);
        if (!cancellationRequested)
            return;

        await repo.MarkCancellationConsumedAsync(jobExecutionId, "orchestrator-checkpoint", CancellationToken.None);

        _logger.LogWarning(
            "Cancellation checkpoint consumed | JobName={JobName} | JobExecutionId={JobExecutionId} | Attempt={Attempt}/{TotalAttempts}",
            jobName,
            jobExecutionId,
            attempt,
            totalAttempts);

        throw new OperationCanceledException(
            $"Cancellation requested for job execution '{jobExecutionId}'.",
            cancellationToken);
    }

    private async Task MonitorCancellationAsync(
        Guid jobExecutionId,
        string jobName,
        string userId,
        int attempt,
        int totalAttempts,
        CancellationTokenSource attemptCts,
        Action onCancellationObserved,
        CancellationToken orchestratorCancellationToken)
    {
        var pollDelay = TimeSpan.FromSeconds(_cancellationPollIntervalSeconds);

        while (!attemptCts.IsCancellationRequested && !orchestratorCancellationToken.IsCancellationRequested)
        {
            bool requested;
            await using (var scope = _scopeFactory.CreateAsyncScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IJobExecutionRepository>();
                requested = await repo.IsCancellationRequestedAsync(jobExecutionId, orchestratorCancellationToken);
                if (requested)
                    await repo.MarkCancellationConsumedAsync(jobExecutionId, "orchestrator-monitor", CancellationToken.None);
            }

            if (requested)
            {
                onCancellationObserved();

                _logger.LogWarning(
                    "Cancellation monitor observed request | JobName={JobName} | JobExecutionId={JobExecutionId} | Attempt={Attempt}/{TotalAttempts} | UserId={UserId}",
                    jobName,
                    jobExecutionId,
                    attempt,
                    totalAttempts,
                    userId);

                attemptCts.Cancel();
                return;
            }

            await Task.Delay(pollDelay, orchestratorCancellationToken);
        }
    }


    /// <summary>
    /// Returns false for exceptions that must never be retried:
    /// configuration errors, validation failures, and business-rule violations.
    /// Only explicit transient infrastructure failures (timeouts, connectivity) are retryable.
    /// Default is now false to avoid overly broad retry surface.
    /// </summary>
    public static bool IsRetryable(Exception ex) => ex switch
    {
        // Never retry on cancellation or operational errors
        OperationCanceledException => false,           // cancellation: never retry
        
        // Never retry on programming/configuration errors
        ArgumentException => false,                    // programming / validation error
        InvalidOperationException => false,            // configuration / business-rule error
        NotSupportedException => false,                // permanent capability error
        NotImplementedException => false,              // permanent / incomplete feature
        
        // Retry on transient infrastructure failures (explicit list)
        TimeoutException => true,                      // transient network/DB timeout
        NpgsqlException => true,                       // PostgreSQL-specific error (retry safe)
        DbUpdateException => true,                     // EF/database transient error
        HttpRequestException => true,                  // Network/HTTP transient error
        System.Net.Sockets.SocketException => true,   // Network socket failure
        IOException => true,                           // Transient I/O error
        
        // Default: do NOT retry (fail-safe: assume permanent unless proven transient)
        _ => false
    };

    private int? ResolveRuntimeTimeoutSeconds(IBatchJob job)
    {
        if (_jobTimeoutOverridesSeconds.TryGetValue(job.Name, out var overrideSeconds) && overrideSeconds > 0)
        {
            return overrideSeconds;
        }

        if (job.MaxExecutionSeconds.HasValue && job.MaxExecutionSeconds.Value > 0)
        {
            return job.MaxExecutionSeconds.Value;
        }

        return _defaultJobTimeoutSeconds > 0 ? _defaultJobTimeoutSeconds : null;
    }
}
