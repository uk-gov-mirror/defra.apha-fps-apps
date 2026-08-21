using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class RefreshPeriodTccStep : RecreateSummariesExecutionStepBase
{
    private readonly int _period;

    public RefreshPeriodTccStep(int period)
    {
        _period = period;
    }

    public override string StepName => "RefreshPeriodTcc";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;
        var fpsYear = context.FpsYear;

        await db.RsPeriodTimeCostCalcs
            .Where(x => x.Period == _period)
            .ExecuteDeleteAsync(cancellationToken);

        // Keep the SERIAL sequence in sync with current table state.
        await db.Database.ExecuteSqlRawAsync(@"
            SELECT setval(
                'fps.period_timecostcalcs_id_seq',
                COALESCE((SELECT MAX(id) FROM fps.period_timecostcalcs), 0)
            );", cancellationToken);

        return await db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO fps.period_timecostcalcs
    (period, project, oracleprojectcode, subaccountcode, month, defraproject,
     occ, opc, spc, scc, name, gradecode, spnumber, chargerate, pay, nonpay, overhead, time, totalcost)
SELECT
    {_period},
    p.parentproject,
    p.oracleprojectcode,
    p.subaccountcode,
    tcc.month,
    CASE WHEN COALESCE(p.isdefraproject, 0) = 0 THEN 'No' ELSE 'Yes' END,
    cc.costcentre,
    cc.profitcentre,
    wg.profitcentre,
    wg.costcentre,
    COALESCE(tcc.name, ''),
    tcc.gradecode,
    emp.spnumber,
    tcc.chargerate,
    tcc.pay,
    tcc.nonpay,
    tcc.overhead,
    tcc.time,
    tcc.cost::numeric::money
FROM fps.timecostcalcs tcc
JOIN fps.workgroup wg ON tcc.workgroup = wg.workgroup
JOIN fps.tlkpproject p ON tcc.project = p.parentproject AND tcc.fpsyear = p.fpsyear
LEFT JOIN fps.costcentre cc ON p.costcentre = cc.costcentre AND p.fpsyear = cc.fpsyear
JOIN fps.tblwgemployee emp ON tcc.staffid = emp.pactid
WHERE tcc.fpsyear = {fpsYear}
", cancellationToken);
    }
}
