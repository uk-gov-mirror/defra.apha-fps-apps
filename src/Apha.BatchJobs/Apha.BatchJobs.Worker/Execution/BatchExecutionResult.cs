using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Worker.Lifecycle;

namespace Apha.BatchJobs.Worker.Execution;

/// <summary>
/// Single immutable representation of one worker invocation's outcome — replaces the separate
/// mutable outcome/failureCategory/exitCode/correlation-id variables the old Program.cs kept in
/// sync by hand. <c>IJobOrchestrator.RunAsync</c> only ever returns on success (see the
/// regression test pinning that contract in <c>JobOrchestratorTests</c>), so there is no
/// terminal-status-mapping branch here — a normal return is always <see cref="Success"/>.
/// </summary>
public sealed record BatchExecutionResult
{
    public required BatchRunOutcome Outcome { get; init; }
    public required int ExitCode { get; init; }

    public BatchFailureCategory? FailureCategory { get; init; }
    public string? ErrorType { get; init; }
    public Exception? Exception { get; init; }
    public ExecutionCancellationReason? CancellationReason { get; init; }

    public string? JobName { get; init; }
    public RunMode? RunMode { get; init; }
    public Guid? JobExecutionId { get; init; }
    public Guid? JobQueueId { get; init; }
    public int? ExecutionId { get; init; }

    public static BatchExecutionResult Success(BatchExecutionRequest request, JobExecutionResult jobResult) => new()
    {
        Outcome = BatchRunOutcome.Success,
        ExitCode = BatchExitCodes.Success,
        JobName = request.JobName,
        RunMode = request.RunMode,
        JobExecutionId = request.JobExecutionId,
        JobQueueId = jobResult.JobQueueId,
        ExecutionId = jobResult.ExecutionId
    };

    public static BatchExecutionResult Cancelled(BatchExecutionRequest? request, ExecutionCancellationReason reason) => new()
    {
        Outcome = BatchRunOutcome.Cancelled,
        ExitCode = BatchExitCodes.Cancelled,
        CancellationReason = reason,
        JobName = request?.JobName,
        RunMode = request?.RunMode,
        JobExecutionId = request?.JobExecutionId
    };

    public static BatchExecutionResult Failure(BatchExecutionRequest? request, BatchFailureClassification classification, Exception exception) => new()
    {
        Outcome = BatchRunOutcome.Failure,
        ExitCode = classification.ExitCode,
        FailureCategory = classification.Category,
        ErrorType = classification.ErrorType,
        Exception = exception,
        JobName = request?.JobName,
        RunMode = request?.RunMode,
        JobExecutionId = request?.JobExecutionId
    };
}
