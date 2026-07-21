namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;

/// <summary>
/// Validates that mabarchive.tbl_settings has exactly one non-blank row for each id
/// vprojectreports_pmmail's WHERE clause depends on, before the authoritative candidate
/// query ever runs. A missing row collapses that view to zero rows for every project,
/// silently — indistinguishable from a genuine empty period without this check
/// (plan section 8.1).
/// </summary>
public interface INotificationSettingsPreflight
{
    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if any required setting is
    /// missing, duplicated, or blank.
    /// </summary>
    Task ValidateAsync(CancellationToken cancellationToken);
}
