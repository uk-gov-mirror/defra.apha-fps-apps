using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class DeleteProjectMonthFinalStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "DeleteProjectMonthFinal";

    protected override Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
        => context.DbContext.RsProjectMonthFinal
            .Where(x => x.FpsYear == context.FpsYear)
            .ExecuteDeleteAsync(cancellationToken);
}
