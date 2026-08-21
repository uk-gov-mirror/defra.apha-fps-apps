using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class DeleteProjectMonthCaseworkStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "DeleteProjectMonthCasework";

    protected override Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
        => context.DbContext.RsProjectMonthCasework
            .Where(x => EF.Property<int>(x, "FpsYear") == context.FpsYear)
            .ExecuteDeleteAsync(cancellationToken);
}
