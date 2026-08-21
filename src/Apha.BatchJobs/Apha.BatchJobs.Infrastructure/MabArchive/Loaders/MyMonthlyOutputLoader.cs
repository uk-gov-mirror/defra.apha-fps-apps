using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Loaders;

internal sealed class MyMonthlyOutputLoader : MabArchiveExecutionLoaderBase
{
    public MyMonthlyOutputLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 5;

    public override string Name => "my_monthlyoutput";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcMonthlyOutput
            .AsNoTracking()
            .Where(m => m.FpsYear == year)
            .Select(m => new MaDstMyMonthlyOutput
            {
                Year = year,
                TestCode = m.TestCode,
                Buyer = m.Buyer,
                Month = m.Month,
                WorkGroup = m.WorkGroup,
                Volume = m.Volume,
                WgBuyer = m.WgBuyer
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyMonthlyOutput.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



