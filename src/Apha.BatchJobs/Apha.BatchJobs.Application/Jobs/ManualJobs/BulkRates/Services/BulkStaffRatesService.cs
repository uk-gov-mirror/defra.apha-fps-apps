using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;

/// <summary>
/// Applies Staff profit-centre grade annual rate changes (PayRate, NPR, OHR)
/// inside a single database transaction, writes permanent history, and
/// clears request-scoped staging rows on success.
/// Drift detection and revalidation are the responsibility of the FPS approval flow;
/// the worker applies frozen effective values directly.
/// </summary>
public sealed class BulkStaffRatesService : IBulkStaffRatesService
{
    private readonly IBulkRatesRepository _repository;
    private readonly ILogger<BulkStaffRatesService> _logger;

    public BulkStaffRatesService(
        IBulkRatesRepository repository,
        ILogger<BulkStaffRatesService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(BulkRatesExecutionContext context, CancellationToken cancellationToken = default)
    {
        // ── 1. Load Running, previously approved request ──────────────────
        var entry = await _repository.GetRunningRequestAsync(context.JobExecutionId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: no job_queue row found for JobExecutionId={context.JobExecutionId:D}.");

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
        var stagingRows = await _repository.GetStaffStagingRowsAsync(jobQueueId, cancellationToken);

        if (stagingRows.Count == 0)
        {
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: no staging rows found for JobQueueId={jobQueueId:D}.");
        }

        _logger.LogInformation(
            "BulkStaffRatesUpdate staging loaded | JobQueueId={JobQueueId} | Rows={Rows}",
            jobQueueId, stagingRows.Count);

        // ── 3. Apply all mutations in one transaction (repository-managed) ─
        var (updated, unchanged) = await _repository.ApplyStaffRatesAsync(
            stagingRows, entry, appliedAt, cancellationToken);

        _logger.LogInformation(
            "BulkStaffRatesUpdate committed | JobQueueId={JobQueueId} | Updated={Updated} | Unchanged={Unchanged}",
            jobQueueId, updated, unchanged);

        // ── US-XC-02: Log commit summary ──────────────────────────────
        // Best-effort: rates are committed; a logging failure must not fail the job.
        try
        {
            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"Rate changes committed: Staff updated={updated}, unchanged={unchanged}.",
                entry.ApprovedBy, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "BulkStaffRatesUpdate execution log failed after commit; rate changes were already applied | JobQueueId={JobQueueId}",
                jobQueueId);
        }

        // ── 4. Delete staging post-commit ─────────────────────────────────
        // Best-effort cleanup: the rate change is already committed, so a failure here must not
        // fail the job or trigger a whole-job retry. Log and move on.
        try
        {
            await _repository.DeleteStaffStagingRowsAsync(jobQueueId, cancellationToken);

            _logger.LogInformation(
                "BulkStaffRatesUpdate staging cleared | JobQueueId={JobQueueId}",
                jobQueueId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "BulkStaffRatesUpdate staging cleanup failed after commit; rate changes were already applied — staging rows may require manual cleanup | JobQueueId={JobQueueId}",
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
                $"BulkStaffRatesUpdate: request {entry.JobQueueId:D} is in status '{entry.Status}', expected 'Running'.");

        if (!string.Equals(entry.JobName, BatchJobNames.BulkStaffRatesUpdate, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: JobExecutionId {context.JobExecutionId:D} belongs to job '{entry.JobName}'.");

        if (entry.FpsYear <= 0)
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: request {entry.JobQueueId:D} has no valid fpsyear.");

        if (context.TriggerYear.HasValue && context.TriggerYear.Value != entry.FpsYear)
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: trigger year {context.TriggerYear.Value} does not match persisted fpsyear {entry.FpsYear}.");

        if (string.IsNullOrWhiteSpace(entry.ApprovedBy) || !entry.ApprovedAtUtc.HasValue)
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: request {entry.JobQueueId:D} is missing approval metadata.");
    }
}
