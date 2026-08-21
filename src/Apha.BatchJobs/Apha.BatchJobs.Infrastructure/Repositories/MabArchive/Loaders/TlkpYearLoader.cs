using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class TlkpYearLoader : MabArchiveExecutionLoaderBase
{
    public TlkpYearLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 16;

    public override string Name => "tlkpyear";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        // Legacy MABArchive read Month from FPSYYYY.dbo.tblDB_Variables, where the physical FPSYYYY
        // database implicitly provided year context. In the consolidated PostgreSQL schema,
        // fps.tbldb_variables has no year dimension and holds obsolete/Planned-year configuration
        // with no active application writer. Use fps.tblcurrentmonth for latestmonthreleased only.

        // Ensure retries do not fail if a prior attempt inserted this year already.
        await context.MaDstTlkpYear
            .Where(x => x.Year == year)
            .ExecuteDeleteAsync(cancellationToken);

        var rows = await context.MaSrcTblCurrentMonth
            .AsNoTracking()
            .Select(x => x.CurrentMonth)
            .ToListAsync(cancellationToken);

        if (rows.Count != 1)
        {
            throw new InvalidOperationException(
                $"Seq 16 TlkpYear: Expected exactly one row in fps.tblcurrentmonth " +
                $"while loading mabarchive.tlkpyear for FPS year {year}, " +
                $"but found {rows.Count}.");
        }

        var currentMonth = rows[0];

        if (currentMonth is < 0 or > 12)
        {
            throw new InvalidOperationException(
                $"Seq 16 TlkpYear: Invalid currentmonth value '{currentMonth}' in " +
                $"fps.tblcurrentmonth while loading mabarchive.tlkpyear " +
                $"for FPS year {year}. Expected a value between 0 and 12.");
        }

        context.MaDstTlkpYear.Add(new MaDstTlkpYear
        {
            Year = year,
            LatestMonthReleased = currentMonth
        });

        return await context.SaveChangesAsync(cancellationToken);
    }
}

