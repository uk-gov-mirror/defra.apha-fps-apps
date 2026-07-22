namespace Apha.BatchJobs.Worker.Lifecycle;

/// <summary>
/// Why an <see cref="ExecutionCancellationContext"/>'s token was cancelled. Host shutdown takes
/// precedence over timeout when both are signalled, so an ECS SIGTERM is never misreported as
/// the configured job having merely run too long.
/// </summary>
public enum ExecutionCancellationReason
{
    HostShutdown,
    Timeout,
    Unclassified
}
