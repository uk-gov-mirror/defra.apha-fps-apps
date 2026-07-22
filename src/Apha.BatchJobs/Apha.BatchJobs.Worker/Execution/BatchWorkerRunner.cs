using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Worker.Lifecycle;
using Apha.BatchJobs.Worker.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Apha.BatchJobs.Worker.Configuration;

namespace Apha.BatchJobs.Worker.Execution;

/// <summary>
/// Coordinates one worker invocation: resolve the request, open a correlation scope, run the
/// orchestrator under a linked cancellation token, map the outcome, and write exactly one final
/// summary. HealthCheck never reaches this type — <c>Program.cs</c> short-circuits before it.
/// </summary>
public sealed class BatchWorkerRunner : IBatchWorkerRunner
{
    private readonly BatchExecutionRequestResolver _requestResolver;
    private readonly IHostApplicationLifetime _hostLifetime;
    private readonly IOptions<BatchRuntimeOptions> _runtimeOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly BatchFailureClassifier _failureClassifier;
    private readonly IBatchRunSummaryWriter _summaryWriter;
    private readonly ILogger<BatchWorkerRunner> _logger;

    public BatchWorkerRunner(
        BatchExecutionRequestResolver requestResolver,
        IHostApplicationLifetime hostLifetime,
        IOptions<BatchRuntimeOptions> runtimeOptions,
        IServiceProvider serviceProvider,
        BatchFailureClassifier failureClassifier,
        IBatchRunSummaryWriter summaryWriter,
        ILogger<BatchWorkerRunner> logger)
    {
        _requestResolver = requestResolver ?? throw new ArgumentNullException(nameof(requestResolver));
        _hostLifetime = hostLifetime ?? throw new ArgumentNullException(nameof(hostLifetime));
        _runtimeOptions = runtimeOptions ?? throw new ArgumentNullException(nameof(runtimeOptions));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _failureClassifier = failureClassifier ?? throw new ArgumentNullException(nameof(failureClassifier));
        _summaryWriter = summaryWriter ?? throw new ArgumentNullException(nameof(summaryWriter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> RunAsync()
    {
        BatchExecutionRequest request;

        try
        {
            request = _requestResolver.Resolve();
        }
        catch (Exception ex)
        {
            // Nothing has logged this yet — request resolution runs before any scope or
            // orchestrator work, so this is the first and only place it gets a marker.
            var classification = _failureClassifier.Classify(ex);
            _logger.LogError(ex, "[{ErrorType}] Batch execution request could not be resolved: {ErrorMessage}", classification.ErrorType, ex.Message);

            var resolutionFailure = BatchExecutionResult.Failure(request: null, classification, ex);
            _summaryWriter.WriteSummary(resolutionFailure, TimeSpan.Zero);
            return resolutionFailure.ExitCode;
        }

        using var correlationScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["JobName"] = request.JobName,
            ["JobExecutionId"] = request.JobExecutionId,
            ["RunMode"] = request.RunMode.ToString(),
            ["RequestedBy"] = request.RequestedBy
        });

        var startedAt = DateTime.UtcNow;

        // Established before the execution scope below (not after) so the entire invocation —
        // scope creation, orchestrator resolution, and execution — is governed by one
        // cancellation boundary, rather than the scope being created under an unbounded token.
        using var cancellationContext = new ExecutionCancellationContext(_hostLifetime, _runtimeOptions.Value.WorkerOverallTimeoutSeconds);

        BatchExecutionResult result;
        try
        {
            await using var executionScope = _serviceProvider.CreateAsyncScope();
            var orchestrator = executionScope.ServiceProvider.GetRequiredService<IJobOrchestrator>();

            var jobResult = await orchestrator.RunAsync(
                request.JobName,
                request.RunMode,
                request.JobExecutionId,
                request.RequestedBy,
                request.RequestedAtUtc,
                cancellationContext.Token);

            result = BatchExecutionResult.Success(request, jobResult);
        }
        catch (OperationCanceledException)
        {
            result = BatchExecutionResult.Cancelled(request, cancellationContext.ClassifyCancellation());
        }
        catch (Exception ex)
        {
            // Already logged once, with the correct marker, by JobOrchestrator.ThrowWithStructuredLog
            // before it re-threw — do not log it again here.
            result = BatchExecutionResult.Failure(request, _failureClassifier.Classify(ex), ex);
        }

        _summaryWriter.WriteSummary(result, DateTime.UtcNow - startedAt);
        return result.ExitCode;
    }
}
