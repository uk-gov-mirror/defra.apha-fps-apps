using Apha.BatchJobs.Domain.Enums;

namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// Scoped holder for the current worker execution's resolved identity and parameters.
/// <c>JobOrchestrator</c> calls <see cref="Initialize"/> exactly once per execution, immediately
/// before invoking the resolved <c>IBatchJob</c>'s <c>ExecuteAsync</c>, using values it has
/// already resolved end-to-end: <see cref="JobExecutionId"/>, <see cref="JobName"/>,
/// <see cref="RunMode"/>, <see cref="RequestedBy"/>, and <see cref="ParametersJson"/> originate
/// from the Worker's <c>BatchExecutionRequest</c>; <see cref="JobQueueId"/> comes from the
/// execution record JobOrchestrator just validated/created.
///
/// Jobs that need any of these values should read them from here instead of re-parsing
/// environment variables or issuing a second repository lookup — environment access belongs
/// exclusively to <c>BatchExecutionRequestResolver</c>.
/// </summary>
public interface ICurrentJobExecutionContext
{
    Guid JobExecutionId { get; }
    Guid JobQueueId { get; }
    string JobName { get; }
    RunMode RunMode { get; }
    string RequestedBy { get; }
    string? ParametersJson { get; }

    /// <summary>Populates the context. Called once per execution, before the job runs.</summary>
    void Initialize(
        Guid jobExecutionId,
        Guid jobQueueId,
        string jobName,
        RunMode runMode,
        string requestedBy,
        string? parametersJson);
}
