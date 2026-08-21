using Apha.BatchJobs.Infrastructure.MabArchive.Configuration;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Loaders;

internal sealed class MyStaffLoader : MabArchiveExecutionLoaderBase
{
    private readonly MabArchiveSettings _settings;
    private readonly ILogger<MyStaffLoader> _logger;

    public MyStaffLoader(BatchJobsDbContext context, IOptions<MabArchiveSettings> settings, ILogger<MyStaffLoader> logger)
        : base(context)
    {
        _settings = settings?.Value ?? new MabArchiveSettings();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override int Sequence => 21;

    public override string Name => "my_staff";

    // CR-028: ports the legacy sp_AddMY_Staff ProfitCentre authorization filter:
    //   WGE.WorkGroupGrade IN (SELECT WGGrade FROM WorkgroupGrade WHERE WorkGroup IN
    //     (SELECT WorkGroup FROM WorkGroup WHERE ProfitCentre IN
    //       (SELECT ProfitCentre FROM tblUser_ProfitCentre WHERE User_ID IN
    //         (SELECT User_ID FROM tblUsers WHERE UserName = USER_NAME(1)))))
    // Legacy resolved the identity from the SQL login executing the procedure; a headless worker has no
    // equivalent, so MabArchiveSettings.StaffProfitCentreAuthorizationUserName names the identity to use
    // instead (see that setting's doc comment for the "dbo" default and the evidence behind it).
    // Safety: if the configured identity isn't found in fps.tblusers, or resolves to zero permitted profit
    // centres, this loader logs a warning and loads all staff unfiltered rather than silently archiving zero
    // rows -- a misconfigured identity must never be worse than the pre-CR-028 unfiltered behavior.
    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var permittedWorkGroupGrades = await ResolvePermittedWorkGroupGradesAsync(context, year, cancellationToken);

        var query =
            from wge in context.MaSrcTblWgEmployee.AsNoTracking()
            join e in context.MaSrcTblEmployee.AsNoTracking()
                on wge.SpNumber equals e.SpNumber
            where wge.FpsYear == year
            select new { wge, e };

        if (permittedWorkGroupGrades is not null)
        {
            query = query.Where(x => x.wge.WorkGroupGrade != null && permittedWorkGroupGrades.Contains(x.wge.WorkGroupGrade));
        }

        var rows = await query
            .Select(x => new MaDstMyStaff
            {
                Year = year,
                StaffId = x.wge.PactId,
                Name = (x.e.LastName ?? string.Empty) + ", " + (x.e.FirstName ?? string.Empty),
                WorkGroupGrade = x.wge.WorkGroupGrade,
                Title = x.e.Title,
                PersonStatus = x.wge.PersonStatus,
                PersonClass = x.wge.PersonClass,
                HrsPaid = x.wge.HrsPaid,
                LeaveHours = x.wge.LeaveHours,
                SickSpecial = x.wge.SickSpecial,
                HrsAvail = x.wge.HrsAvail
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        var distinctRows = rows
            .GroupBy(r => new { r.Year, r.StaffId })
            .Select(g => g.First())
            .ToList();

        await context.MaDstMyStaff.AddRangeAsync(distinctRows, cancellationToken);
        var inserted = await context.SaveChangesAsync(cancellationToken);

        if (inserted != distinctRows.Count)
        {
            throw new InvalidOperationException(
                $"Seq 21 MyStaff: Row count mismatch. Expected to insert {distinctRows.Count} rows, " +
                $"but SaveChangesAsync returned {inserted}.");
        }

        var sourceCount = await context.MaSrcTblWgEmployee
            .AsNoTracking()
            .Where(w => w.FpsYear == year)
            .CountAsync(cancellationToken);

        if (distinctRows.Count > sourceCount)
        {
            throw new InvalidOperationException(
                $"Seq 21 MyStaff: Deduped rows {distinctRows.Count} exceed source WgEmployee rows {sourceCount}.");
        }

        var invalidNames = distinctRows.Count(r => string.IsNullOrWhiteSpace(r.Name) || r.Name == ", ");
        if (invalidNames > 0)
        {
            throw new InvalidOperationException(
                $"Seq 21 MyStaff: {invalidNames} rows have invalid Name field (both LastName and FirstName NULL).");
        }

        return inserted;
    }

    /// <summary>
    /// Resolves the set of WorkGroupGrade values authorized for the configured batch identity, mirroring the
    /// legacy nested IN clauses. Returns null (meaning "no filter") if the identity can't be resolved to a
    /// non-empty permitted set, so a misconfiguration degrades to the safe pre-CR-028 unfiltered behavior
    /// instead of silently archiving zero staff rows.
    /// </summary>
    private async Task<HashSet<string>?> ResolvePermittedWorkGroupGradesAsync(
        BatchJobsDbContext context,
        int year,
        CancellationToken cancellationToken)
    {
        var userName = _settings.StaffProfitCentreAuthorizationUserName;
        if (string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }

        var permittedProfitCentres = await (
            from u in context.MaSrcTblUsers.AsNoTracking()
            where u.UserName == userName
            join upc in context.MaSrcTblUserProfitCentre.AsNoTracking()
                on u.UserId equals upc.UserId
            select upc.ProfitCentre)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (permittedProfitCentres.Count == 0)
        {
            _logger.LogWarning(
                "Seq 21 MyStaff: configured batch identity {UserName} was not found in fps.tblusers / fps.tbluser_profitcentre, " +
                "or is not authorized for any profit centre. Falling back to unfiltered staff load for year {Year} (CR-028 safety fallback).",
                userName,
                year);
            return null;
        }

        var permittedWorkGroups = await context.MaSrcWorkGroup
            .AsNoTracking()
            .Where(w => w.FpsYear == year && w.ProfitCentre != null && permittedProfitCentres.Contains(w.ProfitCentre))
            .Select(w => w.WorkGroup)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permittedWorkGroupGrades = await context.MaSrcWorkGroupGrade
            .AsNoTracking()
            .Where(g => g.FpsYear == year && g.WorkGroup != null && permittedWorkGroups.Contains(g.WorkGroup))
            .Select(g => g.WgGrade)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (permittedWorkGroupGrades.Count == 0)
        {
            _logger.LogWarning(
                "Seq 21 MyStaff: batch identity {UserName} resolved to {ProfitCentreCount} permitted profit centre(s) " +
                "but zero permitted WorkGroupGrade values for year {Year} (no matching fps.workgroup/fps.workgroupgrade rows). " +
                "Falling back to unfiltered staff load (CR-028 safety fallback).",
                userName,
                permittedProfitCentres.Count,
                year);
            return null;
        }

        return permittedWorkGroupGrades.ToHashSet(StringComparer.Ordinal);
    }
}
