using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.BulkRates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;

namespace Apha.BatchJobs.Infrastructure.Services.BulkRates;

/// <summary>
/// Infrastructure implementation of <see cref="IBulkTestRatesService"/>.
/// Applies FEC Test/Product (FEC before AGRUP, spec §15.2) annual rate changes
/// inside a single database transaction, then writes permanent history and
/// clears request-scoped staging rows on success.
/// </summary>
public sealed class BulkTestRatesService : IBulkTestRatesService
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IBulkRatesRepository _repository;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly ILogger<BulkTestRatesService> _logger;

    public BulkTestRatesService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IBulkRatesRepository repository,
        IJobExecutionRepository executionRepository,
        ILogger<BulkTestRatesService> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
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

        // ── 2. Load staging rows ──────────────────────────────────────────
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
            // FEC Test/Product — inserts first, then updates (spec §15.2, §2.4)
            int fecInserted = 0, fecUpdated = 0, fecUnchanged = 0;
            foreach (var row in fecRows)
            {
                var existing = await GetFecCurrentRowAsync(conn, tx, row.TestCode, fpsYear, cancellationToken);

                if (existing is null)
                {
                    // Insert new FEC row
                    await InsertFecRowAsync(conn, tx, row, fpsYear, cancellationToken);
                    historyRows.AddRange(BuildFecInsertHistory(row, entry, appliedAt));
                    fecInserted++;
                }
                else if (row.FecNewRate.HasValue &&
                         (row.FecNewRate != existing.Value.UnitPriceVla || row.FecNewRate != existing.Value.DefraUnitPrice))
                {
                    await UpdateFecRowAsync(conn, tx, row.TestCode, fpsYear, row.FecNewRate.Value, cancellationToken);
                    historyRows.AddRange(BuildFecUpdateHistory(row, existing.Value, entry, appliedAt));
                    fecUpdated++;
                }
                else
                {
                    fecUnchanged++;
                }
            }

            // AGRUP — after FEC (spec §2.4 sequencing rule)
            int agrupInserted = 0, agrupUpdated = 0, agrupUnchanged = 0;
            foreach (var row in agrupRows)
            {
                if (!row.AgrupNew.HasValue)
                {
                    agrupUnchanged++;
                    continue;
                }

                var (existsAgrup, currentUnitPrice) = await GetAgrupCurrentRowAsync(conn, tx, row.TestCode, row.Buyer, fpsYear, cancellationToken);

                if (!existsAgrup)
                {
                    await InsertAgrupRowAsync(conn, tx, row, fpsYear, appliedAt, cancellationToken);
                    historyRows.AddRange(BuildAgrupInsertHistory(row, entry, appliedAt));
                    agrupInserted++;
                }
                else if (row.AgrupNew != currentUnitPrice)
                {
                    await UpdateAgrupRowAsync(conn, tx, row.TestCode, row.Buyer, fpsYear, row.AgrupNew.Value, cancellationToken);
                    historyRows.AddRange(BuildAgrupUpdateHistory(row, currentUnitPrice, entry, appliedAt));
                    agrupUpdated++;
                }
                else
                {
                    agrupUnchanged++;
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
    }

    // ── FEC helpers ─────────────────────────────────────────────────────────

    private static async Task<(decimal UnitPriceVla, decimal DefraUnitPrice)?> GetFecCurrentRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string testCode, int fpsYear,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT unitpricevla::numeric, defraunitprice::numeric
            FROM fps.testorproduct
            WHERE itemcode = @itemcode AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("itemcode", testCode);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);

        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct))
            return null;
        return (r.IsDBNull(0) ? 0m : r.GetDecimal(0), r.IsDBNull(1) ? 0m : r.GetDecimal(1));
    }

    private static async Task InsertFecRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        FecStagingRow row, int fpsYear,
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
        cmd.Parameters.AddWithValue("unitpricevla",     (object?)row.FecNewRate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("owner",            (object?)row.Owner ?? DBNull.Value);
        cmd.Parameters.AddWithValue("shortdescription", (object?)row.ShortDescription ?? DBNull.Value);
        cmd.Parameters.AddWithValue("defraunitprice",   (object?)row.FecNewRate ?? DBNull.Value);
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

    private static async Task<(bool Exists, decimal? UnitPrice)> GetAgrupCurrentRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string testCode, string buyer, int fpsYear,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT unitprice::numeric
            FROM fps.tlkptestreqmt
            WHERE testcode = @testcode AND buyer = @buyer AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("testcode", testCode);
        cmd.Parameters.AddWithValue("buyer",    buyer);
        cmd.Parameters.AddWithValue("fpsyear",  fpsYear);

        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct))
            return (false, null);
        return (true, r.IsDBNull(0) ? null : r.GetDecimal(0));
    }

    private static async Task InsertAgrupRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        AgrupStagingRow row, int fpsYear, DateTime executionTimestamp,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        // Spec §2.3: ProjectBuyerCode = Buyer, DateCreated = execution timestamp, Active = 1
        cmd.CommandText = @"
            INSERT INTO fps.tlkptestreqmt
                (testcode, buyer, unitprice, norequired, projectbuyercode, datecreated, active, fpsyear)
            VALUES
                (@testcode, @buyer, @unitprice, @norequired, @buyer, @datecreated, 1, @fpsyear);";
        cmd.Parameters.AddWithValue("testcode",   row.TestCode);
        cmd.Parameters.AddWithValue("buyer",      row.Buyer);
        cmd.Parameters.AddWithValue("unitprice",  (object?)row.AgrupNew ?? DBNull.Value);
        cmd.Parameters.AddWithValue("norequired", (object?)row.NoRequired ?? DBNull.Value);
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
        // Spec §2.3: Update UnitPrice only; do not touch NoRequired, DateCreated, Active, ProjectBuyerCode
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

    // ── History builders ─────────────────────────────────────────────────────

    private static IEnumerable<RateChangeHistoryRow> BuildFecInsertHistory(
        FecStagingRow row, BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode });
        var common = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                      "FEC", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeHistoryRow(common, "unitpricevla",  null, row.FecNewRate?.ToString(), "Insert");
        yield return MakeHistoryRow(common, "defraunitprice", null, row.FecNewRate?.ToString(), "Insert");
    }

    private static IEnumerable<RateChangeHistoryRow> BuildFecUpdateHistory(
        FecStagingRow row,
        (decimal UnitPriceVla, decimal DefraUnitPrice) before,
        BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode });
        var common = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                      "FEC", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeHistoryRow(common, "unitpricevla",   before.UnitPriceVla.ToString(), row.FecNewRate?.ToString(), "Update");
        yield return MakeHistoryRow(common, "defraunitprice",  before.DefraUnitPrice.ToString(), row.FecNewRate?.ToString(), "Update");
    }

    private static IEnumerable<RateChangeHistoryRow> BuildAgrupInsertHistory(
        AgrupStagingRow row, BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode, buyer = row.Buyer });
        var common = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                      "AGRUP", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeHistoryRow(common, "unitprice", null, row.AgrupNew?.ToString(), "Insert");
    }

    private static IEnumerable<RateChangeHistoryRow> BuildAgrupUpdateHistory(
        AgrupStagingRow row, decimal? currentUnitPrice, BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode, buyer = row.Buyer });
        var common = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                      "AGRUP", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeHistoryRow(common, "unitprice", currentUnitPrice?.ToString(), row.AgrupNew?.ToString(), "Update");
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
