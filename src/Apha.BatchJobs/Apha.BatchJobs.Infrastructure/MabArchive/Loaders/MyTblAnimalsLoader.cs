using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Loaders;

internal sealed class MyTblAnimalsLoader : MabArchiveExecutionLoaderBase
{
    public MyTblAnimalsLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 23;

    public override string Name => "my_tblanimals";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcTblAnimals
            .AsNoTracking()
            .Where(a => a.FpsYear == year)
            .Select(a => new MaDstMyTblAnimals
            {
                Year = year,
                AnimalType = a.AnimalType,
                Species = a.Species,
                SecurityLevel = a.SecurityLevel,
                DailyRate = a.DailyRate,
                PlanByWeek = a.PlanByWeek,
                DefraDailyRate = a.DefraDailyRate
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyTblAnimals.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



