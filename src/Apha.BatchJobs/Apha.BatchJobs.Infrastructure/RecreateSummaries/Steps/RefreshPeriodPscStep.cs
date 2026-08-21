using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class RefreshPeriodPscStep : RecreateSummariesExecutionStepBase
{
    private readonly int _period;

    public RefreshPeriodPscStep(int period)
    {
        _period = period;
    }

    public override string StepName => "RefreshPeriodPsc";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;
        var fpsYear = context.FpsYear;

        await db.RsPeriodProjSubContract
            .Where(x => x.Period == _period)
            .ExecuteDeleteAsync(cancellationToken);

        var rows = await (
            from psc in db.RsProjSubContract.AsNoTracking()
            where psc.FpsYear == fpsYear && psc.Month != null
            join p in db.RsTlkpProject.AsNoTracking()
                on new { Project = psc.Project, psc.FpsYear }
                equals new { Project = p.ParentProject, p.FpsYear }
            where p.FpsYear == fpsYear
            join cc0 in db.RsCostCentre.AsNoTracking()
                on new { CostCentre = p.CostCentre, FpsYear = p.FpsYear }
                equals new { CostCentre = (double?)cc0.CostCentre, cc0.FpsYear } into cc1
            from cc in cc1.DefaultIfEmpty()
            select new RsPeriodProjSubContractTable
            {
                Period = _period,
                SubContCounter = psc.SubContCounter,
                Project = psc.Project,
                OracleProjectCode = p.OracleProjectCode,
                SubAccountCode = p.SubAccountCode,
                IsDefraProject = (p.IsDefraProject ?? 0) == 0 ? "No" : "Yes",
                Opc = cc != null ? cc.ProfitCentre : null,
                Occ = cc != null ? cc.CostCentre : null,
                Month = psc.Month!.Value,
                Amount = psc.Amount,
                AcctCode = psc.AcctCode
            })
            .ToListAsync(cancellationToken);

        await db.RsPeriodProjSubContract.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
