using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace Apha.BatchJobs.Infrastructure.BulkRates.Repositories;

/// <summary>
/// Npgsql implementation of <see cref="IBulkRatesRepository"/>.
/// Uses <see cref="IDbContextFactory{BatchJobsDbContext}"/> to obtain connections,
/// consistent with the existing YearEnd infrastructure pattern.
/// </summary>
public sealed class BulkRatesRepository : IBulkRatesRepository
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly ILogger<BulkRatesRepository> _logger;

    public BulkRatesRepository(IDbContextFactory<BatchJobsDbContext> dbContextFactory, ILogger<BulkRatesRepository> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<BulkRatesJobQueueEntry?> GetRunningRequestAsync(
        Guid jobExecutionId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT
                q.jobqueueid,
                q.jobexecutionid,
                q.jobid,
                m.jobname,
                s.status,
                q.fpsyear,
                q.requestedby,
                q.approved_by,
                q.approved_at_utc,
                q.upload_version,
                q.active_download_version
            FROM fps.job_queue q
            JOIN fps.job_master m ON m.jobid = q.jobid
            JOIN fps.job_status s ON s.statusid = q.statusid AND s.jobid = q.jobid
            WHERE q.jobexecutionid = @jobexecutionid;";
        cmd.Parameters.AddWithValue("jobexecutionid", jobExecutionId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new BulkRatesJobQueueEntry(
            JobQueueId:       reader.GetGuid(0),
            JobExecutionId:   reader.GetGuid(1),
            JobId:            reader.GetInt32(2),
            JobName:          reader.GetString(3),
            Status:           reader.GetString(4),
            FpsYear:          reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
            RequestedBy:      reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            ApprovedBy:       reader.IsDBNull(7) ? null : reader.GetString(7),
            ApprovedAtUtc:    reader.IsDBNull(8) ? null : reader.GetDateTime(8),
            UploadVersion:    reader.IsDBNull(9) ? null : reader.GetInt32(9),
            ActiveDownloadVersion: reader.IsDBNull(10) ? null : reader.GetInt32(10));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FecStagingRow>> GetFecStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT jobqueueid, testcode, unitpricevla::numeric, defraunitprice::numeric,
                   fecnewrate::numeric, change::numeric,
                   itemdescription, shortdescription, owner, comments,
                   calculated_action, effective_new_rate::numeric, source_current_rate::numeric,
                   validation_version
            FROM fps.tblstagingtestorproduct
            WHERE jobqueueid = @jobqueueid
            ORDER BY testcode;";
        cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

        var rows = new List<FecStagingRow>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new FecStagingRow(
                JobQueueId:       reader.GetGuid(0),
                TestCode:         reader.GetString(1),
                UnitPriceVla:     reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                DefraUnitPrice:   reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                FecNewRate:       reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                Change:           reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                ItemDescription:  reader.IsDBNull(6) ? null : reader.GetString(6),
                ShortDescription: reader.IsDBNull(7) ? null : reader.GetString(7),
                Owner:            reader.IsDBNull(8) ? null : reader.GetString(8),
                Comments:         reader.IsDBNull(9) ? null : reader.GetString(9),
                CalculatedAction:  reader.IsDBNull(10) ? null : reader.GetString(10),
                EffectiveNewRate:  reader.IsDBNull(11) ? null : reader.GetDecimal(11),
                SourceCurrentRate: reader.IsDBNull(12) ? null : reader.GetDecimal(12),
                ValidationVersion: reader.IsDBNull(13) ? null : reader.GetInt32(13)));
        }

        return rows;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AgrupStagingRow>> GetAgrupStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT jobqueueid, testcode, buyer,
                   agrup::numeric, agrupnew::numeric, change::numeric,
                   norequired, datecreated, active, comments,
                   projectbuyercode, testbuyercode, testbuyerworkgroup,
                   calculated_action, effective_new_rate::numeric, source_current_rate::numeric,
                   validation_version
            FROM fps.tblstagingtlkptestreqmt
            WHERE jobqueueid = @jobqueueid
            ORDER BY testcode, buyer;";
        cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

        var rows = new List<AgrupStagingRow>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new AgrupStagingRow(
                JobQueueId:  reader.GetGuid(0),
                TestCode:    reader.GetString(1),
                Buyer:       reader.GetString(2),
                Agrup:       reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                AgrupNew:    reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                Change:      reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                NoRequired:  reader.IsDBNull(6) ? null : reader.GetDouble(6),
                DateCreated: reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                Active:      reader.IsDBNull(8) ? null : reader.GetInt16(8),
                Comments:    reader.IsDBNull(9) ? null : reader.GetString(9),
                ProjectBuyerCode:   reader.IsDBNull(10) ? null : reader.GetString(10),
                TestBuyerCode:      reader.IsDBNull(11) ? null : reader.GetString(11),
                TestBuyerWorkGroup: reader.IsDBNull(12) ? null : reader.GetString(12),
                CalculatedAction:   reader.IsDBNull(13) ? null : reader.GetString(13),
                EffectiveNewRate:   reader.IsDBNull(14) ? null : reader.GetDecimal(14),
                SourceCurrentRate:  reader.IsDBNull(15) ? null : reader.GetDecimal(15),
                ValidationVersion:  reader.IsDBNull(16) ? null : reader.GetInt32(16)));
        }

        return rows;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<StaffStagingRow>> GetStaffStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT jobqueueid, pcgrade,
                   payrate::numeric, npr::numeric, ohr::numeric,
                   calculated_action,
                   source_payrate::numeric, source_npr::numeric, source_ohr::numeric,
                   effective_payrate::numeric, effective_npr::numeric, effective_ohr::numeric,
                   validation_version
            FROM fps.tblstagingprofitcentregrade
            WHERE jobqueueid = @jobqueueid
            ORDER BY pcgrade;";
        cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

        var rows = new List<StaffStagingRow>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new StaffStagingRow(
                JobQueueId: reader.GetGuid(0),
                PcGrade:    reader.GetString(1),
                PayRate:    reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                Npr:        reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                Ohr:        reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                CalculatedAction: reader.IsDBNull(5) ? null : reader.GetString(5),
                SourcePayRate:    reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                SourceNpr:        reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                SourceOhr:        reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                EffectivePayRate: reader.IsDBNull(9) ? null : reader.GetDecimal(9),
                EffectiveNpr:     reader.IsDBNull(10) ? null : reader.GetDecimal(10),
                EffectiveOhr:     reader.IsDBNull(11) ? null : reader.GetDecimal(11),
                ValidationVersion: reader.IsDBNull(12) ? null : reader.GetInt32(12)));
        }

        return rows;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AnimalStagingRow>> GetAnimalStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT jobqueueid, animaltype, species, security_level,
                   dailyrate::numeric, defradailyrate::numeric, planbyweek,
                   calculated_action,
                   source_dailyrate::numeric, source_defradailyrate::numeric, source_planbyweek,
                   source_species, source_securitylevel,
                   effective_dailyrate::numeric, effective_defradailyrate::numeric, effective_planbyweek,
                   effective_species, effective_securitylevel,
                   validation_version
            FROM fps.tblstaginganimals
            WHERE jobqueueid = @jobqueueid
            ORDER BY animaltype;";
        cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

        var rows = new List<AnimalStagingRow>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new AnimalStagingRow(
                JobQueueId:    reader.GetGuid(0),
                AnimalType:    reader.GetString(1),
                Species:       reader.IsDBNull(2) ? null : reader.GetString(2),
                SecurityLevel: reader.IsDBNull(3) ? null : reader.GetString(3),
                DailyRate:     reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                DefraDailyRate: reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                PlanByWeek:    reader.IsDBNull(6) ? null : reader.GetBoolean(6),
                CalculatedAction: reader.IsDBNull(7) ? null : reader.GetString(7),
                SourceDailyRate:      reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                SourceDefraDailyRate: reader.IsDBNull(9) ? null : reader.GetDecimal(9),
                SourcePlanByWeek:     reader.IsDBNull(10) ? null : reader.GetBoolean(10),
                SourceSpecies:        reader.IsDBNull(11) ? null : reader.GetString(11),
                SourceSecurityLevel:  reader.IsDBNull(12) ? null : reader.GetString(12),
                EffectiveDailyRate:      reader.IsDBNull(13) ? null : reader.GetDecimal(13),
                EffectiveDefraDailyRate: reader.IsDBNull(14) ? null : reader.GetDecimal(14),
                EffectivePlanByWeek:     reader.IsDBNull(15) ? null : reader.GetBoolean(15),
                EffectiveSpecies:        reader.IsDBNull(16) ? null : reader.GetString(16),
                EffectiveSecurityLevel:  reader.IsDBNull(17) ? null : reader.GetString(17),
                ValidationVersion: reader.IsDBNull(18) ? null : reader.GetInt32(18)));
        }

        return rows;
    }

    /// <inheritdoc />
    public async Task WriteHistoryBatchAsync(
        IReadOnlyList<RateChangeHistoryRow> rows,
        CancellationToken cancellationToken = default)
    {
        if (rows.Count == 0)
            return;

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        foreach (var row in rows)
            await InsertHistoryRowAsync(conn, tx, row, cancellationToken);

        await tx.CommitAsync(cancellationToken);

        _logger.LogInformation("Rate change history written | RowCount={RowCount}", rows.Count);
    }

    /// <inheritdoc />
    public async Task DeleteFecStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        // AGRUP rows first (child FK to FEC staging).
        await using var delAgrup = conn.CreateCommand();
        delAgrup.Transaction = tx;
        delAgrup.CommandText = "DELETE FROM fps.tblstagingtlkptestreqmt WHERE jobqueueid = @jqid;";
        delAgrup.Parameters.AddWithValue("jqid", jobQueueId);
        var agrupDeleted = await delAgrup.ExecuteNonQueryAsync(cancellationToken);

        await using var delFec = conn.CreateCommand();
        delFec.Transaction = tx;
        delFec.CommandText = "DELETE FROM fps.tblstagingtestorproduct WHERE jobqueueid = @jqid;";
        delFec.Parameters.AddWithValue("jqid", jobQueueId);
        var fecDeleted = await delFec.ExecuteNonQueryAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "FEC staging deleted | JobQueueId={JobQueueId} | FecDeleted={FecDeleted} | AgrupDeleted={AgrupDeleted}",
            jobQueueId, fecDeleted, agrupDeleted);
    }

    /// <inheritdoc />
    public async Task DeleteStaffStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM fps.tblstagingprofitcentregrade WHERE jobqueueid = @jqid;";
        cmd.Parameters.AddWithValue("jqid", jobQueueId);
        var deleted = await cmd.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation(
            "Staff staging deleted | JobQueueId={JobQueueId} | RowsDeleted={RowsDeleted}",
            jobQueueId, deleted);
    }

    /// <inheritdoc />
    public async Task DeleteAnimalStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM fps.tblstaginganimals WHERE jobqueueid = @jqid;";
        cmd.Parameters.AddWithValue("jqid", jobQueueId);
        var deleted = await cmd.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation(
            "Animal staging deleted | JobQueueId={JobQueueId} | RowsDeleted={RowsDeleted}",
            jobQueueId, deleted);
    }

    // ── Apply Animal rates in one transaction ────────────────────────────────

    /// <inheritdoc />
    public async Task<(int Inserted, int Updated, int Unchanged)> ApplyAnimalRatesAsync(
        IReadOnlyList<AnimalStagingRow> stagingRows,
        BulkRatesJobQueueEntry entry,
        DateTime appliedAt,
        CancellationToken cancellationToken = default)
    {
        int inserted = 0, updated = 0, unchanged = 0;

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();

        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        var animalTypes = stagingRows
            .Select(r => r.AnimalType)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var liveLookup = await GetAnimalRowsForUpdateAsync(conn, tx, animalTypes, entry.FpsYear, cancellationToken);

        foreach (var row in stagingRows)
        {
            switch (row.CalculatedAction)
            {
                case "NoChange":
                    unchanged++;
                    break;

                case "Update":
                    liveLookup.TryGetValue(row.AnimalType.ToUpperInvariant(), out var liveBefore);
                    var rowsAffected = await UpdateAnimalRowAsync(conn, tx, row, entry.FpsYear, cancellationToken);
                    if (rowsAffected == 0)
                        throw new InvalidOperationException(
                            $"BulkAnimalRatesUpdate: UPDATE matched 0 rows for AnimalType='{row.AnimalType}' in JobQueueId={entry.JobQueueId:D}.");
                    foreach (var historyRow in BuildAnimalUpdateHistory(row, liveBefore, entry, appliedAt))
                        await InsertHistoryRowAsync(conn, tx, historyRow, cancellationToken);
                    updated++;
                    break;

                case "Insert":
                    if (liveLookup.ContainsKey(row.AnimalType.ToUpperInvariant()))
                        throw new InvalidOperationException(
                            $"BulkAnimalRatesUpdate: Animal Insert concurrency conflict for AnimalType='{row.AnimalType}', FPS year {entry.FpsYear}: " +
                            "the approved Insert target already exists.");
                    await InsertAnimalRowAsync(conn, tx, row, entry.FpsYear, cancellationToken);
                    foreach (var historyRow in BuildAnimalInsertHistory(row, entry, appliedAt))
                        await InsertHistoryRowAsync(conn, tx, historyRow, cancellationToken);
                    inserted++;
                    break;

                default:
                    throw new InvalidOperationException(
                        $"BulkAnimalRatesUpdate: unexpected CalculatedAction '{row.CalculatedAction}' " +
                        $"for AnimalType='{row.AnimalType}' in JobQueueId={entry.JobQueueId:D}. " +
                        "Only NoChange, Update, and Insert are supported.");
            }
        }

        await tx.CommitAsync(cancellationToken);
        return (inserted, updated, unchanged);
    }

    private static async Task<Dictionary<string, (decimal? DailyRate, decimal? DefraDailyRate, bool PlanByWeek, string? Species, string? SecurityLevel)>> GetAnimalRowsForUpdateAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> animalTypes, int fpsYear, CancellationToken ct)
    {
        var result = new Dictionary<string, (decimal? DailyRate, decimal? DefraDailyRate, bool PlanByWeek, string? Species, string? SecurityLevel)>(StringComparer.OrdinalIgnoreCase);
        if (animalTypes.Count == 0) return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT animaltype, species, security_level, dailyrate::numeric, defradailyrate::numeric, planbyweek
            FROM fps.tblanimals
            WHERE fpsyear = @fpsyear AND animaltype = ANY(@types)
            ORDER BY animaltype
            FOR UPDATE;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("types", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = animalTypes.ToArray() });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var animalType = r.GetString(0);
            result[animalType.ToUpperInvariant()] = (
                r.IsDBNull(3) ? null : r.GetDecimal(3),
                r.IsDBNull(4) ? null : r.GetDecimal(4),
                !r.IsDBNull(5) && r.GetBoolean(5),
                r.IsDBNull(1) ? null : r.GetString(1),
                r.IsDBNull(2) ? null : r.GetString(2));
        }
        return result;
    }

    internal static async Task InsertAnimalRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        AnimalStagingRow row, int fpsYear, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            INSERT INTO fps.tblanimals
                (animaltype, species, security_level, dailyrate, defradailyrate, planbyweek, fpsyear)
            VALUES
                (@animaltype, @species, @security_level, @dailyrate, @defradailyrate, @planbyweek, @fpsyear);";
        cmd.Parameters.AddWithValue("animaltype",     row.AnimalType);
        cmd.Parameters.AddWithValue("species",        (object?)row.EffectiveSpecies        ?? DBNull.Value);
        cmd.Parameters.AddWithValue("security_level", (object?)row.EffectiveSecurityLevel  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("dailyrate",      (object?)row.EffectiveDailyRate      ?? DBNull.Value);
        cmd.Parameters.AddWithValue("defradailyrate", (object?)row.EffectiveDefraDailyRate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("planbyweek",     row.EffectivePlanByWeek ?? false);
        cmd.Parameters.AddWithValue("fpsyear",        fpsYear);
        try
        {
            await cmd.ExecuteNonQueryAsync(ct);
        }
        catch (NpgsqlException ex) when (ex.SqlState == "23505")
        {
            throw new InvalidOperationException(
                $"BulkAnimalRatesUpdate: Animal Insert concurrency conflict for AnimalType='{row.AnimalType}', FPS year {fpsYear}: " +
                "the approved Insert target already exists.", ex);
        }
    }

    private static async Task<int> UpdateAnimalRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        AnimalStagingRow row, int fpsYear, CancellationToken ct)
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
        cmd.Parameters.AddWithValue("dailyrate",      row.EffectiveDailyRate ?? 0m);
        cmd.Parameters.AddWithValue("defradailyrate", row.EffectiveDefraDailyRate ?? 0m);
        cmd.Parameters.AddWithValue("planbyweek",     row.EffectivePlanByWeek ?? false);
        var species  = string.IsNullOrWhiteSpace(row.EffectiveSpecies)       ? null : row.EffectiveSpecies.Trim();
        var secLevel = string.IsNullOrWhiteSpace(row.EffectiveSecurityLevel) ? null : row.EffectiveSecurityLevel.Trim();
        cmd.Parameters.AddWithValue("species",        (object?)species  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("security_level", (object?)secLevel ?? DBNull.Value);
        cmd.Parameters.AddWithValue("animaltype",     row.AnimalType);
        cmd.Parameters.AddWithValue("fpsyear",        fpsYear);
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    private static RateChangeHistoryRow[] BuildAnimalInsertHistory(
        AnimalStagingRow row, BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { animalType = row.AnimalType, fpsYear = entry.FpsYear });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "Animal", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);
        var species       = string.IsNullOrWhiteSpace(row.EffectiveSpecies)       ? null : row.EffectiveSpecies.Trim();
        var secLevel      = string.IsNullOrWhiteSpace(row.EffectiveSecurityLevel) ? null : row.EffectiveSecurityLevel.Trim();
        var dailyRate     = row.EffectiveDailyRate?.ToString();
        var defraDailyRate = row.EffectiveDefraDailyRate?.ToString();
        var planByWeek    = (row.EffectivePlanByWeek ?? false).ToString();
        return
        [
            MakeHistoryRow(c, "species",        null, species,        "Insert"),
            MakeHistoryRow(c, "security_level", null, secLevel,       "Insert"),
            MakeHistoryRow(c, "dailyrate",      null, dailyRate,      "Insert"),
            MakeHistoryRow(c, "defradailyrate", null, defraDailyRate, "Insert"),
            MakeHistoryRow(c, "planbyweek",     null, planByWeek,     "Insert"),
        ];
    }

    private static RateChangeHistoryRow[] BuildAnimalUpdateHistory(
        AnimalStagingRow row,
        (decimal? DailyRate, decimal? DefraDailyRate, bool PlanByWeek, string? Species, string? SecurityLevel)? before,
        BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { animalType = row.AnimalType });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "Animal", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        var beforeDailyRate      = before?.DailyRate ?? 0m;
        var beforeDefraDailyRate = before?.DefraDailyRate ?? 0m;
        var beforePlanByWeek     = before?.PlanByWeek ?? false;
        var beforeSpecies        = string.IsNullOrWhiteSpace(before?.Species)       ? null : before.Value.Species!.Trim();
        var beforeSecurityLevel  = string.IsNullOrWhiteSpace(before?.SecurityLevel) ? null : before.Value.SecurityLevel!.Trim();
        var afterDailyRate      = row.EffectiveDailyRate ?? 0m;
        var afterDefraDailyRate = row.EffectiveDefraDailyRate ?? 0m;
        var afterPlanByWeek     = row.EffectivePlanByWeek ?? false;
        var afterSpecies        = string.IsNullOrWhiteSpace(row.EffectiveSpecies)       ? null : row.EffectiveSpecies.Trim();
        var afterSecurityLevel  = string.IsNullOrWhiteSpace(row.EffectiveSecurityLevel) ? null : row.EffectiveSecurityLevel.Trim();

        var rows = new List<RateChangeHistoryRow>();
        if (beforeDailyRate != afterDailyRate)
            rows.Add(MakeHistoryRow(c, "dailyrate", beforeDailyRate.ToString(), afterDailyRate.ToString(), "Update"));
        if (beforeDefraDailyRate != afterDefraDailyRate)
            rows.Add(MakeHistoryRow(c, "defradailyrate", beforeDefraDailyRate.ToString(), afterDefraDailyRate.ToString(), "Update"));
        if (beforePlanByWeek != afterPlanByWeek)
            rows.Add(MakeHistoryRow(c, "planbyweek", beforePlanByWeek.ToString(), afterPlanByWeek.ToString(), "Update"));
        if (!string.Equals(beforeSpecies, afterSpecies, StringComparison.OrdinalIgnoreCase))
            rows.Add(MakeHistoryRow(c, "species", beforeSpecies, afterSpecies, "Update"));
        if (!string.Equals(beforeSecurityLevel, afterSecurityLevel, StringComparison.OrdinalIgnoreCase))
            rows.Add(MakeHistoryRow(c, "security_level", beforeSecurityLevel, afterSecurityLevel, "Update"));
        return [.. rows];
    }

    // ── Apply Staff rates in one transaction ─────────────────────────────────

    /// <inheritdoc />
    public async Task<(int Updated, int Unchanged)> ApplyStaffRatesAsync(
        IReadOnlyList<StaffStagingRow> stagingRows,
        BulkRatesJobQueueEntry entry,
        DateTime appliedAt,
        CancellationToken cancellationToken = default)
    {
        int updated = 0, unchanged = 0;

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();

        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        var pcGrades = stagingRows
            .Select(r => r.PcGrade)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var liveLookup = await GetStaffRowsForUpdateAsync(conn, tx, pcGrades, entry.FpsYear, cancellationToken);

        foreach (var row in stagingRows)
        {
            switch (row.CalculatedAction)
            {
                case "NoChange":
                    unchanged++;
                    break;

                case "Update":
                    liveLookup.TryGetValue(row.PcGrade.ToUpperInvariant(), out var liveBefore);
                    var rowsAffected = await UpdateStaffRowAsync(conn, tx, row, entry.FpsYear, cancellationToken);
                    if (rowsAffected == 0)
                        throw new InvalidOperationException(
                            $"BulkStaffRatesUpdate: UPDATE matched 0 rows for PcGrade='{row.PcGrade}' in JobQueueId={entry.JobQueueId:D}.");
                    foreach (var historyRow in BuildStaffHistory(row, liveBefore, entry, appliedAt))
                        await InsertHistoryRowAsync(conn, tx, historyRow, cancellationToken);
                    updated++;
                    break;

                case "Insert":
                    throw new InvalidOperationException(
                        $"BulkStaffRatesUpdate: Staff Insert is not supported. " +
                        $"PcGrade='{row.PcGrade}' in JobQueueId={entry.JobQueueId:D} " +
                        "has CalculatedAction=Insert, which indicates an upstream defect.");

                default:
                    throw new InvalidOperationException(
                        $"BulkStaffRatesUpdate: unexpected CalculatedAction '{row.CalculatedAction}' " +
                        $"for PcGrade='{row.PcGrade}' in JobQueueId={entry.JobQueueId:D}.");
            }
        }

        await tx.CommitAsync(cancellationToken);
        return (updated, unchanged);
    }

    private static async Task<Dictionary<string, (decimal? PayRate, decimal? Npr, decimal? Ohr)>> GetStaffRowsForUpdateAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> pcGrades, int fpsYear, CancellationToken ct)
    {
        var result = new Dictionary<string, (decimal? PayRate, decimal? Npr, decimal? Ohr)>(StringComparer.OrdinalIgnoreCase);
        if (pcGrades.Count == 0) return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT pcgrade, payrate::numeric, npr::numeric, ohr::numeric
            FROM fps.profitcentregrade
            WHERE fpsyear = @fpsyear AND pcgrade = ANY(@grades)
            ORDER BY pcgrade
            FOR UPDATE;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("grades", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = pcGrades.ToArray() });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var pcGrade = r.GetString(0);
            result[pcGrade.ToUpperInvariant()] = (
                r.IsDBNull(1) ? null : r.GetDecimal(1),
                r.IsDBNull(2) ? null : r.GetDecimal(2),
                r.IsDBNull(3) ? null : r.GetDecimal(3));
        }
        return result;
    }

    private static async Task<int> UpdateStaffRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        StaffStagingRow row, int fpsYear, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            UPDATE fps.profitcentregrade
            SET payrate = @payrate::money,
                npr     = @npr::money,
                ohr     = @ohr::money
            WHERE pcgrade = @pcgrade AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("payrate", row.EffectivePayRate ?? 0m);
        cmd.Parameters.AddWithValue("npr",     row.EffectiveNpr ?? 0m);
        cmd.Parameters.AddWithValue("ohr",     row.EffectiveOhr ?? 0m);
        cmd.Parameters.AddWithValue("pcgrade", row.PcGrade);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        return await cmd.ExecuteNonQueryAsync(ct);
    }

    private static RateChangeHistoryRow[] BuildStaffHistory(
        StaffStagingRow row, (decimal? PayRate, decimal? Npr, decimal? Ohr)? before,
        BulkRatesJobQueueEntry entry, DateTime appliedAt)
    {
        var key = JsonSerializer.Serialize(new { pcGrade = row.PcGrade });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "Staff", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);

        var beforePayRate = before?.PayRate ?? 0m;
        var beforeNpr     = before?.Npr ?? 0m;
        var beforeOhr     = before?.Ohr ?? 0m;
        var afterPayRate  = row.EffectivePayRate ?? 0m;
        var afterNpr      = row.EffectiveNpr ?? 0m;
        var afterOhr      = row.EffectiveOhr ?? 0m;

        var rows = new List<RateChangeHistoryRow>();
        if (beforePayRate != afterPayRate)
            rows.Add(MakeHistoryRow(c, "payrate", beforePayRate.ToString(), afterPayRate.ToString(), "Update"));
        if (beforeNpr != afterNpr)
            rows.Add(MakeHistoryRow(c, "npr", beforeNpr.ToString(), afterNpr.ToString(), "Update"));
        if (beforeOhr != afterOhr)
            rows.Add(MakeHistoryRow(c, "ohr", beforeOhr.ToString(), afterOhr.ToString(), "Update"));
        return [.. rows];
    }

    // ── Apply FEC (Test/AGRUP) rates in one transaction ──────────────────────

    /// <inheritdoc />
    public async Task<(int FecInserted, int FecUpdated, int FecUnchanged, int AgrupInserted, int AgrupUpdated, int AgrupUnchanged)> ApplyFecRatesAsync(
        IReadOnlyList<FecStagingRow> fecRows,
        IReadOnlyList<AgrupStagingRow> agrupRows,
        BulkRatesJobQueueEntry entry,
        DateTime appliedAt,
        CancellationToken cancellationToken = default)
    {
        int fecInserted = 0, fecUpdated = 0, fecUnchanged = 0;
        int agrupInserted = 0, agrupUpdated = 0, agrupUnchanged = 0;
        var historyRows = new List<RateChangeHistoryRow>();

        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();

        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        var testCodes = fecRows.Select(r => r.TestCode)
            .Concat(agrupRows.Select(r => r.TestCode))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var liveFecLookup   = await GetFecRowsForUpdateAsync(conn, tx, testCodes, entry.FpsYear, cancellationToken);
        var liveAgrupLookup = await GetAgrupRowsForUpdateAsync(conn, tx, testCodes, entry.FpsYear, cancellationToken);

        foreach (var row in fecRows)
        {
            var effectiveRate = row.EffectiveNewRate ?? 0m;
            liveFecLookup.TryGetValue(row.TestCode.ToUpperInvariant(), out var live);

            switch (row.CalculatedAction)
            {
                case "Insert":
                    await InsertFecRowAsync(conn, tx, row, entry.FpsYear, effectiveRate, cancellationToken);
                    historyRows.AddRange(BuildFecInsertHistory(row, entry, appliedAt, effectiveRate));
                    fecInserted++;
                    break;

                case "Update":
                case "ZeroRateWithdrawal":
                    await UpdateFecRowAsync(conn, tx, row.TestCode, entry.FpsYear, effectiveRate, cancellationToken);
                    historyRows.AddRange(BuildFecUpdateHistory(
                        row, (live.UnitPriceVla ?? 0m, live.DefraUnitPrice ?? 0m),
                        entry, appliedAt, effectiveRate, row.CalculatedAction!));
                    fecUpdated++;
                    break;

                default:
                    fecUnchanged++;
                    break;
            }
        }

        foreach (var row in agrupRows)
        {
            var agrupKey = (row.TestCode.ToUpperInvariant(), row.Buyer.ToUpperInvariant());
            var effectiveRate = row.EffectiveNewRate ?? 0m;
            liveAgrupLookup.TryGetValue(agrupKey, out var live);

            switch (row.CalculatedAction)
            {
                case "Insert":
                    await InsertAgrupRowAsync(conn, tx, row, entry.FpsYear, effectiveRate, appliedAt, cancellationToken);
                    await WriteTestreqLogAsync(conn, tx,
                        row.TestCode, row.Buyer, entry.FpsYear, effectiveRate,
                        row.NoRequired, row.ProjectBuyerCode, row.TestBuyerCode, active: 1,
                        appliedAt, entry.ApprovedBy, "I", cancellationToken);
                    historyRows.AddRange(BuildAgrupInsertHistory(row, entry, appliedAt, effectiveRate));
                    agrupInserted++;
                    break;

                case "Update":
                case "ZeroRateWithdrawal":
                    await UpdateAgrupRowAsync(conn, tx, row.TestCode, row.Buyer, entry.FpsYear, effectiveRate, cancellationToken);
                    await WriteTestreqLogAsync(conn, tx,
                        row.TestCode, row.Buyer, entry.FpsYear, effectiveRate,
                        live.NoRequired, live.ProjectBuyerCode, live.TestBuyerCode, live.Active,
                        appliedAt, entry.ApprovedBy, "I", cancellationToken);
                    historyRows.AddRange(BuildAgrupUpdateHistory(
                        row, live.UnitPrice, entry, appliedAt, effectiveRate, row.CalculatedAction!));
                    agrupUpdated++;
                    break;

                default:
                    agrupUnchanged++;
                    break;
            }
        }

        foreach (var historyRow in historyRows)
            await InsertHistoryRowAsync(conn, tx, historyRow, cancellationToken);

        await tx.CommitAsync(cancellationToken);
        return (fecInserted, fecUpdated, fecUnchanged, agrupInserted, agrupUpdated, agrupUnchanged);
    }

    private static async Task<Dictionary<string, (decimal? UnitPriceVla, decimal? DefraUnitPrice)>> GetFecRowsForUpdateAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> testCodes, int fpsYear, CancellationToken ct)
    {
        var result = new Dictionary<string, (decimal? UnitPriceVla, decimal? DefraUnitPrice)>(StringComparer.OrdinalIgnoreCase);
        if (testCodes.Count == 0) return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT itemcode, unitpricevla::numeric, defraunitprice::numeric
            FROM fps.testorproduct
            WHERE fpsyear = @fpsyear AND itemcode = ANY(@codes)
            FOR UPDATE;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("codes", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = testCodes.ToArray() });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var testCode = r.GetString(0);
            result[testCode.ToUpperInvariant()] = (
                r.IsDBNull(1) ? null : r.GetDecimal(1),
                r.IsDBNull(2) ? null : r.GetDecimal(2));
        }
        return result;
    }

    private static async Task InsertFecRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        FecStagingRow row, int fpsYear, decimal rate, CancellationToken ct)
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
        string testCode, int fpsYear, decimal newRate, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            UPDATE fps.testorproduct
            SET unitpricevla = @rate, defraunitprice = @rate
            WHERE itemcode = @itemcode AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("rate",     newRate);
        cmd.Parameters.AddWithValue("itemcode", testCode);
        cmd.Parameters.AddWithValue("fpsyear",  fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<Dictionary<(string TestCode, string Buyer), (decimal? UnitPrice, double? NoRequired, string? ProjectBuyerCode, string? TestBuyerCode, short? Active)>> GetAgrupRowsForUpdateAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        IReadOnlyCollection<string> testCodes, int fpsYear, CancellationToken ct)
    {
        var result = new Dictionary<(string, string), (decimal? UnitPrice, double? NoRequired, string? ProjectBuyerCode, string? TestBuyerCode, short? Active)>();
        if (testCodes.Count == 0) return result;

        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            SELECT testcode, buyer, unitprice::numeric, projectbuyercode, testbuyercode,
                   norequired, active
            FROM fps.tlkptestreqmt
            WHERE fpsyear = @fpsyear AND testcode = ANY(@codes)
            FOR UPDATE;";
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.Add(new NpgsqlParameter("codes", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = testCodes.ToArray() });

        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var testCode = r.GetString(0);
            var buyer    = r.GetString(1);
            result[(testCode.ToUpperInvariant(), buyer.ToUpperInvariant())] = (
                r.IsDBNull(2) ? null : r.GetDecimal(2),
                r.IsDBNull(5) ? null : r.GetDouble(5),
                r.IsDBNull(3) ? null : r.GetString(3),
                r.IsDBNull(4) ? null : r.GetString(4),
                r.IsDBNull(6) ? null : r.GetInt16(6));
        }
        return result;
    }

    private static async Task InsertAgrupRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        AgrupStagingRow row, int fpsYear, decimal rate, DateTime executionTimestamp, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"
            INSERT INTO fps.tlkptestreqmt
                (testcode, buyer, unitprice, norequired, projectbuyercode, testbuyercode, datecreated, active, fpsyear)
            VALUES
                (@testcode, @buyer, @unitprice, @norequired, @projectbuyercode, @testbuyercode, @datecreated, 1, @fpsyear);";
        cmd.Parameters.AddWithValue("testcode",         row.TestCode);
        cmd.Parameters.AddWithValue("buyer",            row.Buyer);
        cmd.Parameters.AddWithValue("unitprice",        rate);
        cmd.Parameters.AddWithValue("norequired",       (object?)row.NoRequired ?? DBNull.Value);
        cmd.Parameters.AddWithValue("projectbuyercode", (object?)row.ProjectBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("testbuyercode",    (object?)row.TestBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("datecreated",      executionTimestamp);
        cmd.Parameters.AddWithValue("fpsyear",          fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task UpdateAgrupRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string testCode, string buyer, int fpsYear, decimal newRate, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        // Spec §2.3: Update UnitPrice only; do not touch NoRequired, DateCreated, Active,
        // ProjectBuyerCode/TestBuyerCode (existing-row routing immutability).
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

    private static async Task WriteTestreqLogAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        string testCode, string buyer, int fpsYear, decimal unitPrice,
        double? noRequired, string? projectBuyerCode, string? testBuyerCode, short? active,
        DateTime executionTimestamp, string? userId, string insertDelete, CancellationToken ct)
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
        cmd.Parameters.AddWithValue("jobcode",          (object?)projectBuyerCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("fpsyear",          fpsYear);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static IEnumerable<RateChangeHistoryRow> BuildFecInsertHistory(
        FecStagingRow row, BulkRatesJobQueueEntry entry, DateTime appliedAt, decimal newRate)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "FEC", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);
        yield return MakeHistoryRow(c, "unitpricevla",   null, newRate.ToString(), "Insert");
        yield return MakeHistoryRow(c, "defraunitprice", null, newRate.ToString(), "Insert");
    }

    private static IEnumerable<RateChangeHistoryRow> BuildFecUpdateHistory(
        FecStagingRow row,
        (decimal UnitPriceVla, decimal DefraUnitPrice) before,
        BulkRatesJobQueueEntry entry, DateTime appliedAt, decimal newRate, string changeType)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "FEC", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);
        yield return MakeHistoryRow(c, "unitpricevla",   before.UnitPriceVla.ToString(), newRate.ToString(), changeType);
        yield return MakeHistoryRow(c, "defraunitprice", before.DefraUnitPrice.ToString(), newRate.ToString(), changeType);
    }

    private static IEnumerable<RateChangeHistoryRow> BuildAgrupInsertHistory(
        AgrupStagingRow row, BulkRatesJobQueueEntry entry, DateTime appliedAt, decimal newRate)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode, buyer = row.Buyer });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "AGRUP", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);
        yield return MakeHistoryRow(c, "unitprice", null, newRate.ToString(), "Insert");
    }

    private static IEnumerable<RateChangeHistoryRow> BuildAgrupUpdateHistory(
        AgrupStagingRow row, decimal? currentUnitPrice, BulkRatesJobQueueEntry entry, DateTime appliedAt,
        decimal newRate, string changeType)
    {
        var key = JsonSerializer.Serialize(new { testCode = row.TestCode, buyer = row.Buyer });
        var c = (entry.JobQueueId, entry.JobExecutionId, entry.JobId, entry.FpsYear,
                 "AGRUP", key, entry.RequestedBy, entry.ApprovedBy, appliedAt);
        yield return MakeHistoryRow(c, "unitprice", currentUnitPrice?.ToString(), newRate.ToString(), changeType);
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

    // ── Shared history insert ────────────────────────────────────────────────

    internal static async Task InsertHistoryRowAsync(
        NpgsqlConnection conn, NpgsqlTransaction tx,
        RateChangeHistoryRow row, CancellationToken ct)
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
        cmd.Parameters.AddWithValue("jobqueueid",     row.JobQueueId);
        cmd.Parameters.AddWithValue("jobexecutionid", row.JobExecutionId);
        cmd.Parameters.AddWithValue("jobid",          row.JobId);
        cmd.Parameters.AddWithValue("fpsyear",        row.FpsYear);
        cmd.Parameters.AddWithValue("ratecategory",   row.RateCategory);
        cmd.Parameters.AddWithValue("businesskey",    row.BusinessKeyJson);
        cmd.Parameters.AddWithValue("fieldname",      row.FieldName);
        cmd.Parameters.AddWithValue("oldvalue",       (object?)row.OldValue    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("newvalue",       (object?)row.NewValue    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("changetype",     row.ChangeType);
        cmd.Parameters.AddWithValue("requestedby",    (object?)row.RequestedBy ?? DBNull.Value);
        cmd.Parameters.AddWithValue("approvedby",     (object?)row.ApprovedBy  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("appliedatutc",   row.AppliedAtUtc);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // ── Audit log ────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public async Task WriteJobQueueLogAsync(
        Guid jobQueueId,
        string note,
        string? actor,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var conn = (NpgsqlConnection)dbContext.Database.GetDbConnection();

        // Resolve current statusid (required by fps.job_queue_log FK)
        int? statusId = null;
        await using (var statusCmd = conn.CreateCommand())
        {
            statusCmd.CommandText = "SELECT statusid FROM fps.job_queue WHERE jobqueueid = @jqid;";
            statusCmd.Parameters.AddWithValue("jqid", jobQueueId);
            var result = await statusCmd.ExecuteScalarAsync(cancellationToken);
            statusId = result is null or DBNull ? null : (int?)Convert.ToInt32(result);
        }

        if (statusId is null)
        {
            _logger.LogWarning(
                "WriteJobQueueLogAsync: jobqueueid {JobQueueId} not found; log entry skipped.", jobQueueId);
            return;
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO fps.job_queue_log (jobqueueid, statusid, performedby, logtime, note)
            VALUES (@jobqueueid, @statusid, @performedby, NOW(), @note);";
        cmd.Parameters.AddWithValue("jobqueueid",  jobQueueId);
        cmd.Parameters.AddWithValue("statusid",    statusId.Value);
        cmd.Parameters.AddWithValue("performedby", (object?)actor ?? DBNull.Value);
        cmd.Parameters.AddWithValue("note",        (object?)note ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
