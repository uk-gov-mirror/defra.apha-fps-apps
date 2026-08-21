using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;

/// <summary>
/// Applies FEC Test/Product (FEC before AGRUP, spec §15.2) annual rate changes
/// inside a single database transaction, then writes permanent history and
/// clears request-scoped staging rows on success.
/// Drift detection and revalidation are the responsibility of the FPS approval flow;
/// the worker applies frozen effective values directly.
/// </summary>
public sealed class BulkTestRatesService : IBulkTestRatesService
{
    private readonly IBulkRatesRepository _repository;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly ILogger<BulkTestRatesService> _logger;

    public BulkTestRatesService(
        IBulkRatesRepository repository,
        IJobExecutionRepository executionRepository,
        ILogger<BulkTestRatesService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(BulkRatesExecutionContext context, CancellationToken cancellationToken = default)
    {
        // ── 1. Load Running, previously approved request ──────────────────
        var entry = await _repository.GetRunningRequestAsync(context.JobExecutionId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"BulkTestRatesUpdate: no job_queue row found for JobExecutionId={context.JobExecutionId:D}.");

        ValidatePreconditions(entry, context);

        var jobQueueId = entry.JobQueueId;
        var fpsYear    = entry.FpsYear;
        var approvedBy = entry.ApprovedBy;
        var appliedAt  = DateTime.UtcNow;

        // ── US-XC-02: Log execution start ─────────────────────────────
        await _repository.WriteJobQueueLogAsync(
            jobQueueId,
            $"Worker execution starting (FPS year {fpsYear}).",
            approvedBy, cancellationToken);
        _logger.LogInformation(
            "[BulkRates.ExecutionStarted] JobQueueId={JobQueueId} | JobName={JobName} | FpsYear={FpsYear}",
            jobQueueId, entry.JobName, fpsYear);

        // ── 2. Load approved staging rows ──────────────────────────────────
        var fecRows   = await _repository.GetFecStagingRowsAsync(jobQueueId, cancellationToken);
        var agrupRows = await _repository.GetAgrupStagingRowsAsync(jobQueueId, cancellationToken);

        if (fecRows.Count == 0 && agrupRows.Count == 0)
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: no staging rows found for JobQueueId={jobQueueId:D}. " +
                "Request cannot be executed without approved data.");
        }

        _logger.LogInformation(
            "BulkTestRatesUpdate staging loaded | JobQueueId={JobQueueId} | FecRows={FecRows} | AgrupRows={AgrupRows}",
            jobQueueId, fecRows.Count, agrupRows.Count);

        // ── 3. Apply all mutations in one transaction (repository-managed) ─
        var (fecInserted, fecUpdated, fecUnchanged, agrupInserted, agrupUpdated, agrupUnchanged) =
            await _repository.ApplyFecRatesAsync(fecRows, agrupRows, entry, appliedAt, cancellationToken);

        _logger.LogInformation(
            "BulkTestRatesUpdate committed | JobQueueId={JobQueueId} | FecInserted={FI} | FecUpdated={FU} | FecUnchanged={FC} | AgrupInserted={AI} | AgrupUpdated={AU} | AgrupUnchanged={AC}",
            jobQueueId, fecInserted, fecUpdated, fecUnchanged, agrupInserted, agrupUpdated, agrupUnchanged);

        // ── US-XC-02: Log commit summary ──────────────────────────────
        // Best-effort: rates are committed; a logging failure must not fail the job.
        try
        {
            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"Rate changes committed: FEC inserted={fecInserted}, updated={fecUpdated}, unchanged={fecUnchanged}; AGRUP inserted={agrupInserted}, updated={agrupUpdated}, unchanged={agrupUnchanged}.",
                approvedBy, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "BulkTestRatesUpdate execution log failed after commit; rate changes were already applied | JobQueueId={JobQueueId}",
                jobQueueId);
        }

        // ── 4. Delete staging rows AFTER successful commit (spec §10.6) ──
        // Best-effort cleanup: the rate change is already committed, so a failure here must not
        // fail the job or trigger a whole-job retry. Log and move on.
        try
        {
            await _repository.DeleteFecStagingRowsAsync(jobQueueId, cancellationToken);

            _logger.LogInformation(
                "BulkTestRatesUpdate staging cleared | JobQueueId={JobQueueId}",
                jobQueueId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "BulkTestRatesUpdate staging cleanup failed after commit; rate changes were already applied — staging rows may require manual cleanup | JobQueueId={JobQueueId}",
                jobQueueId);
        }
    }

    private static void ValidatePreconditions(BulkRatesJobQueueEntry entry, BulkRatesExecutionContext context)
    {
        // The orchestrator transitions Approved -> Running before invoking ExecuteAsync
        // (see JobOrchestrator.RunAsync), so by the time this runs the persisted status
        // is always 'Running' — checking for 'Approved' here would always fail.
        if (!string.Equals(entry.Status, "Running", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: request {entry.JobQueueId:D} is in status '{entry.Status}', expected 'Running'.");
        }

        if (!string.Equals(entry.JobName, BatchJobNames.BulkTestRatesUpdate, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: JobExecutionId {context.JobExecutionId:D} belongs to job '{entry.JobName}', not '{BatchJobNames.BulkTestRatesUpdate}'.");
        }

        if (entry.FpsYear <= 0)
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: request {entry.JobQueueId:D} has no valid fpsyear.");
        }

        if (context.TriggerYear.HasValue && context.TriggerYear.Value != entry.FpsYear)
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: trigger year {context.TriggerYear.Value} does not match persisted fpsyear {entry.FpsYear}.");
        }

        if (string.IsNullOrWhiteSpace(entry.ApprovedBy) || !entry.ApprovedAtUtc.HasValue)
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: request {entry.JobQueueId:D} is missing approval metadata (approved_by/approved_at_utc).");
        }
    }
}
