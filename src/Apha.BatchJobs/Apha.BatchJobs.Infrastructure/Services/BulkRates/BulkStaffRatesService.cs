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
/// Infrastructure implementation of <see cref="IBulkStaffRatesService"/>.
/// Applies Staff profit-centre grade annual rate changes (PayRate, NPR, OHR)
/// inside a single database transaction, writes permanent history, and
/// clears request-scoped staging rows on success.
/// </summary>
public sealed class BulkStaffRatesService : IBulkStaffRatesService
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IBulkRatesRepository _repository;
    private readonly ILogger<BulkStaffRatesService> _logger;

    public BulkStaffRatesService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IBulkRatesRepository repository,
        ILogger<BulkStaffRatesService> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(BulkRatesExecutionContext context, CancellationToken cancellationToken = default)
    {
        // ── 1. Load and validate ──────────────────────────────────────────
        var entry = await _repository.GetApprovedRequestAsync(context.JobExecutionId, cancellationToken)
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

        // ── 3. Execute mutations + write history in one transaction ───────
        int updated = 0;

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using (var tx = await conn.BeginTransactionAsync(cancellationToken))
        {
            var historyRows = new List<RateChangeHistoryRow>();

            foreach (var row in stagingRows)
            {
                var before = await GetCurrentStaffRatesAsync(conn, tx, row.PcGrade, fpsYear, cancellationToken);

                if (before is null)
                {
                    _logger.LogWarning(
                        "BulkStaffRatesUpdate: pcgrade '{PcGrade}' not found for fpsyear {FpsYear} — skipping",
                        row.PcGrade, fpsYear);
                    continue;
                }

                await UpdateStaffRowAsync(conn, tx, row, fpsYear, cancellationToken);
                historyRows.AddRange(BuildHistory(row, before.Value, entry, appliedAt));
                updated++;
            }

            await WriteHistoryInsideTransactionAsync(conn, tx, historyRows, cancellationToken);
            await tx.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "BulkStaffRatesUpdate committed | JobQueueId={JobQueueId} | Updated={Updated}",
                jobQueueId, updated);

            // ── US-XC-02: Log commit summary ──────────────────────────────
            await _repository.WriteJobQueueLogAsync(
                jobQueueId,
                $"Rate changes committed: Staff updated={updated}.",
                entry.ApprovedBy, cancellationToken);
        }

        // ── 4. Delete staging post-commit ─────────────────────────────────
        await _repository.DeleteStaffStagingRowsAsync(jobQueueId, cancellationToken);

        _logger.LogInformation(
            "BulkStaffRatesUpdate staging cleared | JobQueueId={JobQueueId}",
            jobQueueId);
    }

    private static void ValidatePreconditions(BulkRatesJobQueueEntry entry, BulkRatesExecutionContext context)
    {
        if (!string.Equals(entry.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"BulkStaffRatesUpdate: request {entry.JobQueueId:D} is in status '{entry.Status}', expected 'Approved'.");

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

    private static async Task<(decimal PayRate, decimal Npr, decimal Ohr)?> GetCurrentStaffRatesAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string pcGrade, int fpsYear,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT payrate::numeric, npr::numeric, ohr::numeric
            FROM fps.profitcentregrade
            WHERE pcgrade = @pcgrade AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("pcgrade",  pcGrade);
        cmd.Parameters.AddWithValue("fpsyear",  fpsYear);

        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct))
            return null;
        return (r.IsDBNull(0) ? 0m : r.GetDecimal(0),
                r.IsDBNull(1) ? 0m : r.GetDecimal(1),
                r.IsDBNull(2) ? 0m : r.GetDecimal(2));
    }

    private static async Task UpdateStaffRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        StaffStagingRow row, int fpsYear,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            UPDATE fps.profitcentregrade
            SET payrate = @payrate, npr = @npr, ohr = @ohr
            WHERE pcgrade = @pcgrade AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("payrate", (object?)row.PayRate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("npr",     (object?)row.Npr     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ohr",     (object?)row.Ohr     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("pcgrade", row.PcGrade);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static IEnumerable<RateChangeHistoryRow> BuildHistory(
        StaffStagingRow row,
        (decimal PayRate, decimal Npr, decimal Ohr) before,
        BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { pcGrade = row.PcGrade });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "Staff", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        yield return MakeRow(c, "payrate", before.PayRate.ToString(), row.PayRate?.ToString(), "Update");
        yield return MakeRow(c, "npr",     before.Npr.ToString(),     row.Npr?.ToString(),     "Update");
        yield return MakeRow(c, "ohr",     before.Ohr.ToString(),     row.Ohr?.ToString(),     "Update");
    }

    private static RateChangeHistoryRow MakeRow(
        (Guid JobQueueId, Guid JobExecutionId, int JobId, int FpsYear,
         string RateCategory, string BusinessKeyJson,
         string? RequestedBy, string? ApprovedBy, DateTime AppliedAt) c,
        string field, string? oldVal, string? newVal, string changeType)
        => new(c.JobQueueId, c.JobExecutionId, c.JobId, c.FpsYear,
               c.RateCategory, c.BusinessKeyJson, field,
               oldVal, newVal, changeType, c.RequestedBy, c.ApprovedBy, c.AppliedAt);

    private static async Task WriteHistoryInsideTransactionAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        List<RateChangeHistoryRow> rows, CancellationToken ct)
    {
        foreach (var row in rows)
            await BulkRatesRepository.InsertHistoryRowAsync(conn, tx, row, ct);
    }
}
