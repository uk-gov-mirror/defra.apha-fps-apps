using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyWorkgroupGradeLoader : MabArchiveExecutionLoaderBase
{
    public MyWorkgroupGradeLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 17;

    public override string Name => "my_workgroupgrade";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcWorkGroupGrade
            .AsNoTracking()
            .Where(w => w.FpsYear == year)
            .Select(w => new MaDstMyWorkGroupGrade
            {
                Year = year,
                WgGrade = w.WgGrade,
                ProfitCentreGrade = w.ProfitCentreGrade,
                GradeCode = w.GradeCode,
                WorkGroup = w.WorkGroup
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyWorkGroupGrade.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}

