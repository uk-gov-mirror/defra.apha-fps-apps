using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTlkpProgramLoader : MabArchiveExecutionLoaderBase
{
    public MyTlkpProgramLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 1;

    public override string Name => "my_tlkpprogram";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var sourceRows = await context.MaSrcTlkpProgram
            .AsNoTracking()
            .Where(p => p.FpsYear == year)
            .Select(p => new
            {
                ProgramNo = p.ProgramNo,
                ProgramName = p.ProgramName,
                Directorate = p.Directorate,
                Minim = p.Minim,
                SectorName = p.SectorName,
                Customer = p.Customer,
                Target = p.Target,
                Manager = p.Manager
            })
            .ToListAsync(cancellationToken);

        var rows = sourceRows
            .Select(p => new MaDstMyTlkpProgram
            {
                Year = year,
                ProgramNo = p.ProgramNo,
                ProgramName = p.ProgramName,
                Directorate = p.Directorate,
                Minim = p.Minim,
                SectorName = p.SectorName,
                Customer = p.Customer,
                Target = p.Target,
                Manager = p.Manager
            })
            .ToList();

        if (rows.Count == 0)
        {
            return 0;
        }

        // 2026-05-21: Keep ProgramName nullable to match legacy baseline load behavior
        // (INSERT-SELECT from fps.tlkpProgram did not enforce ProgramName non-null).
        // Only ProgramNo is key-critical for my_tlkpprogram parity and integrity.
        var invalidRows = rows.Count(r => string.IsNullOrWhiteSpace(r.ProgramNo));
        if (invalidRows > 0)
        {
            throw new InvalidOperationException(
            $"Seq 1 MyTlkpProgram: {invalidRows} rows have NULL critical field ProgramNo.");
        }

        await context.MaDstMyTlkpProgram.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



