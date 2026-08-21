using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Validates that Year End year-control metadata exists and contains the current year row.
/// </summary>
public sealed class ValidateYearScopedSchemaStep : IYearEndDataSetupStep
{
    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<ValidateYearScopedSchemaStep> _logger;

    public ValidateYearScopedSchemaStep(
        IYearEndDataSetupRepository repository,
        ILogger<ValidateYearScopedSchemaStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ValidateYearScopedSchemaStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before schema validation.");
        }

        if (!await _repository.TableExistsAsync("fps", "tblyearmaster", cancellationToken))
        {
            throw new InvalidOperationException("Required table fps.tblyearmaster was not found. Year End cannot continue.");
        }

        var requiredColumns = new[] { "fpsyear", "fpsyearcode", "yearstatus", "active" };
        foreach (var columnName in requiredColumns)
        {
            if (!await _repository.ColumnExistsAsync("fps", "tblyearmaster", columnName, cancellationToken))
            {
                throw new InvalidOperationException($"Required column fps.tblyearmaster.{columnName} was not found. Year End cannot continue.");
            }
        }

        var currentYearExists = await _repository.YearRowExistsAsync(
            context.CurrentFpsYear.Value,
            cancellationToken);

        if (!currentYearExists)
        {
            throw new InvalidOperationException(
                $"Current year {context.CurrentFpsYear.Value} does not exist in fps.tblyearmaster. Year End cannot continue.");
        }

        _logger.LogInformation(
            "YearEnd schema validation succeeded | CorrelationId={CorrelationId} | CurrentFpsYear={CurrentFpsYear} | TargetFpsYear={TargetFpsYear}",
            context.CorrelationId,
            context.CurrentFpsYear,
            context.TargetFpsYear);
    }

}
