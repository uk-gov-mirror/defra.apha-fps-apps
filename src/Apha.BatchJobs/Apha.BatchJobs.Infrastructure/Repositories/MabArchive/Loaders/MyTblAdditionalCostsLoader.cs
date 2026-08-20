using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTblAdditionalCostsLoader : MabArchiveExecutionLoaderBase
{
    internal MyTblAdditionalCostsLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 10;

    public override string Name => "my_tbladditionalcosts";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var maxAcCounter = await context.MaDstMyTblAdditionalCosts
            .AsNoTracking()
            .MaxAsync(a => a.AcCounter, cancellationToken) ?? 0;

        var sourceRows = await context.MaSrcTblAdditionalCosts
            .AsNoTracking()
            .Where(a => a.FpsYear == year)
            .OrderBy(a => a.JobCode)
            .ThenBy(a => a.Account)
            .ThenBy(a => a.Description)
            .ToListAsync(cancellationToken);

        if (sourceRows.Count == 0)
        {
            return 0;
        }

        var rows = sourceRows
            .Select((a, index) => new MaDstMyTblAdditionalCosts
            {
                Year = year,
                JobCode = a.JobCode,
                Account = a.Account,
                Description = a.Description,
                ItemCost = a.ItemCost,
                Freq = a.Freq,
                Supplier = a.Supplier,
                AcCounter = maxAcCounter + index + 1
            })
            .ToList();

        await context.MaDstMyTblAdditionalCosts.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



