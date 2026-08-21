using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Service-layer entry point for Year End Data Setup.
/// Concrete table/step implementation will be added incrementally.
/// </summary>
public sealed class YearEndDataSetupService : IYearEndDataSetupService
{
    private readonly ILogger<YearEndDataSetupService> _logger;
    private readonly IReadOnlyList<IYearEndDataSetupStep> _steps;

    public YearEndDataSetupService(
        IEnumerable<IYearEndDataSetupStep> steps,
        ILogger<YearEndDataSetupService> logger)
    {
        _steps = steps?.ToList() ?? throw new ArgumentNullException(nameof(steps));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        _logger.LogInformation(
            "YearEndDataSetup service contract invoked | CorrelationId={CorrelationId} | TargetFpsYear={TargetFpsYear} | CurrentFpsYear={CurrentFpsYear}",
            context.CorrelationId,
            context.TargetFpsYear,
            context.CurrentFpsYear);

        if (_steps.Count == 0)
        {
            throw new InvalidOperationException("Year End Data Setup has no registered execution steps.");
        }

        for (var i = 0; i < _steps.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var step = _steps[i];
            var startedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "YearEndDataSetup step started | CorrelationId={CorrelationId} | StepIndex={StepIndex} | StepCount={StepCount} | StepName={StepName}",
                context.CorrelationId,
                i + 1,
                _steps.Count,
                step.Name);

            await step.ExecuteAsync(context, cancellationToken);

            var elapsedMs = (DateTime.UtcNow - startedAt).TotalMilliseconds;
            _logger.LogInformation(
                "YearEndDataSetup step completed | CorrelationId={CorrelationId} | StepIndex={StepIndex} | StepCount={StepCount} | StepName={StepName} | DurationMs={DurationMs}",
                context.CorrelationId,
                i + 1,
                _steps.Count,
                step.Name,
                elapsedMs);
        }

        _logger.LogInformation(
            "YearEndDataSetup pipeline completed | CorrelationId={CorrelationId}",
            context.CorrelationId);
    }
}
