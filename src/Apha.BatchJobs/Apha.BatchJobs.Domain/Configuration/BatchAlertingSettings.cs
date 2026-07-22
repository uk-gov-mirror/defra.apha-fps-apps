namespace Apha.BatchJobs.Domain.Configuration;

/// <summary>
/// Failure-alerting settings shared across batch processes — not specific to any one job.
/// Consumed today only by <c>EmailNotificationService</c> (currently invoked for MABArchive
/// failures), but the service itself already takes the job name as a parameter, so this section
/// is deliberately not nested under any single job's own settings.
/// </summary>
public sealed class BatchAlertingSettings
{
    /// <summary>
    /// When true, failure notification emails are sent to <see cref="AdminNotificationEmail"/>.
    /// </summary>
    public bool EnableEmailNotifications { get; set; }

    /// <summary>
    /// Email recipient for failure notifications.
    /// </summary>
    public string? AdminNotificationEmail { get; set; }
}
