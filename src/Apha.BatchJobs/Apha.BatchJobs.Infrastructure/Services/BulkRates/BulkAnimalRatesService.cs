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
/// Infrastructure implementation of <see cref="IBulkAnimalRatesService"/>.
/// Applies Animal annual rate changes (DailyRate, DefraDailyRate, PlanByWeek, Species, SecurityLevel)
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
public sealed class BulkAnimalRatesService : IBulkAnimalRatesService
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IBulkRatesRepository _repository;
    private readonly IStaffAnimalValidationService _validationService;
    private readonly ILogger<BulkAnimalRatesService> _logger;

    public BulkAnimalRatesService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IBulkRatesRepository repository,
        IStaffAnimalValidationService validationService,
        ILogger<BulkAnimalRatesService> logger)
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
        // ── 2. Load staging (including the frozen source_*/effective_*/
        // calculated_action/validation_version columns the release-time freeze wrote) ──────
        var stagingRows = await _repository.GetAnimalStagingRowsAsync(jobQueueId, cancellationToken);

        if (stagingRows.Count == 0)
        {
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: no staging rows found for JobQueueId={jobQueueId:D}.");
        }

        _logger.LogInformation(
            "BulkAnimalRatesUpdate staging loaded | JobQueueId={JobQueueId} | Rows={Rows}",
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
            var animalTypes = stagingRows
                .Select(r => r.AnimalType)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var liveLookup = await GetAnimalRowsForUpdateAsync(conn, tx, animalTypes, fpsYear, cancellationToken);

            var stagedForValidation = stagingRows.Select((r, i) => new ValidationAnimalRow
            {
                AnimalType = r.AnimalType,
                DailyRate = r.DailyRate,
                DefraDailyRate = r.DefraDailyRate,
                PlanByWeek = r.PlanByWeek,
                Species = r.Species,
                SecurityLevel = r.SecurityLevel,
                SourceRow = i + 2
            }).ToList();

            var validationContext = new StaffAnimalValidationContext
            {
                JobQueueId = jobQueueId,
                FpsYear = fpsYear,
                LiveStaffLookup = new Dictionary<string, LiveStaffRow>(),
                LiveAnimalLookup = liveLookup,
                StagedStaffRows = [],
                StagedAnimalRows = stagedForValidation
            };

            var rederivedByKey = _validationService.Validate(validationContext).AnimalResults
                .ToDictionary(r => StaffAnimalValidationKeys.AnimalType(r.AnimalType));

            // Drift check — the frozen calculated_action/source_*/effective_*
            // must still match what re-derivation just computed against the locked
            // live rows. A row whose business key no longer resolves live is not skipped
            // (the pre-parity job's old behavior) — it fails here too: NotFound is a
            // hard failure at whichever stage it's detected.
            foreach (var row in stagingRows)
            {
                var key = StaffAnimalValidationKeys.AnimalType(row.AnimalType);
                liveLookup.TryGetValue(key, out var liveLocked);
                AssertNoDrift(row, liveLocked, rederivedByKey[key], jobQueueId);
            }

            foreach (var row in stagingRows)
            {
                var key = StaffAnimalValidationKeys.AnimalType(row.AnimalType);
                liveLookup.TryGetValue(key, out var liveLocked);

                if (row.CalculatedAction == StaffAnimalCalculatedAction.NoChange)
                {
                    unchanged++;
                    continue;
                }

                await UpdateAnimalRowAsync(conn, tx, row, fpsYear, cancellationToken);
                foreach (var historyRow in BuildHistory(row, liveLocked, entry, appliedAt))
                    await BulkRatesRepository.InsertHistoryRowAsync(conn, tx, historyRow, cancellationToken);
                updated++;
            }

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "BulkAnimalRatesUpdate committed | JobQueueId={JobQueueId} | Updated={Updated} | Unchanged={Unchanged}",
                jobQueueId, updated, unchanged);

            // ── US-XC-02: Log commit summary ──────────────────────────────
            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"Rate changes committed: Animal updated={updated}, unchanged={unchanged}.",
                entry.ApprovedBy, cancellationToken);
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

    // ── Drift check ──────────────────────────────────────────────────

    private static void AssertNoDrift(
        AnimalStagingRow row, LiveAnimalRow? liveLocked, AnimalValidationResult rederived, Guid jobQueueId)
    {
        if (row.CalculatedAction is null)
        {
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: staging row '{row.AnimalType}' for JobQueueId={jobQueueId:D} has no frozen " +
                "calculated_action — release-time freeze did not run for this row.");
        }

        // Committed policy: a frozen validation_version that no longer matches the
        // currently-deployed rule set fails safely rather than applying a decision made
        // under superseded rules.
        if (row.ValidationVersion != StaffAnimalValidationVersion.Current)
        {
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: staging row '{row.AnimalType}' for JobQueueId={jobQueueId:D} was frozen under " +
                $"validation_version '{row.ValidationVersion}', but the deployed rule set is " +
                $"'{StaffAnimalValidationVersion.Current}'. Revalidate and release the request again.");
        }

        // Source-state drift across all five mutable fields, normalized
        // comparison (StaffAnimalFieldComparer) so a frozen 0/false/null and a live
        // NULL/false/blank are not a false drift. A missing live row (liveLocked is null)
        // normalizes every field to its "empty" value, which will fail this check unless the
        // frozen source also happened to be all-empty — in which case the action-drift check
        // just below (frozen action vs re-derived NotFound) still catches it. Either way, a
        // business key that no longer resolves live is a hard failure here, never silently
        // skipped.
        if (!StaffAnimalFieldComparer.AmountEquals(row.SourceDailyRate, liveLocked?.DailyRate) ||
            !StaffAnimalFieldComparer.AmountEquals(row.SourceDefraDailyRate, liveLocked?.DefraDailyRate) ||
            !StaffAnimalFieldComparer.FlagEquals(row.SourcePlanByWeek, liveLocked?.PlanByWeek ?? false) ||
            !StaffAnimalFieldComparer.TextEquals(row.SourceSpecies, liveLocked?.Species) ||
            !StaffAnimalFieldComparer.TextEquals(row.SourceSecurityLevel, liveLocked?.SecurityLevel))
        {
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: revalidation drift detected for '{row.AnimalType}' in JobQueueId={jobQueueId:D} — " +
                $"source state at release was DailyRate='{row.SourceDailyRate}' DefraDailyRate='{row.SourceDefraDailyRate}' " +
                $"PlanByWeek='{row.SourcePlanByWeek}' Species='{row.SourceSpecies}' SecurityLevel='{row.SourceSecurityLevel}' " +
                $"but the live row locked just now is DailyRate='{liveLocked?.DailyRate}' DefraDailyRate='{liveLocked?.DefraDailyRate}' " +
                $"PlanByWeek='{liveLocked?.PlanByWeek}' Species='{liveLocked?.Species}' SecurityLevel='{liveLocked?.SecurityLevel}'. " +
                "Live data changed after approval. The request was not applied. Download the latest rates and submit a new request.");
        }

        if (!string.Equals(row.CalculatedAction, rederived.Action, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: revalidation drift detected for '{row.AnimalType}' in JobQueueId={jobQueueId:D} — " +
                $"approved action was '{row.CalculatedAction}' but re-derivation now computes '{rederived.Action}'. " +
                "Live reference data changed since release; failing rather than applying an unreviewed outcome.");
        }

        // Audit-completeness guarantee: the values about to be written to
        // rate_change_history.newvalue must be provably identical to what the release-time freeze
        // recorded as "approved". Effective is deterministic from the staged rate for a given
        // action, so this should never actually fire — it turns that implicit invariant into
        // an enforced one rather than an inferred one.
        if (!StaffAnimalFieldComparer.AmountEquals(row.EffectiveDailyRate, rederived.Effective?.DailyRate) ||
            !StaffAnimalFieldComparer.AmountEquals(row.EffectiveDefraDailyRate, rederived.Effective?.DefraDailyRate) ||
            !StaffAnimalFieldComparer.FlagEquals(row.EffectivePlanByWeek, rederived.Effective?.PlanByWeek ?? false) ||
            !StaffAnimalFieldComparer.TextEquals(row.EffectiveSpecies, rederived.Effective?.Species) ||
            !StaffAnimalFieldComparer.TextEquals(row.EffectiveSecurityLevel, rederived.Effective?.SecurityLevel))
        {
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: revalidation drift detected for '{row.AnimalType}' in JobQueueId={jobQueueId:D} — " +
                "approved effective state did not match re-derivation. " +
                "Live reference data changed since release; failing rather than applying an unreviewed outcome.");
        }
    }

    /// <summary>
    /// Locks and reads the live fps.tblanimals rows this upload targets. Scoped to
    /// only the AnimalTypes present in this request's staging, not the whole year. Ordered by
    /// business key ascending before FOR UPDATE — a deterministic lock order reduces deadlock
    /// risk when concurrent requests target overlapping animal types.
    /// </summary>
    private static async Task<Dictionary<string, LiveAnimalRow>> GetAnimalRowsForUpdateAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> animalTypes, int fpsYear,
        CancellationToken ct)
    {
        var result = new Dictionary<string, LiveAnimalRow>();
        if (animalTypes.Count == 0)
            return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT animaltype, species, security_level, dailyrate::numeric, defradailyrate::numeric, planbyweek
            FROM fps.tblanimals
            WHERE fpsyear = @fpsyear AND animaltype = ANY(@types)
            ORDER BY animaltype
            FOR UPDATE;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("types", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = animalTypes.ToArray()
        });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var animalType = r.GetString(0);
            result[StaffAnimalValidationKeys.AnimalType(animalType)] = new LiveAnimalRow
            {
                AnimalType = animalType,
                Species = r.IsDBNull(1) ? null : r.GetString(1),
                SecurityLevel = r.IsDBNull(2) ? null : r.GetString(2),
                DailyRate = r.IsDBNull(3) ? null : r.GetDecimal(3),
                DefraDailyRate = r.IsDBNull(4) ? null : r.GetDecimal(4),
                PlanByWeek = !r.IsDBNull(5) && r.GetBoolean(5)
            };
        }
        return result;
    }

    /// <summary>
    /// Writes the frozen effective_* state (what the approver reviewed) — never the raw
    /// staged value — so the live row ends up exactly at the approved target regardless of
    /// what a blank cell in the uploaded workbook meant (blank normalizes
    /// to zero/false and is written like any other value, not left untouched).
    /// </summary>
    private static async Task UpdateAnimalRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        AnimalStagingRow row, int fpsYear,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            UPDATE fps.tblanimals
            SET dailyrate      = @dailyrate::money,
                defradailyrate = @defradailyrate::money,
                planbyweek     = @planbyweek,
                species        = @species,
                security_level = @security_level
            WHERE animaltype = @animaltype AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("dailyrate",      StaffAnimalFieldComparer.NormalizeAmount(row.EffectiveDailyRate));
        cmd.Parameters.AddWithValue("defradailyrate", StaffAnimalFieldComparer.NormalizeAmount(row.EffectiveDefraDailyRate));
        cmd.Parameters.AddWithValue("planbyweek",     StaffAnimalFieldComparer.NormalizeFlag(row.EffectivePlanByWeek));
        cmd.Parameters.AddWithValue("species",        (object?)StaffAnimalFieldComparer.NormalizeText(row.EffectiveSpecies) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("security_level", (object?)StaffAnimalFieldComparer.NormalizeText(row.EffectiveSecurityLevel) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("animaltype",     row.AnimalType);
        cmd.Parameters.AddWithValue("fpsyear",        fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static RateChangeHistoryRow[] BuildHistory(
        AnimalStagingRow row, LiveAnimalRow? before,
        BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { animalType = row.AnimalType });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "Animal", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        var beforeDailyRate      = StaffAnimalFieldComparer.NormalizeAmount(before?.DailyRate);
        var beforeDefraDailyRate = StaffAnimalFieldComparer.NormalizeAmount(before?.DefraDailyRate);
        var beforePlanByWeek     = before?.PlanByWeek ?? false;
        var beforeSpecies        = StaffAnimalFieldComparer.NormalizeText(before?.Species);
        var beforeSecurityLevel  = StaffAnimalFieldComparer.NormalizeText(before?.SecurityLevel);

        var afterDailyRate      = StaffAnimalFieldComparer.NormalizeAmount(row.EffectiveDailyRate);
        var afterDefraDailyRate = StaffAnimalFieldComparer.NormalizeAmount(row.EffectiveDefraDailyRate);
        var afterPlanByWeek     = StaffAnimalFieldComparer.NormalizeFlag(row.EffectivePlanByWeek);
        var afterSpecies        = StaffAnimalFieldComparer.NormalizeText(row.EffectiveSpecies);
        var afterSecurityLevel  = StaffAnimalFieldComparer.NormalizeText(row.EffectiveSecurityLevel);

        var rows = new List<RateChangeHistoryRow>();
        if (beforeDailyRate != afterDailyRate)
            rows.Add(MakeRow(c, "dailyrate", beforeDailyRate.ToString(), afterDailyRate.ToString(), "Update"));
        if (beforeDefraDailyRate != afterDefraDailyRate)
            rows.Add(MakeRow(c, "defradailyrate", beforeDefraDailyRate.ToString(), afterDefraDailyRate.ToString(), "Update"));
        if (beforePlanByWeek != afterPlanByWeek)
            rows.Add(MakeRow(c, "planbyweek", beforePlanByWeek.ToString(), afterPlanByWeek.ToString(), "Update"));
        if (!string.Equals(beforeSpecies, afterSpecies, StringComparison.OrdinalIgnoreCase))
            rows.Add(MakeRow(c, "species", beforeSpecies, afterSpecies, "Update"));
        if (!string.Equals(beforeSecurityLevel, afterSecurityLevel, StringComparison.OrdinalIgnoreCase))
            rows.Add(MakeRow(c, "security_level", beforeSecurityLevel, afterSecurityLevel, "Update"));
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
