using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Loaders;

internal sealed class MyMonthlyTimeLoader : MabArchiveExecutionLoaderBase
{
    public MyMonthlyTimeLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 6;

    public override string Name => "my_monthlytime";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcMonthlyTime
            .AsNoTracking()
            .Where(m => m.FpsYear == year)
            .Select(m => new MaDstMyMonthlyTime
            {
                Year = year,
                PactStaffId = m.PactStaffId,
                TimeCode = m.TimeCode,
                Month = m.Month,
                ParentProject = m.ParentProject,
                WorkGroup = m.WorkGroup,
                Hours = m.Hours
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyMonthlyTime.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



