using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyProjInvoiceLoader : MabArchiveExecutionLoaderBase
{
    public MyProjInvoiceLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 7;

    public override string Name => "my_proj_invoice";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        // Month is nullable on both source and destination (dbscript/schemas/02mabarchive/01tables/my_proj_invoice.sql);
        // it is not part of the real DB primary key, so null-month rows are archived exactly as legacy
        // sp_AddMY_Proj_Invoice did (CR-028 -- a prior EF key mismatch had incorrectly excluded them).
        var rows = await context.MaSrcProjInvoice
            .AsNoTracking()
            .Where(i => i.FpsYear == year)
            .Select(i => new MaDstMyProjInvoice
            {
                Year = year,
                ProjectParent = i.ProjectParent,
                Month = i.Month,
                Amount = i.Amount,
                CostOfWork = i.CostOfWork,
                Wip = i.Wip,
                ProfitLoss = i.ProfitLoss,
                Detail = i.Detail,
                InvoiceCounter = i.InvoiceCounter,
                Type = i.Type
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyProjInvoice.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



