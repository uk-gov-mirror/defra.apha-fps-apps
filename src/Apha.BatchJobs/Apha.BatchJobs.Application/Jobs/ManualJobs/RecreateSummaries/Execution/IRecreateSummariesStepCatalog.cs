namespace Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries.Execution;

/// <summary>
/// Builds the ordered step list for RecreateSummaries execution.
/// </summary>
public interface IRecreateSummariesStepCatalog
{
    /// <summary>
    /// Builds mandatory steps 1-14.
    /// </summary>
    IReadOnlyList<IRecreateSummariesExecutionStep> BuildMandatorySteps(int month, int year, string triggeredBy);

    /// <summary>
    /// Builds conditional refresh steps 15-17.
    /// </summary>
    IReadOnlyList<IRecreateSummariesExecutionStep> BuildRefreshSteps(int month);
}
