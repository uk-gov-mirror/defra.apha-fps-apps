using Apha.BatchJobs.Pact.Api.Models;

namespace Apha.BatchJobs.Pact.Api.Services;

/// <summary>
/// Dispatches a validated trigger request to the underlying execution mechanism
/// (e.g. spawning a child process, publishing an EventBridge event, or calling
/// the batch worker directly in a test harness).
/// </summary>
public interface ITriggerDispatcher
{
    /// <summary>
    /// Dispatches the trigger and returns the resulting <see cref="TriggerAttemptRecord"/>
    /// that captures the attempt details for later polling or auditing.
    /// </summary>
    Task<TriggerAttemptRecord> DispatchAsync(
        BatchTriggerRequest request,
        CancellationToken cancellationToken = default);
}
