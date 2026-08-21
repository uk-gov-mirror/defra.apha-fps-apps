using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Creates the target FPS year in fps.tblyearmaster as Planned when it does not exist.
/// </summary>
public sealed class CreatePlannedYearStep : IYearEndDataSetupStep
{
    private const string PlannedStatus = "Planned";

    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<CreatePlannedYearStep> _logger;

    public CreatePlannedYearStep(
        IYearEndDataSetupRepository repository,
        ILogger<CreatePlannedYearStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "CreatePlannedYearStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End Data Setup requires targetFpsYear before creating planned year.");
        }

        var targetYear = context.TargetFpsYear.Value;
        var yearState = await _repository.GetYearStateAsync(targetYear, cancellationToken);

        if (yearState is not null)
        {
            if (!string.Equals(yearState.Value.YearStatus, PlannedStatus, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Target year {targetYear} already exists in fps.tblyearmaster with status '{yearState.Value.YearStatus}'. Expected '{PlannedStatus}'.");
            }

            if (!yearState.Value.Active)
            {
                throw new InvalidOperationException(
                    $"Target year {targetYear} exists in fps.tblyearmaster but is inactive. Manual review is required.");
            }

            _logger.LogInformation(
                "YearEnd target year already exists as Planned | CorrelationId={CorrelationId} | TargetFpsYear={TargetFpsYear}",
                context.CorrelationId,
                targetYear);
            return;
        }

        var targetYearCode = BuildFpsYearCode(targetYear);
        var insertCount = await _repository.InsertPlannedYearAsync(targetYear, targetYearCode, context.CorrelationId, cancellationToken);

        if (insertCount != 1)
        {
            throw new InvalidOperationException(
                $"Expected to insert one row for target year {targetYear}, but inserted {insertCount} rows.");
        }

        _logger.LogInformation(
            "YearEnd planned year created | CorrelationId={CorrelationId} | TargetFpsYear={TargetFpsYear} | FpsYearCode={FpsYearCode}",
            context.CorrelationId,
            targetYear,
            targetYearCode);
    }

    private static string BuildFpsYearCode(int fpsYear)
    {
        var followingYearTwoDigits = (fpsYear + 1) % 100;
        return $"FPS{fpsYear}-{followingYearTwoDigits:D2}";
    }
}
