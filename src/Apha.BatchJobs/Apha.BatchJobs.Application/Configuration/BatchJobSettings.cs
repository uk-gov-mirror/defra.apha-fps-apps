namespace Apha.BatchJobs.Application.Configuration;

/// <summary>
/// Batch job runtime settings.
/// </summary>
public sealed class BatchJobSettings
{
    /// <summary>
    /// Job execution timeout in seconds.
    /// </summary>
    public int JobTimeout { get; set; } = 0;

    /// <summary>
    /// Optional per-job runtime timeout overrides in seconds.
    /// Keys are job names (for example: RecreateSummaries, MABArchive).
    /// </summary>
    public Dictionary<string, int> JobTimeoutOverridesSeconds { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Number of retry attempts for failed jobs.
    /// Retry policy wiring is planned in a future story.
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Delay in seconds between retry attempts.
    /// Retry policy wiring is planned in a future story.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 60;

    /// <summary>
    /// Maximum total duration (in seconds) for all retry attempts combined.
    /// Prevents long-running containers from exceeding ECS task timeout.
    /// </summary>
    public int MaxRetryDurationSeconds { get; set; } = 0;

    /// <summary>
    /// Lock acquisition timeout in seconds for distributed locking.
    /// </summary>
    public int LockTimeoutSeconds { get; set; } = 0;

    /// <summary>
    /// Heartbeat update interval in seconds while a job is running.
    /// </summary>
    public int HeartbeatIntervalSeconds { get; set; } = 30;

}
