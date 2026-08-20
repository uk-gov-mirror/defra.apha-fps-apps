
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;

/// <summary>
/// Contract for a single executable RecreateSummaries step.
/// This abstraction supports the LINQ/EF Core orchestration pipeline.
/// </summary>
public interface IRecreateSummariesExecutionStep
{
    /// <summary>Unique display name matching the legacy procedure name.</summary>
    string StepName { get; }

    Task<StepResult> ExecuteAsync(IRecreateSummariesExecutionContext context, CancellationToken cancellationToken = default);
}
