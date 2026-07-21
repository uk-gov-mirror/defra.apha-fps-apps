using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Infrastructure.Repositories.MilestoneUpdateNotifications;

/// <summary>
/// Implementation of <see cref="INotificationSettingsPreflight"/>. Validates the three
/// mabarchive.tbl_settings ids vprojectreports_pmmail's WHERE clause cross-joins and
/// filters on — a missing/duplicated/blank row collapses that view (and therefore the
/// authoritative candidate query) to zero rows, indistinguishable from a genuine empty
/// period without this explicit upfront check (plan section 8.1).
/// </summary>
public sealed class NotificationSettingsPreflight : INotificationSettingsPreflight
{
    private static readonly string[] RequiredSettingIds =
    [
        "PIMS_Project_Report_Name",
        "PIMS_Project_Current_Root",
        "PIMS_Project_Edit_Link"
    ];

    private readonly BatchJobsDbContext _context;
    private readonly ILogger<NotificationSettingsPreflight> _logger;

    public NotificationSettingsPreflight(BatchJobsDbContext context, ILogger<NotificationSettingsPreflight> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task ValidateAsync(CancellationToken cancellationToken)
    {
        var rows = await _context.MaTblSettings
            .Where(s => RequiredSettingIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var problems = new List<string>();

        foreach (var id in RequiredSettingIds)
        {
            var matches = rows.Where(r => r.Id == id).ToList();

            if (matches.Count == 0)
                problems.Add($"'{id}' is missing");
            else if (matches.Count > 1)
                problems.Add($"'{id}' has {matches.Count} rows (expected exactly one)");
            else if (string.IsNullOrWhiteSpace(matches[0].Setting))
                problems.Add($"'{id}' is blank");
        }

        if (problems.Count > 0)
        {
            throw new InvalidOperationException(
                "Milestone notification settings preflight failed against mabarchive.tbl_settings: " +
                string.Join("; ", problems) +
                ". vprojectreports_pmmail depends on these rows and would otherwise silently return zero " +
                "candidates for every project.");
        }

        _logger.LogInformation(
            "Milestone notification settings preflight passed for {SettingCount} required tbl_settings rows",
            RequiredSettingIds.Length);
    }
}
