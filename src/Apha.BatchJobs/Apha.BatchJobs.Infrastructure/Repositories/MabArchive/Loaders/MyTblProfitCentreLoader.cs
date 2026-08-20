using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTblProfitCentreLoader : MabArchiveExecutionLoaderBase
{
    internal MyTblProfitCentreLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 19;

    public override string Name => "my_tblprofitcentre";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcTblkpProfitCentre
            .AsNoTracking()
            .Select(p => new MaDstMyTblProfitCentre
            {
                Year = year,
                ProfitCentre = p.ProfitCentre,
                ProfitCentreName = p.ProfitCentreName,
                Division = p.Division,
                ContTarget = p.ContTarget,
                ProfitCentreHead = p.ProfitCentreHead,
                DivisionId = p.DivisionId
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyTblProfitCentre.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



