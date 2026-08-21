using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;

/// <summary>
/// Applies Animal annual rate changes (DailyRate, DefraDailyRate, PlanByWeek, Species, SecurityLevel)
/// inside a single database transaction, writes permanent history, and
/// clears request-scoped staging rows on success.
/// Drift detection and revalidation are the responsibility of the FPS approval flow;
/// the worker applies frozen effective values directly.
/// </summary>
public sealed class BulkAnimalRatesService : IBulkAnimalRatesService
{
    private readonly IBulkRatesRepository _repository;
    private readonly ILogger<BulkAnimalRatesService> _logger;

    public BulkAnimalRatesService(
        IBulkRatesRepository repository,
        ILogger<BulkAnimalRatesService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(BulkRatesExecutionContext context, CancellationToken cancellationToken = default)
    {
        // ── 1. Load Running, previously approved request ──────────────────
        var entry = await _repository.GetRunningRequestAsync(context.JobExecutionId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: no job_queue row found for JobExecutionId={context.JobExecutionId:D}.");

        ValidatePreconditions(entry, context);

        var jobQueueId = entry.JobQueueId;
        var fpsYear    = entry.FpsYear;
        var appliedAt  = DateTime.UtcNow;

        // ── US-XC-02: Log execution start ─────────────────────────────
        await _repository.WriteJobQueueLogAsync(
            jobQueueId,
            $"Worker execution starting (FPS year {fpsYear}).",
            entry.ApprovedBy, cancellationToken);
        _logger.LogInformation(
            "[BulkRates.ExecutionStarted] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear}",
            jobQueueId, entry.JobName, fpsYear);

        // ── 2. Load staging ───────────────────────────────────────────────
        var stagingRows = await _repository.GetAnimalStagingRowsAsync(jobQueueId, cancellationToken);

        if (stagingRows.Count == 0)
        {
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: no staging rows found for JobQueueId={jobQueueId:D}.");
        }

        _logger.LogInformation(
            "BulkAnimalRatesUpdate staging loaded | JobQueueId={JobQueueId} | Rows={Rows}",
            jobQueueId, stagingRows.Count);

        // ── 3. Apply all mutations in one transaction (repository-managed) ─
        var (inserted, updated, unchanged) = await _repository.ApplyAnimalRatesAsync(
            stagingRows, entry, appliedAt, cancellationToken);

        _logger.LogInformation(
            "BulkAnimalRatesUpdate committed | JobQueueId={JobQueueId} | Inserted={Inserted} | Updated={Updated} | Unchanged={Unchanged}",
            jobQueueId, inserted, updated, unchanged);

        // ── US-XC-02: Log commit summary ──────────────────────────────
        // Best-effort: rates are committed; a logging failure must not fail the job.
        try
        {
            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"Rate changes committed: Animal inserted={inserted}, updated={updated}, unchanged={unchanged}.",
                entry.ApprovedBy, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "BulkAnimalRatesUpdate execution log failed after commit; rate changes were already applied | JobQueueId={JobQueueId}",
                jobQueueId);
        }

        // ── 4. Delete staging post-commit ─────────────────────────────────
        // Best-effort cleanup: the rate change is already committed, so a failure here must not
        // fail the job or trigger a whole-job retry. Log and move on.
        try
        {
            await _repository.DeleteAnimalStagingRowsAsync(jobQueueId, cancellationToken);

            _logger.LogInformation(
                "BulkAnimalRatesUpdate staging cleared | JobQueueId={JobQueueId}",
                jobQueueId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "BulkAnimalRatesUpdate staging cleanup failed after commit; rate changes were already applied — staging rows may require manual cleanup | JobQueueId={JobQueueId}",
                jobQueueId);
        }
    }

    private static void ValidatePreconditions(BulkRatesJobQueueEntry entry, BulkRatesExecutionContext context)
    {
        // The orchestrator transitions Approved -> Running before invoking ExecuteAsync
        // (see JobOrchestrator.RunAsync), so by the time this runs the persisted status
        // is always 'Running' — checking for 'Approved' here would always fail.
        if (!string.Equals(entry.Status, "Running", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: request {entry.JobQueueId:D} is in status '{entry.Status}', expected 'Running'.");

        if (!string.Equals(entry.JobName, BatchJobNames.BulkAnimalRatesUpdate, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: JobExecutionId {context.JobExecutionId:D} belongs to job '{entry.JobName}'.");

        if (entry.FpsYear <= 0)
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: request {entry.JobQueueId:D} has no valid fpsyear.");

        if (context.TriggerYear.HasValue && context.TriggerYear.Value != entry.FpsYear)
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: trigger year {context.TriggerYear.Value} does not match persisted fpsyear {entry.FpsYear}.");

        if (string.IsNullOrWhiteSpace(entry.ApprovedBy) || !entry.ApprovedAtUtc.HasValue)
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: request {entry.JobQueueId:D} is missing approval metadata.");
    }
}
