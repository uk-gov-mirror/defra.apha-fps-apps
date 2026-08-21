using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class CreateProjectMonthFinalStep : RecreateSummariesExecutionStepBase
{
    private readonly int _month;

    public CreateProjectMonthFinalStep(int month)
    {
        _month = month;
    }

    public override string StepName => "CreateProjectMonthFinal";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        var rows = await (
            from pm2 in db.RsProjectMonth2.AsNoTracking()
            where pm2.FpsYear == context.FpsYear
            join pm3 in db.RsProjectMonth3.AsNoTracking()
                on new { pm2.Project, EndPeriod = pm2.MonthNo, pm2.FpsYear } equals new { pm3.Project, pm3.EndPeriod, pm3.FpsYear }
            join pmcw in db.RsProjectMonthCasework.AsNoTracking()
                on new { pm2.Project, pm2.MonthNo, pm2.FpsYear } equals new { pmcw.Project, pmcw.MonthNo, pmcw.FpsYear }
            select new RsProjectMonthFinalTable
            {
                Project = pm2.Project,
                MonthNo = pm2.MonthNo,
                FpsYear = pm2.FpsYear,
                CostProfile = pm2.CostProfile,
                SubContracts = pm2.SubContracts,
                Animals = pm2.Animals,
                NonAnimals = pm2.NonAnimal,
                TimeCosts = (decimal?)pm2.TimeCosts,
                TransferCosts = (decimal?)pm2.TransferCosts,
                TotalCost = pm2.TotalCost,
                Invoices = pm2.Invoices,
                Coiw = pm2.Coiw,
                PortSales = (decimal?)pm2.PortSales,
                CumCost = pm2.MonthNo <= _month ? pm3.CumCost : null,
                CumProfile = pm3.CumProfile,
                PeriodName = pm3.PeriodName,
                SumOfCostProfile = pm3.SumOfCostProfile,
                CumInvoices = pm2.MonthNo <= _month ? pm3.CumInvoices : null,
                CumCoiw = pm2.MonthNo <= _month ? pm3.CumCoiw : null,
                CumPortSales = pm2.MonthNo <= _month ? pm3.CumPortSales : null,
                MstoneDue = pm2.MstoneDue,
                DueDone = pm2.DueDone,
                OnTime = pm2.OnTime,
                SumOfMstoneDue = pm3.SumOfMstoneDue,
                SumOfDueDone = pm2.MonthNo <= _month ? pm3.SumOfDueDone : null,
                SumOfOnTime = pm2.MonthNo <= _month ? pm3.SumOfOnTime : null,
                CumFlag = pm2.MonthNo <= _month ? 1 : null,
                CwDebit = pm2.MonthNo <= _month ? (decimal?)pmcw.CwDebit : null,
                CwCredit = pm2.MonthNo <= _month ? (decimal?)pmcw.CwCredit : null,
                CumCwDebit = pm2.MonthNo <= _month ? pm3.CumCwDebit : null,
                CumCwCredit = pm2.MonthNo <= _month ? pm3.CumCwCredit : null,
                TotalHours = pm2.TotalHours,
                CumTotalHours = pm2.MonthNo <= _month ? pm3.CumTotalHours : null,
                CumSubContracts = pm2.MonthNo <= _month ? pm3.CumSubContracts : null,
                CumTestCosts = pm2.MonthNo <= _month ? pm3.CumTestCosts : null,
                PayCosts = pm2.PayCosts,
                CumPayCosts = pm2.MonthNo <= _month ? pm3.CumPayCosts : null,
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        await db.RsProjectMonthFinal.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
