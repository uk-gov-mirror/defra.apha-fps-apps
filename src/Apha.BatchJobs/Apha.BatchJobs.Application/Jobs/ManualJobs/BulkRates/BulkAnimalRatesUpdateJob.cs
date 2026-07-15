using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;

/// <summary>
/// Batch job handler for BulkAnimalRatesUpdate.
/// Orchestrates Animal annual rate changes triggered by an approved request in fps.job_queue.
/// Lock lifecycle is owned exclusively by <see cref="JobOrchestrator"/>.
/// </summary>
public sealed class BulkAnimalRatesUpdateJob : IBatchJob
{
    private readonly IBulkAnimalRatesService _service;
    private readonly ICorrelationService _correlationService;
    private readonly ILogger<BulkAnimalRatesUpdateJob> _logger;

    public string Name => BatchJobNames.BulkAnimalRatesUpdate;
    public string IdempotencyStrategy => "ApprovedRowClaimWithJobLock";
    public string? ScheduleExpression => null;
    public string? ScheduleDescription => "Manual approval-triggered Animal bulk rate update";
    public int? MaxExecutionSeconds => 1800;

    public BulkAnimalRatesUpdateJob(
        IBulkAnimalRatesService service,
        ICorrelationService correlationService,
        ILogger<BulkAnimalRatesUpdateJob> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _correlationService = correlationService ?? throw new ArgumentNullException(nameof(correlationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var context = BulkRatesExecutionContext.FromEnvironment(_correlationService.GetCorrelationId());

        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["JobExecutionId"] = context.JobExecutionId,
            ["JobName"] = context.JobName
        });

        _logger.LogInformation(
            "BulkAnimalRatesUpdate handler invoked | JobExecutionId={JobExecutionId} | TriggerYear={TriggerYear}",
            context.JobExecutionId,
            context.TriggerYear);

        using var stepScope = _logger.BeginScope(new Dictionary<string, object?> { ["StepName"] = "ExecuteBulkAnimalRatesUpdate" });
        await _service.ExecuteAsync(context, cancellationToken);
    }
}
