using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyProjectMonthFinalLoader : MabArchiveExecutionLoaderBase
{
    public MyProjectMonthFinalLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 9;

    public override string Name => "my_projectmonthfinal";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcProjectMonthFinal
            .AsNoTracking()
            .Where(p => p.FpsYear == year)
            .Select(p => new MaDstMyProjectMonthFinal
            {
                Year = year,
                Project = p.Project,
                MonthNo = p.MonthNo,
                PeriodName = p.PeriodName,
                CumFlag = p.CumFlag,
                CostProfile = p.CostProfile,
                SubContracts = p.SubContracts,
                Animals = p.Animals,
                NonAnimals = p.NonAnimals,
                TimeCosts = p.TimeCosts,
                TransferCosts = p.TransferCosts,
                TotalCost = p.TotalCost,
                Invoices = p.Invoices,
                Coiw = p.Coiw,
                PortSales = p.PortSales,
                CumCost = p.CumCost,
                CumProfile = p.CumProfile,
                SumOfCostProfile = p.SumOfCostProfile,
                CumInvoices = p.CumInvoices,
                CumCoiw = p.CumCoiw,
                CumPortSales = p.CumPortSales,
                MstoneDue = p.MstoneDue,
                DueDone = p.DueDone,
                OnTime = p.OnTime,
                SumOfMstoneDue = p.SumOfMstoneDue,
                SumOfDueDone = p.SumOfDueDone,
                SumOfOnTime = p.SumOfOnTime,
                CwDebit = p.CwDebit,
                CwCredit = p.CwCredit,
                CumCwDebit = p.CumCwDebit,
                CumCwCredit = p.CumCwCredit,
                TotalHours = p.TotalHours,
                CumTotalHours = p.CumTotalHours,
                CumSubContracts = p.CumSubContracts,
                CumTestCosts = p.CumTestCosts,
                PayCosts = p.PayCosts,
                CumPayCosts = p.CumPayCosts
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyProjectMonthFinal.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}



