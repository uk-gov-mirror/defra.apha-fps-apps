using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyWorkgroupLoader : MabArchiveExecutionLoaderBase
{
    internal MyWorkgroupLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 22;

    public override string Name => "my_workgroup";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcWorkGroup
            .AsNoTracking()
            .Where(w => w.FpsYear == year)
            .Select(w => new MaDstMyWorkGroup
            {
                Year = year,
                WorkGroup = w.WorkGroup,
                ProfitCentre = w.ProfitCentre,
                CostCentre = w.CostCentre,
                Owner = w.Owner,
                Description = w.Description,
                CentralOverhead = w.CentralOverhead,
                SendEmail = w.SendEmail,
                Cos90 = w.Cos90,
                CostCentreOld = w.CostCentreOld,
                EmailRecipient = w.EmailRecipient
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyWorkGroup.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}

