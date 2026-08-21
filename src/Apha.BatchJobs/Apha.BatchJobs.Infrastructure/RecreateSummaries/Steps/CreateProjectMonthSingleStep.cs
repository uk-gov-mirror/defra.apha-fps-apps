using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class CreateProjectMonthSingleStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateProjectMonthSingle";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        var hasNullFpsYear = await db.RsProjectMonth
            .AsNoTracking()
            .AnyAsync(pm => pm.FpsYear == null, cancellationToken);

        if (hasNullFpsYear)
        {
            throw new InvalidOperationException("projectmonth contains rows with null fpsyear. This violates consolidated multi-year integrity requirements.");
        }

        // Two-step: fetch raw nullable values first (avoid COALESCE on PostgreSQL money columns),
        // then apply defaults in C#.
        var rawRows = await (
            from pm in db.RsProjectMonth.AsNoTracking()
            where pm.FpsYear == context.FpsYear
            join sc0 in db.RsQryJobMonthSubContracts.AsNoTracking()
                on new { pm.Project, Month = pm.MonthNo } equals new { sc0.Project, sc0.Month } into sc1
            from sc in sc1.DefaultIfEmpty()
            join tm0 in db.RsQryJobMonthTime.AsNoTracking()
                on new { pm.Project, Month = pm.MonthNo } equals new { tm0.Project, tm0.Month } into tm1
            from tm in tm1.DefaultIfEmpty()
            join ms0 in db.RsQryJobMonthMilestone.AsNoTracking()
                on new { pm.Project, DueMonth = pm.MonthNo } equals new { ms0.Project, ms0.DueMonth } into ms1
            from ms in ms1.DefaultIfEmpty()
            join tr0 in db.RsQryJobMonthTransfersTotal.AsNoTracking()
                on new { pm.Project, Month = pm.MonthNo } equals new { tr0.Project, tr0.Month } into tr1
            from tr in tr1.DefaultIfEmpty()
            join iv0 in db.RsQryJobMonthInvoices.AsNoTracking()
                on new { ProjectParent = pm.Project, Month = pm.MonthNo } equals new { iv0.ProjectParent, iv0.Month } into iv1
            from iv in iv1.DefaultIfEmpty()
            join ps0 in db.RsQryJobMonthPortfolioSales.AsNoTracking()
                on new { PlanPortfolio = pm.Project, Month = pm.MonthNo } equals new { ps0.PlanPortfolio, ps0.Month } into ps1
            from ps in ps1.DefaultIfEmpty()
            join tp0 in db.RsQryJobMonthTotProfile.AsNoTracking()
                on pm.Project equals tp0.Project into tp1
            from tp in tp1.DefaultIfEmpty()
            select new
            {
                Project = pm.Project,
                MonthNo = pm.MonthNo,
                FpsYear = pm.FpsYear!.Value,
                CostProfile = pm.CostProfile,
                ScTotal = sc.Total,
                ScAnimals = sc.Animals,
                ScOther = sc.Other,
                TmSumOfCost = tm.SumOfCost,
                TrSumOfTransferCost = tr.SumOfTransferCost,
                IvSumOfAmount1 = iv.SumOfAmount1,
                IvWorkCost = iv.WorkCost,
                TpSumOfCostProfile = tp.SumOfCostProfile,
                PsFee = ps.Fee,
                MsMstoneDue = ms.MstoneDue,
                MsDueDone = ms.DueDone,
                MsOnTime = ms.OnTime,
                TmSumOfHours = tm.SumOfHours,
                TmSumOfPayRate = tm.SumOfPayRate
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var rows = rawRows.Select(r => new RsProjectMonth2Table
        {
            Project = r.Project,
            MonthNo = r.MonthNo,
            FpsYear = r.FpsYear,
            CostProfile = r.CostProfile,
            SubContracts = r.ScTotal ?? 0m,
            Animals = r.ScAnimals ?? 0m,
            NonAnimal = r.ScOther ?? 0m,
            TimeCosts = r.TmSumOfCost ?? 0d,
            TransferCosts = (double?)(r.TrSumOfTransferCost) ?? 0d,
            TotalCost = (r.ScTotal ?? 0m)
                + (decimal)(r.TmSumOfCost ?? 0d)
                + (r.TrSumOfTransferCost ?? 0m),
            Invoices = r.IvSumOfAmount1 ?? 0m,
            Coiw = r.IvWorkCost ?? 0m,
            SumOfCostProfile = r.TpSumOfCostProfile == null ? null : (decimal?)r.TpSumOfCostProfile,
            PortSales = (double?)(r.PsFee) ?? 0d,
            MstoneDue = r.MsMstoneDue,
            DueDone = r.MsDueDone,
            OnTime = r.MsOnTime,
            TotalHours = r.TmSumOfHours ?? 0d,
            PayCosts = (double?)(r.TmSumOfPayRate) ?? 0d
        })
        .GroupBy(x => new { x.Project, x.MonthNo, x.FpsYear })
        .Select(g => g.First())
        .ToList();

        await db.RsProjectMonth2.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
