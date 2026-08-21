using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Executes the approved Year End data-setup pipeline.
/// </summary>
public interface IYearEndDataSetupService
{
    /// <summary>
    /// Executes Year End Data Setup against the provided validated context.
    /// </summary>
    Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default);
}
