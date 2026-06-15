namespace Apha.BatchJobs.Domain.Enums;

/// <summary>
/// Enumeration of job execution statuses.
/// Lifecycle: Initiated -> Running -> Completed | Failed | Cancelled
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// API accepted the trigger and created the job queue record before publishing to EventBridge.
    /// </summary>
    Initiated = 0,

    /// <summary>
    /// Worker has acquired the lock and is actively executing the job.
    /// </summary>
    Running = 1,

    /// <summary>
    /// Job completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Job failed with an error.
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Job was cancelled before or during execution.
    /// </summary>
    Cancelled = 4
}
