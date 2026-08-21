using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Domain.Enums;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries;

/// <summary>
/// Base class for .NET RecreateSummaries steps.
/// </summary>
internal abstract class RecreateSummariesExecutionStepBase : IRecreateSummariesExecutionStep
{
    public abstract string StepName { get; }

    public async Task<StepResult> ExecuteAsync(IRecreateSummariesExecutionContext context, CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow;

        try
        {
            if (context is not RecreateSummariesExecutionContext executionContext)
                throw new ArgumentException("Unexpected RecreateSummaries execution context.", nameof(context));

            var rowsAffected = await ExecuteCoreAsync(executionContext, cancellationToken);
            return new StepResult(StepName, rowsAffected, start, DateTime.UtcNow, StepStatus.Success);
        }
        catch (OperationCanceledException)
        {
            throw; // Must propagate — caller owns cancellation handling and transaction rollback
        }
        catch (Exception ex)
        {
            var root = ex.GetBaseException();
            var message = ReferenceEquals(root, ex)
                ? ex.Message
                : $"{ex.Message} | Root: {root.Message}";

            return new StepResult(StepName, 0, start, DateTime.UtcNow, StepStatus.Failed, message);
        }
    }

    protected abstract Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken);
}
