using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd;

/// <summary>
/// Year End Cutover batch job handler scaffold.
/// Execution logic will be added in the service layer in a later slice.
/// </summary>
public sealed class YearEndCutoverJobHandler : IBatchJob
{
    private readonly ILogger<YearEndCutoverJobHandler> _logger;
    private readonly ICorrelationService _correlationService;
    private readonly IYearEndCutoverService _service;

    public string Name => BatchJobNames.YearEndCutover;

    public string IdempotencyStrategy => "ApprovedRowClaimWithYearEndLock";

    public string? ScheduleExpression => null;

    public string? ScheduleDescription => "Manual approval-triggered Year End Cutover";

    public int? MaxExecutionSeconds => 10800;

    public YearEndCutoverJobHandler(
        IYearEndCutoverService service,
        ICorrelationService correlationService,
        ILogger<YearEndCutoverJobHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var context = YearEndExecutionContext.FromEnvironment(_correlationService.GetCorrelationId());

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["JobExecutionId"] = context.CorrelationId,
            ["JobName"] = Name
        });

        _logger.LogInformation(
            "YearEndCutover handler invoked | JobExecutionId={JobExecutionId} | TargetFpsYear={TargetFpsYear} | CurrentFpsYear={CurrentFpsYear}",
            context.CorrelationId,
            context.TargetFpsYear,
            context.CurrentFpsYear);

        using var stepScope = _logger.BeginScope(new Dictionary<string, object?> { ["StepName"] = "ExecuteYearEndCutover" });
        await _service.ExecuteAsync(context, cancellationToken);
    }
}