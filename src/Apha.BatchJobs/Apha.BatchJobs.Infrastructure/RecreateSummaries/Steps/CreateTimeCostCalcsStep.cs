using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class CreateTimeCostCalcsStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateTimeCostCalcs";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        return await db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO fps.timecostcalcs
    (workgroup, jobcode, project, month, staffid, gradecode, name, chargerate, class, time, cost, division, pay, nonpay, overhead, fpsyear)
SELECT
    COALESCE(wgg.workgroup, ''),
    mt.timecode,
    tcv.parentproject,
    mt.month,
    vps.pactid,
    COALESCE(wgg.gradecode, ''),
    COALESCE(vps.name, ''),
    (
        CASE
            WHEN COALESCE(p.isdefraproject, 0) = 0 THEN COALESCE(pcg.chargerate::numeric, 0::numeric)
            ELSE COALESCE(pcg.defrachargerate::numeric, 0::numeric)
        END
    )::money,
    CASE WHEN prg.sector_name = 'Charge' THEN 'Charge' ELSE 'Free' END,
    COALESCE(mt.hours, 0),
    CASE
        WHEN prg.sector_name = 'Charge'
            THEN (
                COALESCE(mt.hours, 0)::numeric *
                CASE
                    WHEN COALESCE(p.isdefraproject, 0) = 0 THEN COALESCE(pcg.chargerate::numeric, 0)
                    ELSE COALESCE(pcg.defrachargerate::numeric, 0)
                END
            )::double precision
        ELSE 0::double precision
    END,
    COALESCE(pc.division, ''),
    (COALESCE(mt.hours, 0)::numeric * COALESCE(pcg.payrate::numeric, 0))::money,
    (COALESCE(mt.hours, 0)::numeric * COALESCE(pcg.npr::numeric, 0))::money,
    (COALESCE(mt.hours, 0)::numeric * COALESCE(pcg.ohr::numeric, 0))::money,
    p.fpsyear
FROM fps.tblkpprofitcentre pc
JOIN fps.profitcentregrade pcg
    ON pc.profitcentre = pcg.profitcentre
JOIN fps.workgroupgrade wgg
    ON pcg.pcgrade = wgg.profitcentregrade
    AND wgg.fpsyear = pcg.fpsyear
JOIN fps.vpacttblstaff vps
    ON wgg.wggrade = vps.workgroupgrade
    AND vps.fpsyear = wgg.fpsyear
JOIN fps.monthlytime mt
    ON vps.pactid = mt.pactstaffid
    AND mt.fpsyear = vps.fpsyear
JOIN fps.timecodevalid tcv
    ON mt.workgroup = tcv.workgroup
    AND mt.timecode = tcv.timecode
    AND mt.parentproject = tcv.parentproject
    AND tcv.fpsyear = mt.fpsyear
JOIN fps.tlkpproject p
    ON tcv.parentproject = p.parentproject
    AND p.fpsyear = tcv.fpsyear
JOIN fps.tlkpprogram prg
    ON p.program = prg.programno
    AND prg.fpsyear = p.fpsyear
WHERE p.fpsyear = {context.FpsYear};", cancellationToken);
    }
}
