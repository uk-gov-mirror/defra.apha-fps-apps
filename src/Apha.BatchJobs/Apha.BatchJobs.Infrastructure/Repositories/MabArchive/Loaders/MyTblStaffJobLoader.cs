using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTblStaffJobLoader : MabArchiveExecutionLoaderBase
{
    public MyTblStaffJobLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 13;

    public override string Name => "my_tblstaffjob";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcTblStaffJob
            .AsNoTracking()
            .Where(s => s.FpsYear == year)
            .Select(s => new MaDstMyTblStaffJob
            {
                Year = year,
                StaffId = s.StaffId,
                JobCode = s.JobCode,
                PlannedHours = s.PlannedHours
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyTblStaffJob.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



