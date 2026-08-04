using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.Repositories.BulkRates;

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
