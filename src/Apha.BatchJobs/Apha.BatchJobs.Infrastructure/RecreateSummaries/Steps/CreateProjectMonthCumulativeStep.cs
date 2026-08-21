using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class CreateProjectMonthCumulativeStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateProjectMonthCumulative";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        // Two-step: materialize the join first, then group and aggregate in C# to avoid
        // COALESCE(SUM(money_col), 0::numeric) PostgreSQL type errors on money columns.
        var rawRows = await (
            from tp in db.RsTblPeriod.AsNoTracking()
            where tp.FpsYear == context.FpsYear
            join tpm in db.RsTblkPeriodMonth.AsNoTracking()
                on tp.PeriodName equals tpm.PeriodName
            join pm2 in db.RsProjectMonth2.AsNoTracking()
                on tpm.MonthNo equals pm2.MonthNo
            where pm2.FpsYear == context.FpsYear
            join pmcw in db.RsProjectMonthCasework.AsNoTracking()
                on new { pm2.Project, pm2.MonthNo, pm2.FpsYear } equals new { pmcw.Project, pmcw.MonthNo, FpsYear = EF.Property<int>(pmcw, "FpsYear") }
            select new
            {
                tp.EndPeriod,
                tp.PeriodName,
                pm2.Project,
                pm2.FpsYear,
                pm2.TotalCost,
                pm2.Invoices,
                pm2.Coiw,
                pm2.PortSales,
                pm2.CostProfile,
                pm2.MstoneDue,
                pm2.DueDone,
                pm2.OnTime,
                pm2.TotalHours,
                pm2.SubContracts,
                pm2.TransferCosts,
                pm2.PayCosts,
                pmcw.CwDebit,
                pmcw.CwCredit
            })
            .ToListAsync(cancellationToken);

        var grouped = rawRows
            .GroupBy(r => new { r.EndPeriod, r.PeriodName, r.Project, r.FpsYear })
            .Select(g => new RsProjectMonth3Table
            {
                EndPeriod = g.Key.EndPeriod,
                PeriodName = g.Key.PeriodName,
                Project = g.Key.Project,
                FpsYear = g.Key.FpsYear,
                CumCost = g.Sum(x => x.TotalCost) ?? 0m,
                CumInvoices = g.Sum(x => x.Invoices) ?? 0m,
                CumCoiw = g.Sum(x => x.Coiw) ?? 0m,
                CumPortSales = (decimal?)g.Sum(x => x.PortSales ?? 0d),
                CumProfile = g.Sum(x => x.CostProfile) ?? 0m,
                SumOfCostProfile = g.Sum(x => x.CostProfile) ?? 0m,
                SumOfMstoneDue = g.Sum(x => (double)(x.MstoneDue ?? 0)),
                SumOfDueDone = g.Sum(x => x.DueDone ?? 0d),
                SumOfOnTime = g.Sum(x => x.OnTime ?? 0d),
                CumCwDebit = (decimal?)g.Sum(x => x.CwDebit ?? 0d),
                CumCwCredit = (decimal?)g.Sum(x => x.CwCredit ?? 0d),
                CumTotalHours = g.Sum(x => x.TotalHours ?? 0d),
                CumSubContracts = g.Sum(x => (double)(x.SubContracts ?? 0m)),
                CumTestCosts = g.Sum(x => x.TransferCosts ?? 0d),
                CumPayCosts = g.Sum(x => x.PayCosts ?? 0d)
            })
            .ToList();

        // Dedup on PK {Project, EndPeriod, FpsYear} to prevent EF tracking collisions
        var rows = grouped
            .GroupBy(x => new { x.Project, x.EndPeriod, x.FpsYear })
            .Select(g => g.First())
            .ToList();

        await db.RsProjectMonth3.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
