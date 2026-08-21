using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Loaders;

internal sealed class MyTestOrProductLoader : MabArchiveExecutionLoaderBase
{
    public MyTestOrProductLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 20;

    public override string Name => "my_testorproduct";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcTestOrProduct
            .AsNoTracking()
            .Where(t => t.FpsYear == year)
            .Select(t => new MaDstMyTestOrProduct
            {
                Year = year,
                ItemCode = t.ItemCode,
                ItemDescription = t.ItemDescription,
                TestManager = t.TestManager,
                JobStatus = t.JobStatus,
                UnitPriceVla = t.UnitPriceVla,
                PriceAhvg = t.PriceAhvg,
                Owner = t.Owner,
                ChargeMethod = t.ChargeMethod,
                ShortDescription = t.ShortDescription,
                DefraUnitPrice = t.DefraUnitPrice
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyTestOrProduct.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



