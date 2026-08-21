using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Loaders;

internal sealed class MyTblContractLoader : MabArchiveExecutionLoaderBase
{
    public MyTblContractLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 12;

    public override string Name => "my_tblcontract";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var sourceRows = await context.MaSrcTblContract
            .AsNoTracking()
            .Where(c => c.FpsYear == year)
            .ToListAsync(cancellationToken);

        var rows = sourceRows
            .Select(c => new MaDstMyTblContract
            {
                Year = year,
                ContractNo = c.ContractNo,
                Category = c.Category,
                Manager = c.Manager,
                Customer = c.Customer,
                Title = c.Title,
                RegisteredDate = ToUtcKind(c.RegisteredDate),
                StartDate = ToUtcKind(c.StartDate),
                EndDate = ToUtcKind(c.EndDate),
                ContractDoc = c.ContractDoc,
                Duration = c.Duration
            })
            .ToList();

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyTblContract.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }

    private static DateTime? ToUtcKind(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return value.Value.Kind == DateTimeKind.Utc
            ? value.Value
            : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
    }
}



