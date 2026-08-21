using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Grouping;

/// <summary>
/// Implementation of <see cref="INotificationGroupingService"/>. Groups every
/// classified candidate — including disabled and missing-email ones — by the composite
/// recipient identity, deduplicating project links by project identity (Year +
/// ParentProject) rather than by comparing raw EditLink text, and ordering links by
/// project code ascending (plan section 9.1).
/// </summary>
public sealed class NotificationGroupingService : INotificationGroupingService
{
    private readonly IRecipientIdentityBuilder _recipientIdentityBuilder;

    public NotificationGroupingService(IRecipientIdentityBuilder recipientIdentityBuilder)
    {
        _recipientIdentityBuilder = recipientIdentityBuilder ?? throw new ArgumentNullException(nameof(recipientIdentityBuilder));
    }

    /// <inheritdoc />
    public IReadOnlyList<NotificationGroup> GroupCandidates(
        IReadOnlyList<MilestoneNotificationCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(candidates);

        return candidates
            .GroupBy(c => _recipientIdentityBuilder.BuildRecipientId(c.MNumber, c.ProjectManager, c.Email))
            .Select(group =>
            {
                var first = group.First();

                var projects = group
                    .GroupBy(c => (c.Year, c.ParentProject))
                    .Select(p => new NotificationProjectLink(p.Key.Year, p.Key.ParentProject, p.First().EditLink))
                    .OrderBy(p => p.ParentProject, StringComparer.Ordinal)
                    .ToList();

                return new NotificationGroup(
                    RecipientId: group.Key,
                    DurablePersonId: _recipientIdentityBuilder.BuildDurablePersonId(first.MNumber),
                    ProjectManager: first.ProjectManager,
                    MNumber: first.MNumber,
                    Email: first.Email,
                    IsDisabled: first.IsDisabled,
                    Projects: projects);
            })
            .ToList();
    }
}
