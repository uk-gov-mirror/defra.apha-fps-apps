using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Executes the approved Year End cutover pipeline.
/// </summary>
public interface IYearEndCutoverService
{
    /// <summary>
    /// Executes Year End Cutover against the provided validated context.
    /// </summary>
    Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default);
}
