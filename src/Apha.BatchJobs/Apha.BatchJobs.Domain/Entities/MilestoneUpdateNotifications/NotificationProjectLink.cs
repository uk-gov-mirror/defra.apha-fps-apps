namespace Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

/// <summary>
/// One distinct project within a recipient's notification group, deduplicated by
/// project identity (Year + ParentProject), not by comparing raw EditLink text
/// (plan section 9.1).
/// </summary>
public sealed record NotificationProjectLink(int Year, string ParentProject, string? EditLink);
