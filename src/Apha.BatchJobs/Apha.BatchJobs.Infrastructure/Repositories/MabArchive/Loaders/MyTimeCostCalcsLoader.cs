using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTimeCostCalcsLoader : MabArchiveExecutionLoaderBase
{
    internal MyTimeCostCalcsLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 14;

    public override string Name => "my_timecostcalcs";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcTimeCostCalcs
            .AsNoTracking()
            .Where(t => t.FpsYear == year)
            .Select(t => new MaDstMyTimeCostCalcs
            {
                Year = year,
                WorkGroup = t.WorkGroup,
                JobCode = t.JobCode,
                Project = t.Project,
                Month = t.Month,
                StaffId = t.StaffId,
                GradeCode = t.GradeCode,
                Name = t.Name,
                ChargeRate = t.ChargeRate,
                Class = t.Class,
                Time = t.Time,
                Cost = t.Cost,
                Division = t.Division,
                JobCodeOld = t.JobCodeOld,
                Pay = t.Pay,
                NonPay = t.NonPay,
                Overhead = t.Overhead
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyTimeCostCalcs.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



