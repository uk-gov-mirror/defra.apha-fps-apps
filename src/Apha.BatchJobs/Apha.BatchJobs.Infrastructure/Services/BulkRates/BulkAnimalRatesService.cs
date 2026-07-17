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
/// Infrastructure implementation of <see cref="IBulkAnimalRatesService"/>.
/// Applies Animal annual rate changes (DailyRate, DefraDailyRate, PlanByWeek, Species, SecurityLevel)
/// inside a single database transaction, writes permanent history, and
/// clears request-scoped staging rows on success.
/// </summary>
public sealed class BulkAnimalRatesService : IBulkAnimalRatesService
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly IBulkRatesRepository _repository;
    private readonly ILogger<BulkAnimalRatesService> _logger;

    public BulkAnimalRatesService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        IBulkRatesRepository repository,
        ILogger<BulkAnimalRatesService> logger)
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

        // ── 3. Execute mutations + write history in one transaction ───────
        int updated = 0, unchanged = 0;

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using (var tx = await conn.BeginTransactionAsync(cancellationToken))
        {
            var historyRows = new List<RateChangeHistoryRow>();

            foreach (var row in stagingRows)
            {
                var before = await GetCurrentAnimalRatesAsync(conn, tx, row.AnimalType, fpsYear, cancellationToken);

                if (before is null)
                {
                    _logger.LogWarning(
                        "BulkAnimalRatesUpdate: animaltype '{AnimalType}' not found for fpsyear {FpsYear} — skipping",
                        row.AnimalType, fpsYear);
                    continue;
                }

                var dailyRateChanged      = row.DailyRate.HasValue      && row.DailyRate.Value      != before.Value.DailyRate;
                var defraDailyRateChanged = row.DefraDailyRate.HasValue && row.DefraDailyRate.Value != before.Value.DefraDailyRate;
                var planByWeekChanged     = row.PlanByWeek.HasValue     && row.PlanByWeek.Value     != before.Value.PlanByWeek;
                var speciesChanged        = row.Species is not null       && row.Species       != before.Value.Species;
                var securityLevelChanged  = row.SecurityLevel is not null && row.SecurityLevel != before.Value.SecurityLevel;

                if (!dailyRateChanged && !defraDailyRateChanged && !planByWeekChanged && !speciesChanged && !securityLevelChanged)
                {
                    unchanged++;
                    continue;
                }

                await UpdateAnimalRowAsync(conn, tx, row, fpsYear, cancellationToken);
                historyRows.AddRange(BuildHistory(row, before.Value, entry, appliedAt));
                updated++;
            }

            await WriteHistoryInsideTransactionAsync(conn, tx, historyRows, cancellationToken);
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
        await _repository.DeleteAnimalStagingRowsAsync(jobQueueId, cancellationToken);

        _logger.LogInformation(
            "BulkAnimalRatesUpdate staging cleared | JobQueueId={JobQueueId}",
            jobQueueId);
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

    private static async Task<(decimal DailyRate, decimal DefraDailyRate, bool PlanByWeek, string? Species, string? SecurityLevel)?> GetCurrentAnimalRatesAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string animalType, int fpsYear,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT dailyrate::numeric, defradailyrate::numeric, planbyweek, species, security_level
            FROM fps.tblanimals
            WHERE animaltype = @animaltype AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("animaltype", animalType);
        cmd.Parameters.AddWithValue("fpsyear",    fpsYear);

        await using var r = await cmd.ExecuteReaderAsync(ct);
        if (!await r.ReadAsync(ct))
            return null;
        return (r.IsDBNull(0) ? 0m : r.GetDecimal(0),
                r.IsDBNull(1) ? 0m : r.GetDecimal(1),
                r.IsDBNull(2) ? false : r.GetBoolean(2),
                r.IsDBNull(3) ? null : r.GetString(3),
                r.IsDBNull(4) ? null : r.GetString(4));
    }

    private static async Task UpdateAnimalRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        AnimalStagingRow row, int fpsYear,
        CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        // dailyrate/defradailyrate are `money` columns; COALESCE requires an explicit
        // cast to match — a bare numeric-typed parameter has no implicit cast to/from money.
        cmd.CommandText = @"
            UPDATE fps.tblanimals
            SET dailyrate      = COALESCE(@dailyrate::money, dailyrate),
                defradailyrate = COALESCE(@defradailyrate::money, defradailyrate),
                planbyweek     = COALESCE(@planbyweek, planbyweek),
                species        = COALESCE(@species, species),
                security_level = COALESCE(@security_level, security_level)
            WHERE animaltype = @animaltype AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("dailyrate",      (object?)row.DailyRate      ?? DBNull.Value);
        cmd.Parameters.AddWithValue("defradailyrate", (object?)row.DefraDailyRate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("planbyweek",     (object?)row.PlanByWeek     ?? DBNull.Value);
        cmd.Parameters.AddWithValue("species",        (object?)row.Species        ?? DBNull.Value);
        cmd.Parameters.AddWithValue("security_level", (object?)row.SecurityLevel  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("animaltype",     row.AnimalType);
        cmd.Parameters.AddWithValue("fpsyear",        fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static IEnumerable<RateChangeHistoryRow> BuildHistory(
        AnimalStagingRow row,
        (decimal DailyRate, decimal DefraDailyRate, bool PlanByWeek, string? Species, string? SecurityLevel) before,
        BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { animalType = row.AnimalType });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "Animal", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        if (row.DailyRate.HasValue && row.DailyRate.Value != before.DailyRate)
            yield return MakeRow(c, "dailyrate", before.DailyRate.ToString(), row.DailyRate.Value.ToString(), "Update");

        if (row.DefraDailyRate.HasValue && row.DefraDailyRate.Value != before.DefraDailyRate)
            yield return MakeRow(c, "defradailyrate", before.DefraDailyRate.ToString(), row.DefraDailyRate.Value.ToString(), "Update");

        if (row.PlanByWeek.HasValue && row.PlanByWeek.Value != before.PlanByWeek)
            yield return MakeRow(c, "planbyweek", before.PlanByWeek.ToString(), row.PlanByWeek.Value.ToString(), "Update");

        if (row.Species is not null && row.Species != before.Species)
            yield return MakeRow(c, "species", before.Species, row.Species, "Update");

        if (row.SecurityLevel is not null && row.SecurityLevel != before.SecurityLevel)
            yield return MakeRow(c, "security_level", before.SecurityLevel, row.SecurityLevel, "Update");
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
