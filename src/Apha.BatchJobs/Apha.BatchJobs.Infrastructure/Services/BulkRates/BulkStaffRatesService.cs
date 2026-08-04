using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.BulkRates;
using Apha.Common.BulkRates.Validation.StaffAnimal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Apha.BatchJobs.Infrastructure.Services.BulkRates;

/// <summary>
/// Infrastructure implementation of <see cref="IBulkStaffRatesService"/>.
/// Applies Staff profit-centre grade annual rate changes (PayRate, NPR, OHR)
/// inside a single database transaction, writes permanent history, and
/// clears request-scoped staging rows on success.
///
/// Revalidates against the shared
/// <see cref="IStaffAnimalValidationService"/> inside the same transaction that applies the
/// changes, against rows locked with SELECT ... FOR UPDATE — not before the transaction
/// opens, and not against an unlocked read. The re-derived per-row classification is
/// compared against the calculated_action/source_*/effective_* frozen at release time;
/// a disagreement means live data changed since release in a way that would
/// alter what the approver reviewed, so the request fails rather than silently applying a
/// different outcome.
/// </summary>
public sealed class BulkStaffRatesService : IBulkStaffRatesService
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IBulkRatesRepository _repository;
    private readonly IStaffAnimalValidationService _validationService;
    private readonly ILogger<BulkStaffRatesService> _logger;

    public BulkStaffRatesService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IBulkRatesRepository repository,
        IStaffAnimalValidationService validationService,
        ILogger<BulkStaffRatesService> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
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
        // ── 2. Load staging (including the frozen source_*/effective_*/
        // calculated_action/validation_version columns the release-time freeze wrote) ──────
        var stagingRows = await _repository.GetStaffStagingRowsAsync(jobQueueId, cancellationToken);

        if (stagingRows.Count == 0)
        {
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: no staging rows found for JobQueueId={jobQueueId:D}.");
        }

        _logger.LogInformation(
            "BulkStaffRatesUpdate staging loaded | JobQueueId={JobQueueId} | Rows={Rows}",
            jobQueueId, stagingRows.Count);

        // ── 3. Execute all mutations in one transaction ───────────────────
        int updated = 0, unchanged = 0;

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using (var tx = await conn.BeginTransactionAsync(cancellationToken))
        {
            // ── Lock the specific live rows this upload targets, then
            // revalidate against them under that lock — never an unlocked pre-transaction
            // read. Deterministic lock order (business key ascending) reduces deadlock risk.
            var pcGrades = stagingRows
                .Select(r => r.PcGrade)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var liveLookup = await GetStaffRowsForUpdateAsync(conn, tx, pcGrades, fpsYear, cancellationToken);

            var stagedForValidation = stagingRows.Select((r, i) => new ValidationStaffRow
            {
                PcGrade = r.PcGrade,
                PayRate = r.PayRate,
                Npr = r.Npr,
                Ohr = r.Ohr,
                SourceRow = i + 2
            }).ToList();

            var validationContext = new StaffAnimalValidationContext
            {
                JobQueueId = jobQueueId,
                FpsYear = fpsYear,
                LiveStaffLookup = liveLookup,
                LiveAnimalLookup = new Dictionary<string, LiveAnimalRow>(),
                StagedStaffRows = stagedForValidation,
                StagedAnimalRows = []
            };

            var rederivedByKey = _validationService.Validate(validationContext).StaffResults
                .ToDictionary(r => StaffAnimalValidationKeys.PcGrade(r.PcGrade));

            // Drift check — the frozen calculated_action/source_*/effective_*
            // must still match what re-derivation just computed against the locked
            // live rows. A row whose business key no longer resolves live is not skipped
            // (the pre-parity job's old behavior) — it fails here too: NotFound is a
            // hard failure at whichever stage it's detected.
            foreach (var row in stagingRows)
            {
                var key = StaffAnimalValidationKeys.PcGrade(row.PcGrade);
                liveLookup.TryGetValue(key, out var liveLocked);
                AssertNoDrift(row, liveLocked, rederivedByKey[key], jobQueueId);
            }

            foreach (var row in stagingRows)
            {
                var key = StaffAnimalValidationKeys.PcGrade(row.PcGrade);
                liveLookup.TryGetValue(key, out var liveLocked);

                if (row.CalculatedAction == StaffAnimalCalculatedAction.NoChange)
                {
                    unchanged++;
                    continue;
                }

                await UpdateStaffRowAsync(conn, tx, row, fpsYear, cancellationToken);
                foreach (var historyRow in BuildHistory(row, liveLocked, entry, appliedAt))
                    await BulkRatesRepository.InsertHistoryRowAsync(conn, tx, historyRow, cancellationToken);
                updated++;
            }

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "BulkStaffRatesUpdate committed | JobQueueId={JobQueueId} | Updated={Updated} | Unchanged={Unchanged}",
                jobQueueId, updated, unchanged);

            // ── US-XC-02: Log commit summary ──────────────────────────────
            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"Rate changes committed: Staff updated={updated}, unchanged={unchanged}.",
                entry.ApprovedBy, cancellationToken);
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

    // ── Drift check ──────────────────────────────────────────────────

    private static void AssertNoDrift(
        StaffStagingRow row, LiveStaffRow? liveLocked, StaffValidationResult rederived, Guid jobQueueId)
    {
        if (row.CalculatedAction is null)
        {
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: staging row '{row.PcGrade}' for JobQueueId={jobQueueId:D} has no frozen " +
                "calculated_action — release-time freeze did not run for this row.");
        }

        // Committed policy: a frozen validation_version that no longer matches the
        // currently-deployed rule set fails safely rather than applying a decision made
        // under superseded rules.
        if (row.ValidationVersion != StaffAnimalValidationVersion.Current)
        {
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: staging row '{row.PcGrade}' for JobQueueId={jobQueueId:D} was frozen under " +
                $"validation_version '{row.ValidationVersion}', but the deployed rule set is " +
                $"'{StaffAnimalValidationVersion.Current}'. Revalidate and release the request again.");
        }

        // Source-state drift: the approver approved a specific decision against a specific
        // live baseline — CalculatedAction alone can't detect the live value moving to a
        // third value that happens to re-derive the same action. Normalized comparison
        // (StaffAnimalFieldComparer) so a frozen 0 and a live NULL PayRate are not a false
        // drift (null and 0 are equivalent for Staff/Animal amounts). A
        // missing live row (liveLocked is null) normalizes every field to 0/false, which will
        // fail this check unless the frozen source also happened to be all-zero — in which
        // case the action-drift check just below (frozen action vs re-derived NotFound) still
        // catches it. Either way, a business key that no longer resolves live is a hard
        // failure here, never silently skipped.
        if (!StaffAnimalFieldComparer.AmountEquals(row.SourcePayRate, liveLocked?.PayRate) ||
            !StaffAnimalFieldComparer.AmountEquals(row.SourceNpr, liveLocked?.Npr) ||
            !StaffAnimalFieldComparer.AmountEquals(row.SourceOhr, liveLocked?.Ohr))
        {
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: revalidation drift detected for '{row.PcGrade}' in JobQueueId={jobQueueId:D} — " +
                $"source state at release was PayRate='{row.SourcePayRate}' Npr='{row.SourceNpr}' Ohr='{row.SourceOhr}' " +
                $"but the live row locked just now is PayRate='{liveLocked?.PayRate}' Npr='{liveLocked?.Npr}' Ohr='{liveLocked?.Ohr}'. " +
                "Live data changed after approval. The request was not applied. Download the latest rates and submit a new request.");
        }

        if (!string.Equals(row.CalculatedAction, rederived.Action, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: revalidation drift detected for '{row.PcGrade}' in JobQueueId={jobQueueId:D} — " +
                $"approved action was '{row.CalculatedAction}' but re-derivation now computes '{rederived.Action}'. " +
                "Live reference data changed since release; failing rather than applying an unreviewed outcome.");
        }

        // Audit-completeness guarantee: the values about to be written to
        // rate_change_history.newvalue must be provably identical to what the release-time freeze
        // recorded as "approved". Effective is deterministic from the staged rate for a given
        // action, so this should never actually fire — it turns that implicit invariant into
        // an enforced one rather than an inferred one.
        if (!StaffAnimalFieldComparer.AmountEquals(row.EffectivePayRate, rederived.Effective?.PayRate) ||
            !StaffAnimalFieldComparer.AmountEquals(row.EffectiveNpr, rederived.Effective?.Npr) ||
            !StaffAnimalFieldComparer.AmountEquals(row.EffectiveOhr, rederived.Effective?.Ohr))
        {
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: revalidation drift detected for '{row.PcGrade}' in JobQueueId={jobQueueId:D} — " +
                $"approved effective state was PayRate='{row.EffectivePayRate}' Npr='{row.EffectiveNpr}' Ohr='{row.EffectiveOhr}' " +
                $"but re-derivation now computes PayRate='{rederived.Effective?.PayRate}' Npr='{rederived.Effective?.Npr}' Ohr='{rederived.Effective?.Ohr}'. " +
                "Live reference data changed since release; failing rather than applying an unreviewed outcome.");
        }
    }

    /// <summary>
    /// Locks and reads the live fps.profitcentregrade rows this upload targets.
    /// Scoped to only the PcGrades present in this request's staging, not the whole year.
    /// Ordered by business key ascending before FOR UPDATE — a deterministic lock order
    /// reduces deadlock risk when concurrent requests target overlapping grades.
    /// </summary>
    private static async Task<Dictionary<string, LiveStaffRow>> GetStaffRowsForUpdateAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> pcGrades, int fpsYear,
        CancellationToken ct)
    {
        var result = new Dictionary<string, LiveStaffRow>();
        if (pcGrades.Count == 0)
            return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT pcgrade, payrate::numeric, npr::numeric, ohr::numeric
            FROM fps.profitcentregrade
            WHERE fpsyear = @fpsyear AND pcgrade = ANY(@grades)
            ORDER BY pcgrade
            FOR UPDATE;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("grades", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = pcGrades.ToArray()
        });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var pcGrade = r.GetString(0);
            result[StaffAnimalValidationKeys.PcGrade(pcGrade)] = new LiveStaffRow
            {
                PcGrade = pcGrade,
                PayRate = r.IsDBNull(1) ? null : r.GetDecimal(1),
                Npr = r.IsDBNull(2) ? null : r.GetDecimal(2),
                Ohr = r.IsDBNull(3) ? null : r.GetDecimal(3)
            };
        }
        return result;
    }

    /// <summary>
    /// Writes the frozen effective_* state (what the approver reviewed) — never the raw
    /// staged value — so the live row ends up exactly at the approved target regardless of
    /// what a blank cell in the uploaded workbook meant (blank normalizes
    /// to zero and is written like any other value, not left untouched).
    /// </summary>
    private static async Task UpdateStaffRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        StaffStagingRow row, int fpsYear,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            UPDATE fps.profitcentregrade
            SET payrate = @payrate::money,
                npr     = @npr::money,
                ohr     = @ohr::money
            WHERE pcgrade = @pcgrade AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("payrate", StaffAnimalFieldComparer.NormalizeAmount(row.EffectivePayRate));
        cmd.Parameters.AddWithValue("npr",     StaffAnimalFieldComparer.NormalizeAmount(row.EffectiveNpr));
        cmd.Parameters.AddWithValue("ohr",     StaffAnimalFieldComparer.NormalizeAmount(row.EffectiveOhr));
        cmd.Parameters.AddWithValue("pcgrade", row.PcGrade);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static RateChangeHistoryRow[] BuildHistory(
        StaffStagingRow row, LiveStaffRow? before,
        BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { pcGrade = row.PcGrade });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "Staff", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        var beforePayRate = StaffAnimalFieldComparer.NormalizeAmount(before?.PayRate);
        var beforeNpr     = StaffAnimalFieldComparer.NormalizeAmount(before?.Npr);
        var beforeOhr     = StaffAnimalFieldComparer.NormalizeAmount(before?.Ohr);
        var afterPayRate  = StaffAnimalFieldComparer.NormalizeAmount(row.EffectivePayRate);
        var afterNpr      = StaffAnimalFieldComparer.NormalizeAmount(row.EffectiveNpr);
        var afterOhr      = StaffAnimalFieldComparer.NormalizeAmount(row.EffectiveOhr);

        var rows = new List<RateChangeHistoryRow>();
        if (beforePayRate != afterPayRate)
            rows.Add(MakeRow(c, "payrate", beforePayRate.ToString(), afterPayRate.ToString(), "Update"));
        if (beforeNpr != afterNpr)
            rows.Add(MakeRow(c, "npr", beforeNpr.ToString(), afterNpr.ToString(), "Update"));
        if (beforeOhr != afterOhr)
            rows.Add(MakeRow(c, "ohr", beforeOhr.ToString(), afterOhr.ToString(), "Update"));
        return [.. rows];
    }

    private static RateChangeHistoryRow MakeRow(
        (Guid JobQueueId, Guid JobExecutionId, int JobId, int FpsYear,
         string RateCategory, string BusinessKeyJson,
         string? RequestedBy, string? ApprovedBy, DateTime AppliedAt) c,
        string field, string? oldVal, string? newVal, string changeType)
        => new(c.JobQueueId, c.JobExecutionId, c.JobId, c.FpsYear,
               c.RateCategory, c.BusinessKeyJson, field,
               oldVal, newVal, changeType, c.RequestedBy, c.ApprovedBy, c.AppliedAt);
}
