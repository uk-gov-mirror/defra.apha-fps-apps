namespace Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

/// <summary>
/// The (year, latestmonthreleased) pair resolved from mabarchive.vlatestmonthyear —
/// used only by the zero-candidate fallback path (plan section 6.1).
/// </summary>
public sealed record ReportingYear(int Year, int? LatestMonthReleased);
