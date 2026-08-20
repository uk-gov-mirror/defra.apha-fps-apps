using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyProfitCentreGradeLoader : MabArchiveExecutionLoaderBase
{
    internal MyProfitCentreGradeLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 18;

    public override string Name => "my_profitcentregrade";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcProfitCentreGrade
            .AsNoTracking()
            .Where(p => p.FpsYear == year)
            .Select(p => new MaDstMyProfitCentreGrade
            {
                Year = year,
                PcGrade = p.PcGrade,
                DivisionGrade = p.DivisionGrade,
                GradeCode = p.GradeCode,
                ProfitCentre = p.ProfitCentre,
                ChargeRate = p.ChargeRate,
                DirectRate = p.DirectRate,
                PayRate = p.PayRate,
                Npr = p.Npr,
                Ohr = p.Ohr
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyProfitCentreGrade.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



