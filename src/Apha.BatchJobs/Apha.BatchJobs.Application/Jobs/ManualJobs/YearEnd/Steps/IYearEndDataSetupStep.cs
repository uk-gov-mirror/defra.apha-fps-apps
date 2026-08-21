using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Represents one ordered step in the Year End Data Setup pipeline.
/// </summary>
public interface IYearEndDataSetupStep
{
    /// <summary>
    /// Human-readable step name used in structured logs.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the step.
    /// </summary>
    Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default);
}
