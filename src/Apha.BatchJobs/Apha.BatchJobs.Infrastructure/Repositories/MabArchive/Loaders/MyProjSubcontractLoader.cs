using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyProjSubcontractLoader : MabArchiveExecutionLoaderBase
{
    public MyProjSubcontractLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 8;

    public override string Name => "my_proj_subcontract";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        // month is nullable on both source (fps.proj_subcontract) and destination
        // (mabarchive.my_proj_subcontract; not part of pk_my_proj_subcontract), so null-month rows are
        // archived exactly as legacy sp_AddMY_Proj_SubContract did. The "AND p.month IS NOT NULL" filter
        // previously here was added to work around an EF materialization crash (GetDouble on NULL) in an
        // earlier, non-raw-SQL version of this loader; that crash risk doesn't apply to this raw INSERT ...
        // SELECT, so the filter was vestigial and has been removed (CR-028).
        return await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO mabarchive.my_proj_subcontract
            (
                year,
                subcontcounter,
                project,
                testjob,
                month,
                amount,
                workgroup,
                acctcode,
                supplier,
                description,
                suppliernumber,
                dailyrate,
                animaldays
            )
            SELECT
                {year},
                p.subcontcounter,
                p.project,
                p.testjob,
                p.month,
                p.amount,
                p.workgroup,
                p.acctcode,
                p.supplier,
                p.description,
                p.suppliernumber,
                p.dailyrate,
                p.animaldays
            FROM fps.proj_subcontract p
            WHERE p.fpsyear = {year};
        ", cancellationToken);
    }
}



