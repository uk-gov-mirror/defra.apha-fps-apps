using System.ComponentModel.DataAnnotations;

namespace Apha.BatchJobs.Worker.Configuration;

/// <summary>
/// Validated worker-level runtime settings that are new to the startup refactor:
/// the graceful shutdown window and the overall wall-clock timeout for one job execution.
/// Deliberately does not include <c>DbCommandTimeoutSeconds</c>/<c>LockTimeoutSeconds</c> —
/// those stay owned by <see cref="Apha.BatchJobs.Domain.Configuration.BatchJobSettings"/> and
/// <c>ServiceCollectionSetup</c>, where <c>0</c> is a load-bearing "unbounded" sentinel that a
/// blanket "must be &gt; 0" rule here would break.
/// </summary>
public sealed class BatchRuntimeOptions
{
    public const string SectionName = "BatchJobs";

    /// <summary>
    /// Seconds allowed for <c>host.StopAsync()</c> to complete after a job finishes or the host
    /// receives a stop signal, before the ECS SIGTERM→forced-stop deadline is reached.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int GracefulShutdownWindowSeconds { get; init; } = 25;

    /// <summary>
    /// Overall wall-clock timeout, in seconds, for one <c>IJobOrchestrator.RunAsync</c> call —
    /// enforced by <see cref="Apha.BatchJobs.Worker.Lifecycle.ExecutionCancellationContext"/>.
    /// Distinct from <c>BatchJobs:JobTimeout</c> (the existing per-attempt timeout consumed
    /// inside the orchestrator, where <c>0</c> means unbounded) — this is the new, outer bound.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int WorkerOverallTimeoutSeconds { get; init; } = 3600;
}
