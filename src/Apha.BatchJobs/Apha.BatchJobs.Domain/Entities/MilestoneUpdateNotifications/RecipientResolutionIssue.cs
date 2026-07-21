namespace Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

/// <summary>
/// A project/manager pairing excluded before vprojectreports_pmmail's inner join to
/// tblprojectmanager could ever produce a row (plan section 7.2) — invisible to
/// <c>GetNotificationCandidatesAsync</c> by construction. Reporting-only; must never
/// gate or filter the authoritative candidate query.
/// </summary>
public sealed record RecipientResolutionIssue(
    int Year,
    string ParentProject,
    string ProjectManager);
