namespace Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

/// <summary>
/// One row from mabarchive.vprojectreports_pmmilestoneemail (plan section 7) — the
/// authoritative, unfiltered source for milestone notification eligibility. Disabled
/// and missing-email rows are included deliberately so they can be logged/counted
/// rather than silently dropped (plan section 7.1).
/// </summary>
public sealed record MilestoneNotificationCandidate(
    int Year,
    string ParentProject,
    string ProjectManager,
    string? MNumber,
    string? Email,
    bool IsDisabled,
    string? EditLink);
