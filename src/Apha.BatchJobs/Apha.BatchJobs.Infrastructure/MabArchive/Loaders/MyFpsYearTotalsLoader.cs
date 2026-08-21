using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Loaders;

internal sealed class MyFpsYearTotalsLoader : MabArchiveExecutionLoaderBase
{
    public MyFpsYearTotalsLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 4;

    public override string Name => "my_fpsyeartotals";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcFpsYearTotals
            .AsNoTracking()
            .Where(f => f.FpsYear == year)
            .Select(f => new MaDstMyFpsYearTotals
            {
                Year = year,
                ParentProject = f.ParentProject,
                Program = f.Program,
                TotalAdditionalCosts = f.TotalAdditionalCosts,
                TotalAnimalCosts = f.TotalAnimalCosts,
                TotalStaffCosts = f.TotalStaffCosts,
                TotalTestCosts = f.TotalTestCosts,
                TotalCosts = f.TotalCosts,
                CustIncome = f.CustIncome,
                TransferIncome = f.TransferIncome,
                TotalIncome = f.TotalIncome,
                BudgetCvl = f.BudgetCvl,
                RequiredProfit = f.RequiredProfit,
                Manager = f.Manager,
                Customer = f.Customer,
                ProjectStatus = f.ProjectStatus,
                PvsIncome = f.PvsIncome,
                PlanCaseworkDebit = f.PlanCaseworkDebit,
                TotalPayCosts = f.TotalPayCosts
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyFpsYearTotals.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



