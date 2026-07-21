using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;

/// <summary>
/// Fallback-only reporting year source. The normal path reads the year off the
/// authoritative candidate query's own rows; this is used solely when that query
/// returns zero rows, since there's then no row to read a year from but the run
/// still needs one to log and put in the CAPS summary (plan section 6.1).
/// </summary>
public interface IReportingYearResolver
{
    /// <summary>
    /// Queries mabarchive.vlatestmonthyear directly for the current (year,
    /// latestmonthreleased) pair.
    /// </summary>
    Task<ReportingYear> ResolveAsync(CancellationToken cancellationToken);
}
