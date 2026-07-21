using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;

/// <summary>
/// Pure, no-I/O grouping of candidate rows into per-recipient notification groups
/// (plan section 9.1). Groups every classified candidate, not only sendable ones,
/// so disabled/missing-email recipients still produce an auditable group
/// (plan section 11.3).
/// </summary>
public interface INotificationGroupingService
{
    IReadOnlyList<NotificationGroup> GroupCandidates(
        IReadOnlyList<MilestoneNotificationCandidate> candidates);
}
