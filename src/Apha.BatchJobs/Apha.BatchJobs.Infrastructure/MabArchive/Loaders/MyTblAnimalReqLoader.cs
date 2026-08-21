using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Loaders;

internal sealed class MyTblAnimalReqLoader : MabArchiveExecutionLoaderBase
{
    public MyTblAnimalReqLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 11;

    public override string Name => "my_tblanimalreq";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        // Ensure loader-level idempotency if orchestration retries resume after a partial load.
        await context.MaDstMyTblAnimalReq
            .Where(x => x.Year == year)
            .ExecuteDeleteAsync(cancellationToken);

        // Keep source insertion order so explicit ar_counter values match SQL loader behavior.
        var sourceRows = await context.MaSrcTblAnimalReq
            .AsNoTracking()
            .Where(a => a.FpsYear == year)
            .OrderBy(a => a.IndCounter)
            .ToListAsync(cancellationToken);

        if (sourceRows.Count == 0)
        {
            return 0;
        }

        // Align sequence with live table state to avoid duplicate ar_counter keys.
        await context.Database.ExecuteSqlRawAsync(
            "SELECT setval(pg_get_serial_sequence('mabarchive.my_tblanimalreq', 'ar_counter'), COALESCE((SELECT MAX(ar_counter) FROM mabarchive.my_tblanimalreq), 0) + 1, false)",
            cancellationToken);

        var rows = sourceRows
            .Select(a => new MaDstMyTblAnimalReq
            {
                Year = year,
                JobCode = a.JobCode,
                AnimalType = a.AnimalType,
                NumberOfDays = a.NumberOfDays,
                NumberOfAnimals = a.NumberOfAnimals
            })
            .ToList();

        await context.MaDstMyTblAnimalReq.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



