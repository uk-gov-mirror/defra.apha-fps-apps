using Apha.BatchJobs.Pact.Api.Models;

namespace Apha.BatchJobs.Pact.Api.Services;

/// <summary>
/// Persists and retrieves <see cref="TriggerAttemptRecord"/> instances so that
/// trigger attempts survive process restarts or caller polling.
/// Implementations include an in-memory cache and a Redis-backed store.
/// </summary>
public interface ITriggerAttemptStore
{
    /// <summary>Human-readable store identifier used in diagnostics.</summary>
    string StoreName { get; }

    /// <summary>
    /// Persists <paramref name="record"/> and updates the latest-by-job-name pointer.
    /// </summary>
    Task SaveAsync(TriggerAttemptRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the record for <paramref name="jobExecutionId"/>, or <c>null</c> if not found.
    /// </summary>
    Task<TriggerAttemptRecord?> GetByJobExecutionIdAsync(string jobExecutionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the most recently saved record for <paramref name="jobName"/>, or <c>null</c>.
    /// </summary>
    Task<TriggerAttemptRecord?> GetLatestByJobNameAsync(string jobName, CancellationToken cancellationToken = default);
}
