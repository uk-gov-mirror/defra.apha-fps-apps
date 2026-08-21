using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class RefreshPeriodMoStep : RecreateSummariesExecutionStepBase
{
    private readonly int _period;

    public RefreshPeriodMoStep(int period)
    {
        _period = period;
    }

    public override string StepName => "RefreshPeriodMo";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;
        var fpsYear = context.FpsYear;

        await db.RsPeriodMonthlyOutput
            .Where(x => x.Period == _period)
            .ExecuteDeleteAsync(cancellationToken);

        // Keep the SERIAL sequence in sync with current table state.
        await db.Database.ExecuteSqlRawAsync(@"
            SELECT setval(
                'fps.period_monthlyoutput_id_seq',
                COALESCE((SELECT MAX(id) FROM fps.period_monthlyoutput), 0)
            );", cancellationToken);

        return await db.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.period_monthlyoutput
                (period, project, oracleprojectcode, subaccountcode, isdefraproject, opc, occ, month, spc, workgroup, scc, testcode, volume, testprice, totalcost)
            SELECT
                {_period},
                p.parentproject,
                p.oracleprojectcode,
                p.subaccountcode,
                CASE WHEN COALESCE(p.isdefraproject, 0) = 0 THEN 'No' ELSE 'Yes' END,
                c.profitcentre,
                c.costcentre,
                mo.month,
                wg.profitcentre,
                wg.workgroup,
                wg.costcentre,
                mo.testcode,
                mo.volume,
                tr.unitprice,
                tr.unitprice * mo.volume::numeric
            FROM fps.monthlyoutput AS mo
            INNER JOIN fps.workgroup AS wg
                ON mo.workgroup = wg.workgroup
            INNER JOIN fps.tlkptestreqmt AS tr
                ON mo.buyer = tr.projectbuyercode
               AND mo.testcode = tr.testcode
               AND tr.fpsyear = {fpsYear}
            INNER JOIN fps.tlkpproject AS p
                ON mo.buyer = p.parentproject
            LEFT JOIN fps.costcentre AS c
                ON p.costcentre = c.costcentre
               AND p.fpsyear = c.fpsyear
            WHERE p.fpsyear = {fpsYear}
              AND mo.fpsyear = {fpsYear};", cancellationToken);
    }
}
