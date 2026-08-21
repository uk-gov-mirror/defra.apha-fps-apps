using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Interfaces.MilestoneUpdateNotifications;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.MilestoneUpdateNotifications.Repositories;

/// <summary>
/// Implementation of <see cref="IMilestoneNotificationReadRepository"/>. Queries the
/// migrated mabarchive legacy-parity view directly (plan section 7) rather than
/// re-deriving its filter logic, so this worker inherits its guaranteed legacy parity.
/// </summary>
public sealed class MilestoneNotificationReadRepository : IMilestoneNotificationReadRepository
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<MilestoneNotificationReadRepository> _logger;

    public MilestoneNotificationReadRepository(BatchJobsDbContext context, ILogger<MilestoneNotificationReadRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MilestoneNotificationCandidate>> GetNotificationCandidatesAsync(
        CancellationToken cancellationToken)
    {
        var rows = await _context.MaVProjectReportsPmMilestoneEmail
            .OrderBy(r => r.MNumber)
            .ThenBy(r => r.ProjectManager)
            .ThenBy(r => r.ParentProject)
            .ToListAsync(cancellationToken);

        var candidates = rows
            .Select(r => new MilestoneNotificationCandidate(
                Year: r.Year,
                ParentProject: r.ParentProject,
                ProjectManager: r.ProjectManager ?? string.Empty,
                MNumber: r.MNumber,
                Email: r.Email,
                IsDisabled: r.Disable,
                EditLink: r.EditLink))
            .ToList();

        _logger.LogInformation(
            "Loaded {CandidateCount} milestone notification candidates from vprojectreports_pmmilestoneemail",
            candidates.Count);

        return candidates;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecipientResolutionIssue>> GetRecipientResolutionIssuesAsync(
        CancellationToken cancellationToken)
    {
        // Mirrors vprojectreports_pmmail's own filters (RadTrackProg, projectstatus,
        // EU_PROG/ZT_Prog exclusion) plus vprojectreports_pmmilestoneemail's milestone
        // EXISTS check, but starts from vcurrent_projectinfo directly and turns the
        // tblprojectmanager join from inner to left — surfacing exactly the rows the
        // authoritative view's inner join makes invisible (plan section 7.2).
        // vcurrent_projectinfo is itself already scoped to vlatestmonthyear.year via its
        // own join to my_tlkpproject, so no separate year filter is needed here.
        const string sql = @"
            SELECT
                vcurrent_projectinfo.year,
                vcurrent_projectinfo.parentproject,
                vcurrent_projectinfo.manager AS projectmanager
            FROM mabarchive.vcurrent_projectinfo
            JOIN mabarchive.tblradtrackprog
                ON tblradtrackprog.program = vcurrent_projectinfo.program
            LEFT JOIN mabarchive.tblprojectmanager
                ON tblprojectmanager.projectmanager = vcurrent_projectinfo.manager
            WHERE tblradtrackprog.radtrackprog = true
              AND vcurrent_projectinfo.projectstatus <> 'Completed'
              AND vcurrent_projectinfo.projectgroup NOT IN ('EU_PROG', 'ZT_Prog')
              AND tblprojectmanager.projectmanager IS NULL
              AND EXISTS (
                    SELECT 1 FROM mabarchive.tblmilestone
                    WHERE tblmilestone.project = vcurrent_projectinfo.parentproject
                      AND vcurrent_projectinfo.year = date_part('year', tblmilestone.datedue)
                  )
            ORDER BY vcurrent_projectinfo.year, vcurrent_projectinfo.parentproject, vcurrent_projectinfo.manager;";

        var conn = (NpgsqlConnection)_context.Database.GetDbConnection();
        var ownsConnection = conn.State != System.Data.ConnectionState.Open;
        if (ownsConnection)
            await conn.OpenAsync(cancellationToken);

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            var issues = new List<RecipientResolutionIssue>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                issues.Add(new RecipientResolutionIssue(
                    Year: reader.GetInt32(0),
                    ParentProject: reader.GetString(1),
                    ProjectManager: reader.IsDBNull(2) ? string.Empty : reader.GetString(2)));
            }

            _logger.LogInformation(
                "Recipient resolution diagnostic found {IssueCount} unresolved manager/project rows",
                issues.Count);

            return issues;
        }
        finally
        {
            if (ownsConnection)
                await conn.CloseAsync();
        }
    }
}
