using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Placeholder for planned Year End Data Setup steps that are not implemented yet.
/// </summary>
public sealed class PlaceholderYearEndDataSetupStep : IYearEndDataSetupStep
{
    public PlaceholderYearEndDataSetupStep(string name)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Step name is required.", nameof(name))
            : name;
    }

    public string Name { get; }

    public Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        throw new NotImplementedException(
            $"Year End Data Setup step '{Name}' is not implemented yet.");
    }
}
