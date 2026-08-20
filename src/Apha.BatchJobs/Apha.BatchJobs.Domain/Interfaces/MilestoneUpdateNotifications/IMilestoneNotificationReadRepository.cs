using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

namespace Apha.BatchJobs.Domain.Interfaces.MilestoneUpdateNotifications;

/// <summary>
/// Reads milestone notification eligibility from the migrated mabarchive legacy-parity
/// views (plan section 7). <see cref="GetNotificationCandidatesAsync"/> is the single
/// authoritative, unfiltered source every send decision is driven from.
/// <see cref="GetRecipientResolutionIssuesAsync"/> is a reporting-only diagnostic that
/// must never gate or replace it (plan section 7.2).
/// </summary>
public interface IMilestoneNotificationReadRepository
{
    /// <summary>
    /// Queries mabarchive.vprojectreports_pmmilestoneemail directly, with no
    /// disable/email filter applied in SQL — disabled and missing-email rows are
    /// returned so they can be logged and counted (plan section 7.1).
    /// </summary>
    Task<IReadOnlyList<MilestoneNotificationCandidate>> GetNotificationCandidatesAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Diagnostic-only query: the same upstream filters as vprojectreports_pmmail plus
    /// the milestone-existence check, but with the tblprojectmanager join changed from
    /// inner to left, filtered to unmatched managers (plan section 7.2).
    /// </summary>
    Task<IReadOnlyList<RecipientResolutionIssue>> GetRecipientResolutionIssuesAsync(
        CancellationToken cancellationToken);
}
