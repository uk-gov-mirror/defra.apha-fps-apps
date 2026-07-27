using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.BulkRates;
using Apha.Common.BulkRates.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Apha.BatchJobs.Infrastructure.Services.BulkRates;

/// <summary>
/// Infrastructure implementation of <see cref="IBulkTestRatesService"/>.
/// Applies FEC Test/Product (FEC before AGRUP, spec §15.2) annual rate changes
/// inside a single database transaction, then writes permanent history and
/// clears request-scoped staging rows on success.
///
/// DR-WK-01/02/03/04 (plan §5): revalidates against the shared
/// <see cref="IBulkRatesValidationService"/> (DR-VAL-01) inside the same transaction that
/// applies the changes, against rows locked with SELECT ... FOR UPDATE (plan §5.1) — not
/// before the transaction opens, and not against an unlocked read. The re-derived
/// per-row classification is compared against the calculated_action frozen at release time
/// (DR-API-07, CR056); a disagreement means live reference data changed since release in a
/// way that would alter what the approver reviewed, so the request fails rather than
/// silently applying a different outcome (plan §5.2).
/// </summary>
public sealed class BulkTestRatesService : IBulkTestRatesService
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IBulkRatesRepository _repository;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly IBulkRatesValidationService _validationService;
    private readonly ILogger<BulkTestRatesService> _logger;

    public BulkTestRatesService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IBulkRatesRepository repository,
        IJobExecutionRepository executionRepository,
        IBulkRatesValidationService validationService,
        ILogger<BulkTestRatesService> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(BulkRatesExecutionContext context, CancellationToken cancellationToken = default)
    {
        // ── 1. Load Running, previously approved request ──────────────────
        // Re-reading job_queue/staging here (rather than inside the write transaction below)
        // is safe: this request's own status/staging identity cannot change concurrently —
        // JobOrchestrator's job-name lock guarantees only one worker execution of this
        // JobQueueId runs at a time, and staging rows are frozen (no re-upload permitted)
        // once a request reaches Approved. The race plan §5.1 actually closes is on the
        // *live* fps.testorproduct/fps.tlkptestreqmt rows, which is why those specifically
        // move inside the transaction under FOR UPDATE below.
        var entry = await _repository.GetRunningRequestAsync(context.JobExecutionId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"BulkTestRatesUpdate: no job_queue row found for JobExecutionId={context.JobExecutionId:D}.");

        ValidatePreconditions(entry, context);

        var jobQueueId    = entry.JobQueueId;
        var jobId         = entry.JobId;
        var fpsYear       = entry.FpsYear;
        var requestedBy   = entry.RequestedBy;
        var approvedBy    = entry.ApprovedBy;
        var appliedAt     = DateTime.UtcNow;

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

        // ── 3. Execute all mutations in one transaction ───────────────────
        var historyRows = new List<RateChangeHistoryRow>();

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using (var tx = await conn.BeginTransactionAsync(cancellationToken))
        {
            // ── Plan §5.1 steps 4-5: lock the specific live rows this upload targets,
            // then revalidate against them under that lock. ──────────────────────────
            var testCodes = fecRows.Select(r => r.TestCode)
                .Concat(agrupRows.Select(r => r.TestCode))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var liveFecLookup = await GetFecRowsForUpdateAsync(conn, tx, testCodes, fpsYear, cancellationToken);
            var liveAgrupLookup = await GetAgrupRowsForUpdateAsync(conn, tx, testCodes, fpsYear, cancellationToken);

            var projectCodes = agrupRows
                .Where(r => !string.IsNullOrWhiteSpace(r.ProjectBuyerCode))
                .Select(r => r.ProjectBuyerCode!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var projectLookup = await GetExistingProjectCodesAsync(conn, tx, projectCodes, fpsYear, cancellationToken);

            var capabilityPairs = agrupRows
                .Where(r => !string.IsNullOrWhiteSpace(r.TestBuyerWorkGroup))
                .Select(r => (r.TestCode, r.TestBuyerWorkGroup!))
                .ToHashSet();
            var capabilityLookup = await GetExistingCapabilityPairsAsync(conn, tx, capabilityPairs, fpsYear, cancellationToken);

            IReadOnlyList<DownloadedSnapshotKey> frozenSnapshot = entry.ActiveDownloadVersion.HasValue
                ? await GetSnapshotRowsAsync(conn, tx, jobQueueId, entry.ActiveDownloadVersion.Value, cancellationToken)
                : [];

            // SourceRow has no worksheet to point back to at execution time (this is a DB
            // read-back, not a fresh parse) — synthetic index+2, matching BuildFreezeAsync's
            // own release-time read-back convention. Findings here drive fail/apply decisions
            // only, never a persisted per-row error, so the exact value is not user-facing.
            var stagedFec = fecRows.Select((r, i) => new ValidationFecRow
            {
                TestCode = r.TestCode,
                FecNewRate = r.FecNewRate,
                ItemDescription = r.ItemDescription,
                ShortDescription = r.ShortDescription,
                Owner = r.Owner,
                Comments = r.Comments,
                SourceRow = i + 2
            }).ToList();

            var stagedAgrup = agrupRows.Select((r, i) => new ValidationAgrupRow
            {
                TestCode = r.TestCode,
                Buyer = r.Buyer,
                AgrupNew = r.AgrupNew,
                ProjectBuyerCode = r.ProjectBuyerCode,
                TestBuyerCode = r.TestBuyerCode,
                TestBuyerWorkGroup = r.TestBuyerWorkGroup,
                Comments = r.Comments,
                SourceRow = i + 2
            }).ToList();

            var validationContext = new ValidationContext
            {
                JobQueueId = jobQueueId,
                FpsYear = fpsYear,
                DownloadVersion = entry.ActiveDownloadVersion,
                UploadVersion = entry.UploadVersion!.Value,
                LiveFecLookup = liveFecLookup,
                LiveAgrupLookup = liveAgrupLookup,
                ProjectLookup = projectLookup,
                CapabilityLookup = capabilityLookup,
                StagedFecRows = stagedFec,
                StagedAgrupRows = stagedAgrup,
                FrozenSnapshot = frozenSnapshot,
                // Worker-only: BC-05 interim check for a live positive AGRUP row under a
                // withdrawn FEC TestCode that was never part of the download snapshot
                // (plan §5.2) — must never run at API release time (ValidationContext docs).
                IncludeWorkerOnlyChecks = true
            };

            var findings = _validationService.Validate(validationContext);

            var blockingErrors = findings.Where(f => f.Severity == ValidationSeverity.Error).ToList();
            if (blockingErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"BulkTestRatesUpdate: worker revalidation blocked JobQueueId={jobQueueId:D} " +
                    $"with {blockingErrors.Count} issue(s): {string.Join(" | ", blockingErrors.Select(f => f.Message))}");
            }

            // Every remaining staged row has exactly one ROW_CLASSIFIED finding at this point —
            // any row that lacked one also carried a blocking Error above, which already threw.
            var fecClassifications = findings
                .Where(f => f.ValidationCode == "ROW_CLASSIFIED" && string.Equals(f.Sheet, "FEC", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(f => BulkRatesValidationKeys.TestCode(f.BusinessKey!));
            var agrupClassifications = findings
                .Where(f => f.ValidationCode == "ROW_CLASSIFIED" && string.Equals(f.Sheet, "AGRUP", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(f => SplitAgrupBusinessKey(f.BusinessKey!));

            // Plan §5.2 drift check — the frozen calculated_action (DR-API-07) must still
            // match what re-derivation just computed against the locked live rows, AND the
            // live source rate itself must still match what was frozen at release. The approver
            // approved a specific decision against a specific source state (source_current_rate);
            // if the live rate has since moved to a third value, that decision is stale even
            // though the action label and approved rate haven't changed.
            //
            // FEC carries the source rate on two live columns (UnitPriceVla, DefraUnitPrice) —
            // Bulk Rates always writes both together (UpdateFecRowAsync), but a live row could
            // still have diverged between them via some other write path. Freezing only compares
            // against DefraUnitPrice (BulkRatesValidator.BuildFreezeAsync), so a worker-time
            // check against DefraUnitPrice alone could miss a row where UnitPriceVla is the one
            // that moved — e.g. frozen=100, DefraUnitPrice=100 (looks unchanged), UnitPriceVla=110
            // (actually drifted). Passing both live values in makes AssertNoDrift require each to
            // still equal the frozen source, which also transitively requires them to equal each
            // other. AGRUP has only one live rate column, so its call just passes that value twice.
            foreach (var row in fecRows)
            {
                liveFecLookup.TryGetValue(BulkRatesValidationKeys.TestCode(row.TestCode), out var liveFec);
                AssertNoDrift(row.TestCode, row.CalculatedAction, row.EffectiveNewRate,
                    row.SourceCurrentRate, liveFec?.DefraUnitPrice, liveFec?.UnitPriceVla,
                    fecClassifications[BulkRatesValidationKeys.TestCode(row.TestCode)], jobQueueId);
            }
            foreach (var row in agrupRows)
            {
                liveAgrupLookup.TryGetValue(BulkRatesValidationKeys.AgrupKey(row.TestCode, row.Buyer), out var liveAgrup);
                AssertNoDrift($"{row.TestCode}/{row.Buyer}", row.CalculatedAction, row.EffectiveNewRate,
                    row.SourceCurrentRate, liveAgrup?.UnitPrice, liveAgrup?.UnitPrice,
                    agrupClassifications[BulkRatesValidationKeys.AgrupKey(row.TestCode, row.Buyer)], jobQueueId);
            }

            // FEC Test/Product — inserts first, then updates (spec §15.2, §2.4)
            int fecInserted = 0, fecUpdated = 0, fecUnchanged = 0;
            foreach (var row in fecRows)
            {
                var classification = fecClassifications[BulkRatesValidationKeys.TestCode(row.TestCode)];
                liveFecLookup.TryGetValue(BulkRatesValidationKeys.TestCode(row.TestCode), out var live);
                var effectiveRate = classification.EffectiveNewRate ?? 0m;

                switch (classification.CalculatedAction)
                {
                    case ValidationCalculatedAction.Insert:
                        await InsertFecRowAsync(conn, tx, row, fpsYear, effectiveRate, cancellationToken);
                        historyRows.AddRange(BuildFecInsertHistory(row, entry, appliedAt, effectiveRate));
                        fecInserted++;
                        break;

                    case ValidationCalculatedAction.Update:
                    case ValidationCalculatedAction.ZeroRateWithdrawal:
                        await UpdateFecRowAsync(conn, tx, row.TestCode, fpsYear, effectiveRate, cancellationToken);
                        historyRows.AddRange(BuildFecUpdateHistory(
                            row, (live?.UnitPriceVla ?? 0m, live?.DefraUnitPrice ?? 0m),
                            entry, appliedAt, effectiveRate, classification.CalculatedAction));
                        fecUpdated++;
                        break;

                    default:
                        fecUnchanged++;
                        break;
                }
            }

            // AGRUP — after FEC (spec §2.4 sequencing rule)
            int agrupInserted = 0, agrupUpdated = 0, agrupUnchanged = 0;
            foreach (var row in agrupRows)
            {
                var classification = agrupClassifications[BulkRatesValidationKeys.AgrupKey(row.TestCode, row.Buyer)];
                liveAgrupLookup.TryGetValue(BulkRatesValidationKeys.AgrupKey(row.TestCode, row.Buyer), out var live);
                var effectiveRate = classification.EffectiveNewRate ?? 0m;

                switch (classification.CalculatedAction)
                {
                    case ValidationCalculatedAction.Insert:
                        await InsertAgrupRowAsync(conn, tx, row, fpsYear, effectiveRate, appliedAt, cancellationToken);
                        await WriteTestreqLogAsync(conn, tx,
                            row.TestCode, row.Buyer, fpsYear, effectiveRate,
                            row.NoRequired, row.ProjectBuyerCode, row.TestBuyerCode, active: 1,
                            appliedAt, approvedBy, "I", cancellationToken);
                        historyRows.AddRange(BuildAgrupInsertHistory(row, entry, appliedAt, effectiveRate));
                        agrupInserted++;
                        break;

                    case ValidationCalculatedAction.Update:
                    case ValidationCalculatedAction.ZeroRateWithdrawal:
                        await UpdateAgrupRowAsync(conn, tx, row.TestCode, row.Buyer, fpsYear, effectiveRate, cancellationToken);
                        await WriteTestreqLogAsync(conn, tx,
                            row.TestCode, row.Buyer, fpsYear, effectiveRate,
                            live?.NoRequired, live?.ProjectBuyerCode, live?.TestBuyerCode, live?.Active,
                            appliedAt, approvedBy, "I", cancellationToken);
                        historyRows.AddRange(BuildAgrupUpdateHistory(
                            row, live?.UnitPrice, entry, appliedAt, effectiveRate, classification.CalculatedAction));
                        agrupUpdated++;
                        break;

                    default:
                        agrupUnchanged++;
                        break;
                }
            }

            // ── 4. Write permanent history inside the transaction ─────────
            await WriteHistoryInsideTransactionAsync(conn, tx, historyRows, cancellationToken);

            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "BulkTestRatesUpdate committed | JobQueueId={JobQueueId} | FecInserted={FI} | FecUpdated={FU} | FecUnchanged={FC} | AgrupInserted={AI} | AgrupUpdated={AU} | AgrupUnchanged={AC}",
                jobQueueId, fecInserted, fecUpdated, fecUnchanged, agrupInserted, agrupUpdated, agrupUnchanged);

            // ── US-XC-02: Log commit summary ──────────────────────────────
            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"Rate changes committed: FEC inserted={fecInserted}, updated={fecUpdated}, unchanged={fecUnchanged}; AGRUP inserted={agrupInserted}, updated={agrupUpdated}, unchanged={agrupUnchanged}.",
                approvedBy, cancellationToken);
        }

        // ── 5. Delete staging rows AFTER successful commit (spec §10.6) ──
        // Best-effort cleanup: the rate change is already committed, so a failure here must not
        // fail the job or trigger a whole-job retry — that would re-run an already-applied change
        // against staging rows that (mostly) still need clearing. Log and move on.
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

    // ── Precondition validation ─────────────────────────────────────────────

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

        if (!entry.UploadVersion.HasValue)
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: request {entry.JobQueueId:D} is missing upload_version — cannot revalidate (DR-WK-04) without a known upload identity.");
        }
    }

    // ── Plan §5.2 drift check ────────────────────────────────────────────────

    private static void AssertNoDrift(
        string businessKey, string? frozenAction, decimal? frozenEffectiveRate,
        decimal? frozenSourceRate, decimal? currentSourceRatePrimary, decimal? currentSourceRateSecondary,
        ValidationFinding rederived, Guid jobQueueId)
    {
        if (frozenAction is null)
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: staging row '{businessKey}' for JobQueueId={jobQueueId:D} has no frozen " +
                "calculated_action — release-time freeze (DR-API-07) did not run for this row.");
        }

        // Plan §5.2 source-drift check: the approver approved a specific decision against a
        // specific source state — CalculatedAction/EffectiveNewRate alone can't detect the live
        // rate moving to a third value, since EffectiveNewRate is deterministic from the staged
        // rate (not the live baseline) and the action label doesn't change just because the
        // baseline moved to some other number that's still "not equal to the new rate". This is
        // the check that actually enforces "apply the approved change from the frozen source
        // state, provided that state is still valid at execution time" — must run before the
        // action/rate checks below, which cannot see this class of drift at all.
        //
        // Both live values must still equal the frozen source (for FEC, UnitPriceVla and
        // DefraUnitPrice; for AGRUP, the same single value passed twice) — requiring each to
        // match frozenSourceRate independently also transitively requires them to match each
        // other, so a FEC row where only one of the two columns has drifted (e.g. DefraUnitPrice
        // still 100 but UnitPriceVla now 110) is caught even though comparing DefraUnitPrice
        // alone would have missed it.
        if (frozenSourceRate != currentSourceRatePrimary || frozenSourceRate != currentSourceRateSecondary)
        {
            var currentDescription = currentSourceRatePrimary == currentSourceRateSecondary
                ? $"'{currentSourceRatePrimary}'"
                : $"'{currentSourceRatePrimary}' / '{currentSourceRateSecondary}'";
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: revalidation drift detected for '{businessKey}' in JobQueueId={jobQueueId:D} — " +
                $"source rate at release was '{frozenSourceRate}' but the live rate(s) locked just now are {currentDescription}. " +
                "Live rate changed after approval. The request was not applied. Download the latest rates and submit a new request.");
        }

        if (!string.Equals(frozenAction, rederived.CalculatedAction, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: revalidation drift detected for '{businessKey}' in JobQueueId={jobQueueId:D} — " +
                $"approved action was '{frozenAction}' but re-derivation now computes '{rederived.CalculatedAction}'. " +
                "Live reference data changed since release; failing rather than applying an unreviewed outcome.");
        }

        // Plan §7.5 audit-completeness guarantee: the value about to be written to
        // rate_change_history.newvalue must be provably identical to the value DR-API-07 froze
        // as "approved," not merely an action that happens to share a name. EffectiveNewRate is
        // deterministic from the staged rate for a given CalculatedAction (see
        // BulkRatesValidationService's classification branches), so this should never actually
        // fire — it turns that implicit invariant into an enforced one rather than an inferred one.
        if (frozenEffectiveRate != rederived.EffectiveNewRate)
        {
            throw new InvalidOperationException(
                $"BulkTestRatesUpdate: revalidation drift detected for '{businessKey}' in JobQueueId={jobQueueId:D} — " +
                $"approved effective rate was '{frozenEffectiveRate}' but re-derivation now computes '{rederived.EffectiveNewRate}'. " +
                "Live reference data changed since release; failing rather than applying an unreviewed outcome.");
        }
    }

    /// <summary>AGRUP ROW_CLASSIFIED findings carry "TestCode/Buyer" as a single BusinessKey string.</summary>
    private static (string TestCode, string Buyer) SplitAgrupBusinessKey(string businessKey)
    {
        var parts = businessKey.Split('/', 2);
        return BulkRatesValidationKeys.AgrupKey(parts[0], parts.Length > 1 ? parts[1] : string.Empty);
    }

    // ── FEC helpers ─────────────────────────────────────────────────────────

    /// <summary>
    /// Locks and reads the live fps.testorproduct rows this upload targets (plan §5.1 step 4).
    /// Scoped to only the TestCodes present in this request's staging, not the whole year.
    /// </summary>
    private static async Task<Dictionary<string, LiveFecRow>> GetFecRowsForUpdateAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> testCodes, int fpsYear,
        CancellationToken ct)
    {
        var result = new Dictionary<string, LiveFecRow>();
        if (testCodes.Count == 0)
            return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT itemcode, unitpricevla::numeric, defraunitprice::numeric
            FROM fps.testorproduct
            WHERE fpsyear = @fpsyear AND itemcode = ANY(@codes)
            FOR UPDATE;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("codes", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = testCodes.ToArray()
        });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var testCode = r.GetString(0);
            result[BulkRatesValidationKeys.TestCode(testCode)] = new LiveFecRow
            {
                TestCode = testCode,
                UnitPriceVla = r.IsDBNull(1) ? null : r.GetDecimal(1),
                DefraUnitPrice = r.IsDBNull(2) ? null : r.GetDecimal(2)
            };
        }
        return result;
    }

    private static async Task InsertFecRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        FecStagingRow row, int fpsYear, decimal rate,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            INSERT INTO fps.testorproduct
                (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
            VALUES
                (@itemcode, @itemdescription, @unitpricevla, @owner, @shortdescription, @defraunitprice, @fpsyear);";
        cmd.Parameters.AddWithValue("itemcode",         row.TestCode);
        cmd.Parameters.AddWithValue("itemdescription",  (object?)row.ItemDescription ?? DBNull.Value);
        cmd.Parameters.AddWithValue("unitpricevla",     rate);
        cmd.Parameters.AddWithValue("owner",            (object?)row.Owner ?? DBNull.Value);
        cmd.Parameters.AddWithValue("shortdescription", (object?)row.ShortDescription ?? DBNull.Value);
        cmd.Parameters.AddWithValue("defraunitprice",   rate);
        cmd.Parameters.AddWithValue("fpsyear",          fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task UpdateFecRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string testCode, int fpsYear, decimal newRate,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            UPDATE fps.testorproduct
            SET unitpricevla = @rate, defraunitprice = @rate
            WHERE itemcode = @itemcode AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("rate",    newRate);
        cmd.Parameters.AddWithValue("itemcode", testCode);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // ── AGRUP helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Locks and reads every live fps.tlkptestreqmt row under any TestCode this upload
    /// targets (plan §5.1 step 4) — by TestCode, not by the exact staged (TestCode,Buyer)
    /// keys, so the BC-05 interim check (plan §5.2) can see AGRUP buyers never present in
    /// the upload at all, not only the ones the staged rows happen to touch.
    /// </summary>
    private static async Task<Dictionary<(string TestCode, string Buyer), LiveAgrupRow>> GetAgrupRowsForUpdateAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> testCodes, int fpsYear,
        CancellationToken ct)
    {
        var result = new Dictionary<(string, string), LiveAgrupRow>();
        if (testCodes.Count == 0)
            return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT testcode, buyer, unitprice::numeric, projectbuyercode, testbuyercode,
                   norequired, active
            FROM fps.tlkptestreqmt
            WHERE fpsyear = @fpsyear AND testcode = ANY(@codes)
            FOR UPDATE;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("codes", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = testCodes.ToArray()
        });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var testCode = r.GetString(0);
            var buyer = r.GetString(1);
            result[BulkRatesValidationKeys.AgrupKey(testCode, buyer)] = new LiveAgrupRow
            {
                TestCode = testCode,
                Buyer = buyer,
                UnitPrice = r.IsDBNull(2) ? null : r.GetDecimal(2),
                ProjectBuyerCode = r.IsDBNull(3) ? null : r.GetString(3),
                TestBuyerCode = r.IsDBNull(4) ? null : r.GetString(4),
                NoRequired = r.IsDBNull(5) ? null : r.GetDouble(5),
                Active = r.IsDBNull(6) ? null : r.GetInt16(6)
            };
        }
        return result;
    }

    private static async Task InsertAgrupRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        AgrupStagingRow row, int fpsYear, decimal rate, DateTime executionTimestamp,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        // DR-WK-03: use the staged routing fields (CR056) instead of the old hardcoded
        // ProjectBuyerCode = Buyer / omitted TestBuyerCode — supersedes tracker A-14
        // (reconciliation §2.4, superseded-not-edited per that doc's own ground rule).
        cmd.CommandText = @"
            INSERT INTO fps.tlkptestreqmt
                (testcode, buyer, unitprice, norequired, projectbuyercode, testbuyercode, datecreated, active, fpsyear)
            VALUES
                (@testcode, @buyer, @unitprice, @norequired, @projectbuyercode, @testbuyercode, @datecreated, 1, @fpsyear);";
        cmd.Parameters.AddWithValue("testcode",   row.TestCode);
        cmd.Parameters.AddWithValue("buyer",      row.Buyer);
        cmd.Parameters.AddWithValue("unitprice",  rate);
        cmd.Parameters.AddWithValue("norequired", (object?)row.NoRequired ?? DBNull.Value);
        cmd.Parameters.AddWithValue("projectbuyercode", (object?)row.ProjectBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("testbuyercode",    (object?)row.TestBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("datecreated", executionTimestamp);
        cmd.Parameters.AddWithValue("fpsyear",    fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task UpdateAgrupRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string testCode, string buyer, int fpsYear, decimal newRate,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        // Spec §2.3: Update UnitPrice only; do not touch NoRequired, DateCreated, Active,
        // ProjectBuyerCode/TestBuyerCode (existing-row routing immutability, DR-API-05).
        cmd.CommandText = @"
            UPDATE fps.tlkptestreqmt
            SET unitprice = @unitprice
            WHERE testcode = @testcode AND buyer = @buyer AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("unitprice", newRate);
        cmd.Parameters.AddWithValue("testcode",  testCode);
        cmd.Parameters.AddWithValue("buyer",     buyer);
        cmd.Parameters.AddWithValue("fpsyear",   fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // ── Reference lookups (bulk, scoped to this upload only — plan §3.2/§7.6) ──────

    private static async Task<IReadOnlySet<string>> GetExistingProjectCodesAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> parentProjectCodes, int fpsYear, CancellationToken ct)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (parentProjectCodes.Count == 0)
            return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT parentproject FROM fps.tlkpproject
            WHERE fpsyear = @fpsyear AND parentproject = ANY(@codes);";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("codes", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = parentProjectCodes.ToArray()
        });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            result.Add(r.GetString(0));
        return result;
    }

    private static async Task<IReadOnlySet<(string TestCode, string WorkGroup)>> GetExistingCapabilityPairsAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<(string TestCode, string WorkGroup)> pairs, int fpsYear, CancellationToken ct)
    {
        var result = new HashSet<(string, string)>();
        if (pairs.Count == 0)
            return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        // Two real columns (testcode, workgroup) — DR-VAL-02, never a concatenated string.
        cmd.CommandText = @"
            SELECT c.testcode, c.workgroup
            FROM fps.tlkptestcapability c
            JOIN unnest(@testcodes::text[], @workgroups::text[]) AS v(tc, wg)
              ON c.testcode = v.tc AND c.workgroup = v.wg
            WHERE c.fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("testcodes", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = pairs.Select(p => p.TestCode).ToArray()
        });
        cmd.Parameters.Add(new NpgsqlParameter("workgroups", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = pairs.Select(p => p.WorkGroup).ToArray()
        });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
            result.Add((r.GetString(0), r.GetString(1)));
        return result;
    }

    /// <summary>
    /// Reads the immutable download snapshot (CR057) for the request's active download
    /// version — never a live requery (reconciliation §2.6).
    /// </summary>
    private static async Task<List<DownloadedSnapshotKey>> GetSnapshotRowsAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        Guid jobQueueId, int downloadVersion, CancellationToken ct)
    {
        var result = new List<DownloadedSnapshotKey>();

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT sheetname, testcode, buyer, source_rate::numeric
            FROM fps.bulk_rates_downloaded_key
            WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion;";
        cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
        cmd.Parameters.AddWithValue("downloadversion", downloadVersion);

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            result.Add(new DownloadedSnapshotKey
            {
                Sheet = r.GetString(0),
                TestCode = r.GetString(1),
                Buyer = r.IsDBNull(2) ? null : r.GetString(2),
                SourceRate = r.IsDBNull(3) ? null : r.GetDecimal(3)
            });
        }
        return result;
    }

    // ── History builders ─────────────────────────────────────────────────────

    private static IEnumerable<RateChangeHistoryRow> BuildFecInsertHistory(
        FecStagingRow row, BulkRatesJobQueueEntry entry, DateTime appliedAt, decimal newRate)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode });
        var common = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                      "FEC", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeHistoryRow(common, "unitpricevla",  null, newRate.ToString(), "Insert");
        yield return MakeHistoryRow(common, "defraunitprice", null, newRate.ToString(), "Insert");
    }

    private static IEnumerable<RateChangeHistoryRow> BuildFecUpdateHistory(
        FecStagingRow row,
        (decimal UnitPriceVla, decimal DefraUnitPrice) before,
        BulkRatesJobQueueEntry entry, DateTime appliedAt, decimal newRate, string changeType)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode });
        var common = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                      "FEC", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeHistoryRow(common, "unitpricevla",   before.UnitPriceVla.ToString(), newRate.ToString(), changeType);
        yield return MakeHistoryRow(common, "defraunitprice",  before.DefraUnitPrice.ToString(), newRate.ToString(), changeType);
    }

    private static IEnumerable<RateChangeHistoryRow> BuildAgrupInsertHistory(
        AgrupStagingRow row, BulkRatesJobQueueEntry entry, DateTime appliedAt, decimal newRate)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode, buyer = row.Buyer });
        var common = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                      "AGRUP", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeHistoryRow(common, "unitprice", null, newRate.ToString(), "Insert");
    }

    private static IEnumerable<RateChangeHistoryRow> BuildAgrupUpdateHistory(
        AgrupStagingRow row, decimal? currentUnitPrice, BulkRatesJobQueueEntry entry, DateTime appliedAt,
        decimal newRate, string changeType)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode, buyer = row.Buyer });
        var common = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                      "AGRUP", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeHistoryRow(common, "unitprice", currentUnitPrice?.ToString(), newRate.ToString(), changeType);
    }

    private static RateChangeHistoryRow MakeHistoryRow(
        (Guid JobQueueId, Guid JobExecutionId, int JobId, int FpsYear,
         string RateCategory, string BusinessKeyJson,
         string? RequestedBy, string? ApprovedBy, DateTime AppliedAt) c,
        string fieldName, string? oldValue, string? newValue, string changeType)
        => new(c.JobQueueId, c.JobExecutionId, c.JobId, c.FpsYear,
               c.RateCategory, c.BusinessKeyJson, fieldName,
               oldValue, newValue, changeType,
               c.RequestedBy, c.ApprovedBy, c.AppliedAt);

    // ── testreq_log write (DR-WK-testreq) ───────────────────────────────────
    // Restores the legacy trigger-equivalent row snapshot in fps.testreq_log.
    // Always insert_delete='I' for Insert, Update, and ZeroRateWithdrawal (the final
    // live row image after the change is applied). 'D' is reserved for physical Delete
    // paths if one is ever introduced. Same transaction as the live-table mutation and
    // rate_change_history insert — rolls back together on failure.

    private static async Task WriteTestreqLogAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string testCode, string buyer, int fpsYear, decimal unitPrice,
        double? noRequired, string? projectBuyerCode, string? testBuyerCode, short? active,
        DateTime executionTimestamp, string? userId, string insertDelete,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            INSERT INTO fps.testreq_log
                (testcode, buyer, unitprice, norequired, projectbuyercode, testbuyercode,
                 active, date_time, user_id, insert_delete, jobcode, fpsyear)
            VALUES
                (@testcode, @buyer, @unitprice, @norequired, @projectbuyercode, @testbuyercode,
                 @active, @date_time, @user_id, @insert_delete, @jobcode, @fpsyear);";
        // Truncate to column sizes: testcode/buyer varchar(20), user_id varchar(20)
        cmd.Parameters.AddWithValue("testcode",         testCode.Length <= 20 ? testCode : testCode[..20]);
        cmd.Parameters.AddWithValue("buyer",            buyer.Length <= 20 ? buyer : buyer[..20]);
        cmd.Parameters.AddWithValue("unitprice",        (double)unitPrice);
        cmd.Parameters.AddWithValue("norequired",       noRequired.HasValue ? (object)(int)noRequired.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("projectbuyercode", (object?)projectBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("testbuyercode",    (object?)testBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("active",           active.HasValue ? (object)active.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("date_time",        executionTimestamp);
        cmd.Parameters.AddWithValue("user_id",
            userId is null ? DBNull.Value
            : userId.Length <= 20 ? (object)userId : userId[..20]);
        cmd.Parameters.AddWithValue("insert_delete",    insertDelete);
        // jobcode mirrors projectbuyercode per schema comment
        cmd.Parameters.AddWithValue("jobcode",          (object?)projectBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("fpsyear",          fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // ── Write history inside an existing open transaction ───────────────────
    // We use the same connection/transaction as the mutations so history is
    // included in the same commit (spec §17.2).

    private static async Task WriteHistoryInsideTransactionAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyList<RateChangeHistoryRow> rows,
        CancellationToken ct)
    {
        foreach (var row in rows)
        {
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
                INSERT INTO fps.rate_change_history
                    (jobqueueid, jobexecutionid, jobid, fpsyear, ratecategory,
                     businesskey, fieldname, oldvalue, newvalue, changetype,
                     requestedby, approvedby, appliedatutc)
                VALUES
                    (@jobqueueid, @jobexecutionid, @jobid, @fpsyear, @ratecategory,
                     @businesskey::jsonb, @fieldname, @oldvalue, @newvalue, @changetype,
                     @requestedby, @approvedby, @appliedatutc);";
            cmd.Parameters.AddWithValue("jobqueueid",    row.JobQueueId);
            cmd.Parameters.AddWithValue("jobexecutionid", row.JobExecutionId);
            cmd.Parameters.AddWithValue("jobid",         row.JobId);
            cmd.Parameters.AddWithValue("fpsyear",       row.FpsYear);
            cmd.Parameters.AddWithValue("ratecategory",  row.RateCategory);
            cmd.Parameters.AddWithValue("businesskey",   row.BusinessKeyJson);
            cmd.Parameters.AddWithValue("fieldname",     row.FieldName);
            cmd.Parameters.AddWithValue("oldvalue",      (object?)row.OldValue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("newvalue",      (object?)row.NewValue ?? DBNull.Value);
            cmd.Parameters.AddWithValue("changetype",    row.ChangeType);
            cmd.Parameters.AddWithValue("requestedby",   (object?)row.RequestedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("approvedby",    (object?)row.ApprovedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("appliedatutc",  row.AppliedAtUtc);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
