using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class CreateProjectMonthCaseworkStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateProjectMonthCasework";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        // Preserve legacy data-shape logic while making year explicit in the shared multi-year DB.
        return await db.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.projectmonthcasework (project, monthno, fpsyear, cwdebit, cwcredit)
            SELECT DISTINCT
                q.project,
                q.monthno,
                q.fpsyear,
                COALESCE(q.cwdebit::numeric, 0::numeric)::double precision,
                COALESCE(q.cwcredit::numeric, 0::numeric)::double precision
            FROM fps.qryprojectmonthcw q
            WHERE q.fpsyear = {context.FpsYear};", cancellationToken);
    }
}
