using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTlkpTestReqmtLoader : MabArchiveExecutionLoaderBase
{
    internal MyTlkpTestReqmtLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 15;

    public override string Name => "my_tlkptestreqmt";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        // 2026-05-21: Baseline SQL does not enforce NOT NULL here, and production source data
        // can contain null/blank ProjectBuyerCode/TestCode. EF materialization throws when
        // required key fields are null, so we skip invalid rows instead of failing the whole load.
        var rows = await context.MaSrcTlkpTestReqmt
            .AsNoTracking()
            .Where(t => t.FpsYear == year && !string.IsNullOrWhiteSpace(t.ProjectBuyerCode) && !string.IsNullOrWhiteSpace(t.TestCode))
            .Select(t => new MaDstMyTlkpTestReqmt
            {
                Year = year,
                TestCode = t.TestCode!,
                Buyer = t.Buyer,
                UnitPrice = t.UnitPrice,
                NoRequired = t.NoRequired,
                ProjectBuyerCode = t.ProjectBuyerCode!,
                TestBuyerCode = t.TestBuyerCode
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyTlkpTestReqmt.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}

