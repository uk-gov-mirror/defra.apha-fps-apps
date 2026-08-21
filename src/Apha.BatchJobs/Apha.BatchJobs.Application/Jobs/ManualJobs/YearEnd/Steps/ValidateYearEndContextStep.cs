using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Validates mandatory Year End context values before data movement starts.
/// </summary>
public sealed class ValidateYearEndContextStep : IYearEndDataSetupStep
{
    public string Name => "ValidateYearEndContextStep";

    public Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End Data Setup requires currentFpsYear in BATCH_JOB_PARAMETERS_JSON.");
        }

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End Data Setup requires targetFpsYear in BATCH_JOB_PARAMETERS_JSON.");
        }

        if (context.TargetFpsYear.Value == context.CurrentFpsYear.Value)
        {
            throw new InvalidOperationException("targetFpsYear must be different from currentFpsYear.");
        }

        if (context.TargetFpsYear.Value < context.CurrentFpsYear.Value)
        {
            throw new InvalidOperationException("targetFpsYear must be greater than currentFpsYear.");
        }

        return Task.CompletedTask;
    }
}
