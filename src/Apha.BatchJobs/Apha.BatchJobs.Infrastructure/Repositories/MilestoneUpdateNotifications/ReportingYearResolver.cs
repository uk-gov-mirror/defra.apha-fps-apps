using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Infrastructure.Repositories.MilestoneUpdateNotifications;

/// <summary>
/// Implementation of <see cref="IReportingYearResolver"/>. Queries
/// mabarchive.vlatestmonthyear directly — used only as the zero-candidate fallback
/// (plan section 6.1); the normal path reads the year off the authoritative candidate
/// query's own rows instead.
/// </summary>
public sealed class ReportingYearResolver : IReportingYearResolver
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<ReportingYearResolver> _logger;

    public ReportingYearResolver(BatchJobsDbContext context, ILogger<ReportingYearResolver> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ReportingYear> ResolveAsync(CancellationToken cancellationToken)
    {
        var row = await _context.MaVLatestMonthYear
            .OrderByDescending(r => r.Year)
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            throw new InvalidOperationException(
                "mabarchive.vlatestmonthyear returned no rows — cannot resolve a reporting year for the " +
                "zero-candidate fallback path.");
        }

        _logger.LogInformation(
            "Resolved reporting year {Year} (latest month released {LatestMonthReleased}) from vlatestmonthyear fallback",
            row.Year, row.LatestMonthReleased);

        return new ReportingYear(row.Year, row.LatestMonthReleased);
    }
}
