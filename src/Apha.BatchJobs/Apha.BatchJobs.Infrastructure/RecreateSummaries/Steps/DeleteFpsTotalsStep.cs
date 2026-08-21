using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class DeleteFpsTotalsStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "DeleteFpsTotals";

    protected override Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
        => context.DbContext.RsFpsYearTotals
            .Where(r => r.FpsYear == context.FpsYear)
            .ExecuteDeleteAsync(cancellationToken);
}
