namespace Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

/// <summary>
/// One recipient's grouped notification (plan section 9.1) — every classified candidate
/// is grouped, not only sendable ones, so disabled/missing-email recipients still
/// produce a group for audit purposes (plan section 11.3). <see cref="RecipientId"/> is
/// the composite grouping/duplicate-prevention key; <see cref="DurablePersonId"/> is a
/// separate, never-grouped-on reporting identifier.
/// </summary>
public sealed record NotificationGroup(
    string RecipientId,
    string? DurablePersonId,
    string ProjectManager,
    string? MNumber,
    string? Email,
    bool IsDisabled,
    IReadOnlyList<NotificationProjectLink> Projects);
