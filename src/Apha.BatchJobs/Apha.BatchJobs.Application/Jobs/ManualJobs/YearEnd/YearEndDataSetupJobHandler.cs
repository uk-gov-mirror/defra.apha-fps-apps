using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd;

/// <summary>
/// Year End Data Setup batch job handler scaffold.
/// Execution logic will be added in the service layer in a later slice.
/// </summary>
public sealed class YearEndDataSetupJobHandler : IBatchJob
{
    private readonly ILogger<YearEndDataSetupJobHandler> _logger;
    private readonly ICorrelationContextAccessor _correlationService;
    private readonly IYearEndDataSetupService _service;

    public string Name => BatchJobNames.YearEndDataSetup;

    public string IdempotencyStrategy => "ApprovedRowClaimWithYearEndLock";

    public string? ScheduleExpression => null;

    public string? ScheduleDescription => "Manual approval-triggered Year End Data Setup";

    public int? MaxExecutionSeconds => 10800;

    public YearEndDataSetupJobHandler(
        IYearEndDataSetupService service,
        ICorrelationContextAccessor correlationService,
        ILogger<YearEndDataSetupJobHandler> logger)
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
            "YearEndDataSetup handler invoked | JobExecutionId={JobExecutionId} | TargetFpsYear={TargetFpsYear} | CurrentFpsYear={CurrentFpsYear}",
            context.CorrelationId,
            context.TargetFpsYear,
            context.CurrentFpsYear);

        using var stepScope = _logger.BeginScope(new Dictionary<string, object?> { ["StepName"] = "ExecuteYearEndDataSetup" });
        await _service.ExecuteAsync(context, cancellationToken);
    }
}