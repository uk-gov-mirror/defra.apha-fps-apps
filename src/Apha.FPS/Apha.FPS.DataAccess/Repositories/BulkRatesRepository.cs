using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Raw-Npgsql implementation of <see cref="IBulkRatesRepository"/> for the FPS API.
    /// Uses <see cref="FpsDbContext"/> to obtain the underlying Npgsql connection;
    /// no EF entities are used — all SQL is written explicitly.
    /// </summary>
    public class BulkRatesRepository : IBulkRatesRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly ILogger<BulkRatesRepository> _logger;

        public BulkRatesRepository(FpsDbContext dbContext, ILogger<BulkRatesRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── Connection helper ────────────────────────────────────────────────────

        private async Task<NpgsqlConnection> OpenAsync(CancellationToken ct)
        {
            await _dbContext.Database.OpenConnectionAsync(ct);
            return (NpgsqlConnection)_dbContext.Database.GetDbConnection();
        }

        // ── Job master / status lookup ───────────────────────────────────────────

        public async Task<int?> GetJobIdByNameAsync(string jobName, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT jobid FROM fps.job_master WHERE jobname = @jobname;";
            cmd.Parameters.AddWithValue("jobname", jobName);
            var result = await cmd.ExecuteScalarAsync(ct);
            return result is null or DBNull ? null : (int?)Convert.ToInt32(result);
        }

        public async Task<int?> GetStatusIdByNameAsync(int jobId, string statusName, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT statusid FROM fps.job_status WHERE jobid = @jobid AND status = @status;";
            cmd.Parameters.AddWithValue("jobid", jobId);
            cmd.Parameters.AddWithValue("status", statusName);
            var result = await cmd.ExecuteScalarAsync(ct);
            return result is null or DBNull ? null : (int?)Convert.ToInt32(result);
        }

        // ── Queue entry CRUD ─────────────────────────────────────────────────────

        public async Task<BulkRatesQueueEntry> CreateRequestAsync(
            Guid jobQueueId, Guid jobExecutionId, int jobId, int initiatedStatusId,
            string requestedBy, DateTime requestedAtUtc, int fpsYear,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO fps.job_queue
                    (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, fpsyear)
                VALUES
                    (@jobqueueid, @jobexecutionid, @jobid, @statusid, @requestedby, @requested_at_utc, @fpsyear);";
            cmd.Parameters.AddWithValue("jobqueueid",        jobQueueId);
            cmd.Parameters.AddWithValue("jobexecutionid",    jobExecutionId);
            cmd.Parameters.AddWithValue("jobid",             jobId);
            cmd.Parameters.AddWithValue("statusid",          initiatedStatusId);
            cmd.Parameters.AddWithValue("requestedby",       requestedBy);
            cmd.Parameters.AddWithValue("requested_at_utc",  requestedAtUtc);
            cmd.Parameters.AddWithValue("fpsyear",           fpsYear);
            await cmd.ExecuteNonQueryAsync(ct);

            _logger.LogInformation("Created job_queue row | JobQueueId={JobQueueId} | JobId={JobId} | FpsYear={FpsYear}",
                jobQueueId, jobId, fpsYear);

            return await GetRequestAsync(jobQueueId, ct)
                ?? throw new InvalidOperationException($"Row just inserted for {jobQueueId} could not be read back.");
        }

        public async Task<BulkRatesQueueEntry?> GetRequestAsync(Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT q.jobqueueid, q.jobid, m.jobname, q.statusid, s.status,
                       q.jobexecutionid, q.requestedby, q.requested_at_utc, q.fpsyear,
                       q.configuration_json,
                       q.approved_by, q.approved_at_utc,
                       q.rejected_by, q.rejected_at_utc, q.rejection_reason,
                       q.cancelled_by, q.cancelled_at_utc, q.cancellation_reason,
                       q.triggered_by, q.triggered_at_utc,
                       q.startdatetime, q.enddatetime, q.errormessage
                FROM fps.job_queue q
                JOIN fps.job_master m ON m.jobid = q.jobid
                JOIN fps.job_status s ON s.statusid = q.statusid AND s.jobid = q.jobid
                WHERE q.jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return null;

            return ReadQueueEntry(reader);
        }

        public async Task<IReadOnlyList<BulkRatesQueueEntry>> GetRequestsAsync(
            string? jobName, int? fpsYear, string? status, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            var sql = new StringBuilder(@"
                SELECT q.jobqueueid, q.jobid, m.jobname, q.statusid, s.status,
                       q.jobexecutionid, q.requestedby, q.requested_at_utc, q.fpsyear,
                       q.configuration_json,
                       q.approved_by, q.approved_at_utc,
                       q.rejected_by, q.rejected_at_utc, q.rejection_reason,
                       q.cancelled_by, q.cancelled_at_utc, q.cancellation_reason,
                       q.triggered_by, q.triggered_at_utc,
                       q.startdatetime, q.enddatetime, q.errormessage
                FROM fps.job_queue q
                JOIN fps.job_master m ON m.jobid = q.jobid
                JOIN fps.job_status s ON s.statusid = q.statusid AND s.jobid = q.jobid
                WHERE 1=1");

            if (jobName != null)
            {
                sql.Append(" AND m.jobname = @jobname");
                cmd.Parameters.AddWithValue("jobname", jobName);
            }
            if (fpsYear.HasValue)
            {
                sql.Append(" AND q.fpsyear = @fpsyear");
                cmd.Parameters.AddWithValue("fpsyear", fpsYear.Value);
            }
            if (status != null)
            {
                sql.Append(" AND s.status = @status");
                cmd.Parameters.AddWithValue("status", status);
            }
            sql.Append(" ORDER BY q.requested_at_utc DESC;");
            cmd.CommandText = sql.ToString();

            var results = new List<BulkRatesQueueEntry>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                results.Add(ReadQueueEntry(reader));

            return results;
        }

        // ── Status transitions ───────────────────────────────────────────────────

        public async Task<bool> TransitionStatusAsync(
            Guid jobQueueId, int expectedStatusId, int newStatusId,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE fps.job_queue
                SET statusid = @new_statusid, updated_at = NOW()
                WHERE jobqueueid = @jobqueueid AND statusid = @expected_statusid;";
            cmd.Parameters.AddWithValue("new_statusid",      newStatusId);
            cmd.Parameters.AddWithValue("jobqueueid",        jobQueueId);
            cmd.Parameters.AddWithValue("expected_statusid", expectedStatusId);
            var affected = await cmd.ExecuteNonQueryAsync(ct);
            return affected > 0;
        }

        public async Task SetApprovalAsync(
            Guid jobQueueId, Guid jobExecutionId,
            string approvedBy, DateTime approvedAtUtc,
            string triggeredBy, DateTime triggeredAtUtc,
            int approvedStatusId,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE fps.job_queue
                SET statusid         = @statusid,
                    approved_by      = @approved_by,
                    approved_at_utc  = @approved_at_utc,
                    triggered_by     = @triggered_by,
                    triggered_at_utc = @triggered_at_utc,
                    updated_at       = NOW()
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("statusid",         approvedStatusId);
            cmd.Parameters.AddWithValue("approved_by",      approvedBy);
            cmd.Parameters.AddWithValue("approved_at_utc",  approvedAtUtc);
            cmd.Parameters.AddWithValue("triggered_by",     triggeredBy);
            cmd.Parameters.AddWithValue("triggered_at_utc", triggeredAtUtc);
            cmd.Parameters.AddWithValue("jobqueueid",       jobQueueId);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task SetRejectionAsync(
            Guid jobQueueId, string rejectedBy, DateTime rejectedAtUtc,
            string reason, int rejectedStatusId,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE fps.job_queue
                SET statusid        = @statusid,
                    rejected_by     = @rejected_by,
                    rejected_at_utc = @rejected_at_utc,
                    rejection_reason = @rejection_reason,
                    updated_at      = NOW()
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("statusid",         rejectedStatusId);
            cmd.Parameters.AddWithValue("rejected_by",      rejectedBy);
            cmd.Parameters.AddWithValue("rejected_at_utc",  rejectedAtUtc);
            cmd.Parameters.AddWithValue("rejection_reason", reason);
            cmd.Parameters.AddWithValue("jobqueueid",       jobQueueId);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task SetCancellationAsync(
            Guid jobQueueId, string cancelledBy, DateTime cancelledAtUtc,
            string? reason, int cancelledStatusId,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE fps.job_queue
                SET statusid             = @statusid,
                    cancelled_by         = @cancelled_by,
                    cancelled_at_utc     = @cancelled_at_utc,
                    cancellation_reason  = @cancellation_reason,
                    updated_at           = NOW()
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("statusid",            cancelledStatusId);
            cmd.Parameters.AddWithValue("cancelled_by",        cancelledBy);
            cmd.Parameters.AddWithValue("cancelled_at_utc",    cancelledAtUtc);
            cmd.Parameters.AddWithValue("cancellation_reason", (object?)reason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("jobqueueid",          jobQueueId);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        // ── configuration_json ───────────────────────────────────────────────────

        public async Task UpdateConfigurationJsonAsync(
            Guid jobQueueId, string configurationJson, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE fps.job_queue
                SET configuration_json = @configuration_json::jsonb,
                    updated_at         = NOW()
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("configuration_json", configurationJson);
            cmd.Parameters.AddWithValue("jobqueueid",         jobQueueId);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        // ── Audit log ────────────────────────────────────────────────────────────

        public async Task WriteJobQueueLogAsync(
            Guid jobQueueId, string note, string? actor, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);

            // Resolve current statusid (required by fps.job_queue_log FK constraint)
            int? statusId = null;
            await using (var statusCmd = conn.CreateCommand())
            {
                statusCmd.CommandText = "SELECT statusid FROM fps.job_queue WHERE jobqueueid = @jqid;";
                statusCmd.Parameters.AddWithValue("jqid", jobQueueId);
                var result = await statusCmd.ExecuteScalarAsync(ct);
                statusId = result is null or DBNull ? null : (int?)Convert.ToInt32(result);
            }

            if (statusId is null)
            {
                _logger.LogWarning("WriteJobQueueLogAsync: jobqueueid {JobQueueId} not found; log entry skipped.", jobQueueId);
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
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<IReadOnlyList<BulkRatesQueueLog>> GetJobQueueLogsAsync(
            Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT jobqueuelogid, jobqueueid, note, performedby, logtime
                FROM fps.job_queue_log
                WHERE jobqueueid = @jobqueueid
                ORDER BY logtime ASC;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

            var results = new List<BulkRatesQueueLog>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                results.Add(new BulkRatesQueueLog
                {
                    LogId       = reader.GetInt64(0),
                    JobQueueId  = reader.GetGuid(1),
                    Note        = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Actor       = reader.IsDBNull(3) ? null : reader.GetString(3),
                    CreatedAtUtc = reader.GetDateTime(4)
                });
            }
            return results;
        }

        // ── Staging — replace semantics ──────────────────────────────────────────

        public async Task ReplaceStagingFecAsync(
            Guid jobQueueId,
            IReadOnlyList<FecStagingRow> fecRows,
            IReadOnlyList<AgrupStagingRow> agrupRows,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            // Delete AGRUP first (child FK to FEC staging)
            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM fps.tblstagingtlkptestreqmt WHERE jobqueueid = @jqid;";
                del.Parameters.AddWithValue("jqid", jobQueueId);
                await del.ExecuteNonQueryAsync(ct);
            }
            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM fps.tblstagingtestorproduct WHERE jobqueueid = @jqid;";
                del.Parameters.AddWithValue("jqid", jobQueueId);
                await del.ExecuteNonQueryAsync(ct);
            }

            // Insert FEC rows
            foreach (var row in fecRows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.tblstagingtestorproduct
                        (jobqueueid, testcode, unitpricevla, defraunitprice, fecnewrate,
                         change, itemdescription, shortdescription, owner, comments)
                    VALUES
                        (@jobqueueid, @testcode, @unitpricevla, @defraunitprice, @fecnewrate,
                         @change, @itemdescription, @shortdescription, @owner, @comments);";
                ins.Parameters.AddWithValue("jobqueueid",       jobQueueId);
                ins.Parameters.AddWithValue("testcode",         row.TestCode);
                ins.Parameters.AddWithValue("unitpricevla",     (object?)row.UnitPriceVla ?? DBNull.Value);
                ins.Parameters.AddWithValue("defraunitprice",   (object?)row.DefraUnitPrice ?? DBNull.Value);
                ins.Parameters.AddWithValue("fecnewrate",       (object?)row.FecNewRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("change",           (object?)row.Change ?? DBNull.Value);
                ins.Parameters.AddWithValue("itemdescription",  (object?)row.ItemDescription ?? DBNull.Value);
                ins.Parameters.AddWithValue("shortdescription", (object?)row.ShortDescription ?? DBNull.Value);
                ins.Parameters.AddWithValue("owner",            (object?)row.Owner ?? DBNull.Value);
                ins.Parameters.AddWithValue("comments",         (object?)row.Comments ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            // Insert AGRUP rows
            foreach (var row in agrupRows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.tblstagingtlkptestreqmt
                        (jobqueueid, testcode, buyer, agrup, agrupnew, change,
                         norequired, datecreated, active, comments)
                    VALUES
                        (@jobqueueid, @testcode, @buyer, @agrup, @agrupnew, @change,
                         @norequired, @datecreated, @active, @comments);";
                ins.Parameters.AddWithValue("jobqueueid",  jobQueueId);
                ins.Parameters.AddWithValue("testcode",    row.TestCode);
                ins.Parameters.AddWithValue("buyer",       row.Buyer);
                ins.Parameters.AddWithValue("agrup",       (object?)row.Agrup ?? DBNull.Value);
                ins.Parameters.AddWithValue("agrupnew",    (object?)row.AgrupNew ?? DBNull.Value);
                ins.Parameters.AddWithValue("change",      (object?)row.Change ?? DBNull.Value);
                ins.Parameters.AddWithValue("norequired",  (object?)row.NoRequired ?? DBNull.Value);
                ins.Parameters.AddWithValue("datecreated", (object?)row.DateCreated ?? DBNull.Value);
                ins.Parameters.AddWithValue("active",      (object?)row.Active ?? DBNull.Value);
                ins.Parameters.AddWithValue("comments",    (object?)row.Comments ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "ReplaceStagingFec | JobQueueId={JobQueueId} | FecRows={FecRows} | AgrupRows={AgrupRows}",
                jobQueueId, fecRows.Count, agrupRows.Count);
        }

        public async Task ReplaceStagingStaffAsync(
            Guid jobQueueId,
            IReadOnlyList<StaffStagingRow> rows,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM fps.tblstagingprofitcentregrade WHERE jobqueueid = @jqid;";
                del.Parameters.AddWithValue("jqid", jobQueueId);
                await del.ExecuteNonQueryAsync(ct);
            }

            foreach (var row in rows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.tblstagingprofitcentregrade
                        (jobqueueid, pcgrade, payrate, npr, ohr)
                    VALUES
                        (@jobqueueid, @pcgrade, @payrate, @npr, @ohr);";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("pcgrade",    row.PcGrade);
                ins.Parameters.AddWithValue("payrate",    (object?)row.PayRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("npr",        (object?)row.Npr ?? DBNull.Value);
                ins.Parameters.AddWithValue("ohr",        (object?)row.Ohr ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "ReplaceStagingStaff | JobQueueId={JobQueueId} | Rows={Rows}",
                jobQueueId, rows.Count);
        }

        public async Task ReplaceStagingAnimalAsync(
            Guid jobQueueId,
            IReadOnlyList<AnimalStagingRow> rows,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM fps.tblstaginganimals WHERE jobqueueid = @jqid;";
                del.Parameters.AddWithValue("jqid", jobQueueId);
                await del.ExecuteNonQueryAsync(ct);
            }

            foreach (var row in rows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.tblstaginganimals
                        (jobqueueid, animaltype, species, security_level,
                         dailyrate, defradailyrate, planbyweek)
                    VALUES
                        (@jobqueueid, @animaltype, @species, @security_level,
                         @dailyrate, @defradailyrate, @planbyweek);";
                ins.Parameters.AddWithValue("jobqueueid",    jobQueueId);
                ins.Parameters.AddWithValue("animaltype",    row.AnimalType);
                ins.Parameters.AddWithValue("species",       (object?)row.Species ?? DBNull.Value);
                ins.Parameters.AddWithValue("security_level",(object?)row.SecurityLevel ?? DBNull.Value);
                ins.Parameters.AddWithValue("dailyrate",     (object?)row.DailyRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("defradailyrate",(object?)row.DefraDailyRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("planbyweek",    (object?)row.PlanByWeek ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "ReplaceStagingAnimal | JobQueueId={JobQueueId} | Rows={Rows}",
                jobQueueId, rows.Count);
        }

        public async Task ClearStagingByJobQueueIdAsync(
            Guid jobQueueId, string jobName, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            // FEC/AGRUP: AGRUP first (child FK), then FEC
            await DeleteFromAsync(conn, tx, "fps.tblstagingtlkptestreqmt", jobQueueId, ct);
            await DeleteFromAsync(conn, tx, "fps.tblstagingtestorproduct", jobQueueId, ct);
            await DeleteFromAsync(conn, tx, "fps.tblstagingprofitcentregrade", jobQueueId, ct);
            await DeleteFromAsync(conn, tx, "fps.tblstaginganimals", jobQueueId, ct);

            await tx.CommitAsync(ct);

            _logger.LogInformation("ClearStagingByJobQueueId | JobQueueId={JobQueueId} | JobName={JobName}",
                jobQueueId, jobName);
        }

        // ── Staging read ─────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<FecStagingRow>> GetFecStagingRowsAsync(
            Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT jobqueueid, testcode, unitpricevla::numeric, defraunitprice::numeric,
                       fecnewrate::numeric, change::numeric,
                       itemdescription, shortdescription, owner, comments
                FROM fps.tblstagingtestorproduct
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

            var rows = new List<FecStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new FecStagingRow
                {
                    JobQueueId       = reader.GetGuid(0),
                    TestCode         = reader.GetString(1),
                    UnitPriceVla     = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    DefraUnitPrice   = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    FecNewRate       = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    Change           = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                    ItemDescription  = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ShortDescription = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Owner            = reader.IsDBNull(8) ? null : reader.GetString(8),
                    Comments         = reader.IsDBNull(9) ? null : reader.GetString(9)
                });
            }
            return rows;
        }

        public async Task<IReadOnlyList<AgrupStagingRow>> GetAgrupStagingRowsAsync(
            Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT jobqueueid, testcode, buyer,
                       agrup::numeric, agrupnew::numeric, change::numeric,
                       norequired, datecreated, active, comments
                FROM fps.tblstagingtlkptestreqmt
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

            var rows = new List<AgrupStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new AgrupStagingRow
                {
                    JobQueueId  = reader.GetGuid(0),
                    TestCode    = reader.GetString(1),
                    Buyer       = reader.GetString(2),
                    Agrup       = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    AgrupNew    = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    Change      = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                    NoRequired  = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                    DateCreated = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    Active      = reader.IsDBNull(8) ? null : reader.GetInt16(8),
                    Comments    = reader.IsDBNull(9) ? null : reader.GetString(9)
                });
            }
            return rows;
        }

        public async Task<IReadOnlyList<StaffStagingRow>> GetStaffStagingRowsAsync(
            Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT jobqueueid, pcgrade,
                       payrate::numeric, npr::numeric, ohr::numeric
                FROM fps.tblstagingprofitcentregrade
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

            var rows = new List<StaffStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new StaffStagingRow
                {
                    JobQueueId = reader.GetGuid(0),
                    PcGrade    = reader.GetString(1),
                    PayRate    = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    Npr        = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Ohr        = reader.IsDBNull(4) ? null : reader.GetDecimal(4)
                });
            }
            return rows;
        }

        public async Task<IReadOnlyList<AnimalStagingRow>> GetAnimalStagingRowsAsync(
            Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT jobqueueid, animaltype, species, security_level,
                       dailyrate::numeric, defradailyrate::numeric, planbyweek
                FROM fps.tblstaginganimals
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

            var rows = new List<AnimalStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new AnimalStagingRow
                {
                    JobQueueId    = reader.GetGuid(0),
                    AnimalType    = reader.GetString(1),
                    Species       = reader.IsDBNull(2) ? null : reader.GetString(2),
                    SecurityLevel = reader.IsDBNull(3) ? null : reader.GetString(3),
                    DailyRate     = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    DefraDailyRate = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                    PlanByWeek    = reader.IsDBNull(6) ? null : reader.GetBoolean(6)
                });
            }
            return rows;
        }

        // ── Validation errors ────────────────────────────────────────────────────

        public async Task ReplaceValidationErrorsAsync(
            Guid jobQueueId,
            IReadOnlyList<StagingValidationError> errors,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM fps.staging_validation_error WHERE jobqueueid = @jqid;";
                del.Parameters.AddWithValue("jqid", jobQueueId);
                await del.ExecuteNonQueryAsync(ct);
            }

            foreach (var err in errors)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.staging_validation_error
                        (jobqueueid, upload_version, source_row_number, field_name,
                         validation_code, severity, validation_message)
                    VALUES
                        (@jobqueueid, @upload_version, @source_row_number, @field_name,
                         @validation_code, @severity, @validation_message);";
                ins.Parameters.AddWithValue("jobqueueid",        jobQueueId);
                ins.Parameters.AddWithValue("upload_version",    err.UploadVersion);
                ins.Parameters.AddWithValue("source_row_number", err.SourceRowNumber);
                ins.Parameters.AddWithValue("field_name",        (object?)err.FieldName ?? DBNull.Value);
                ins.Parameters.AddWithValue("validation_code",   (object?)err.ValidationCode ?? DBNull.Value);
                ins.Parameters.AddWithValue("severity",          err.Severity);
                ins.Parameters.AddWithValue("validation_message",err.ValidationMessage);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "ReplaceValidationErrors | JobQueueId={JobQueueId} | Errors={Errors}",
                jobQueueId, errors.Count);
        }

        public async Task<IReadOnlyList<StagingValidationError>> GetValidationErrorsAsync(
            Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id, jobqueueid, upload_version, source_row_number, field_name,
                       validation_code, severity, validation_message
                FROM fps.staging_validation_error
                WHERE jobqueueid = @jobqueueid
                ORDER BY source_row_number, id;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);

            var results = new List<StagingValidationError>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                results.Add(new StagingValidationError
                {
                    Id                = reader.GetInt64(0),
                    JobQueueId        = reader.GetGuid(1),
                    UploadVersion     = reader.GetInt32(2),
                    SourceRowNumber   = reader.GetInt32(3),
                    FieldName         = reader.IsDBNull(4) ? null : reader.GetString(4),
                    ValidationCode    = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Severity          = reader.GetString(6),
                    ValidationMessage = reader.GetString(7)
                });
            }
            return results;
        }

        // ── Cancel + clear staging (atomic) ──────────────────────────────────────

        public async Task CancelAndClearStagingAsync(
            Guid jobQueueId, string jobName,
            string cancelledBy, DateTime cancelledAtUtc,
            string? reason, int cancelledStatusId,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            // Update job_queue with cancellation metadata
            await using (var upd = conn.CreateCommand())
            {
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.job_queue
                    SET statusid            = @statusid,
                        cancelled_by        = @cancelled_by,
                        cancelled_at_utc    = @cancelled_at_utc,
                        cancellation_reason = @cancellation_reason,
                        updated_at          = NOW()
                    WHERE jobqueueid = @jobqueueid;";
                upd.Parameters.AddWithValue("statusid",            cancelledStatusId);
                upd.Parameters.AddWithValue("cancelled_by",        cancelledBy);
                upd.Parameters.AddWithValue("cancelled_at_utc",    cancelledAtUtc);
                upd.Parameters.AddWithValue("cancellation_reason", (object?)reason ?? DBNull.Value);
                upd.Parameters.AddWithValue("jobqueueid",          jobQueueId);
                await upd.ExecuteNonQueryAsync(ct);
            }

            // Clear all staging rows within the same transaction
            await DeleteFromAsync(conn, tx, "fps.tblstagingtlkptestreqmt",   jobQueueId, ct);
            await DeleteFromAsync(conn, tx, "fps.tblstagingtestorproduct",    jobQueueId, ct);
            await DeleteFromAsync(conn, tx, "fps.tblstagingprofitcentregrade", jobQueueId, ct);
            await DeleteFromAsync(conn, tx, "fps.tblstaginganimals",          jobQueueId, ct);

            // Clear validation errors within the same transaction
            await using (var del = conn.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = "DELETE FROM fps.staging_validation_error WHERE jobqueueid = @jqid;";
                del.Parameters.AddWithValue("jqid", jobQueueId);
                await del.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "CancelAndClearStaging committed | JobQueueId={JobQueueId} | CancelledBy={CancelledBy}",
                jobQueueId, cancelledBy);
        }

        // ── Reference checks ─────────────────────────────────────────────────────

        public async Task<bool> FpsYearExistsAsync(int fpsYear, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM fps.tblyearmaster WHERE fpsyear = @fpsyear AND active = true;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);
            var result = await cmd.ExecuteScalarAsync(ct);
            return result is not null;
        }

        public async Task<IReadOnlySet<string>> GetExistingTestCodesAsync(
            IEnumerable<string> testCodes, int fpsYear, CancellationToken ct = default)
        {
            var codeList = testCodes.ToList();
            if (codeList.Count == 0)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT itemcode FROM fps.testorproduct
                WHERE fpsyear = @fpsyear AND itemcode = ANY(@codes);";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);
            cmd.Parameters.Add(new NpgsqlParameter("codes", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text)
            {
                Value = codeList.ToArray()
            });

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                result.Add(reader.GetString(0));

            return result;
        }

        public async Task<IReadOnlySet<(string TestCode, string Buyer)>> GetExistingAgrupKeysAsync(
            IEnumerable<(string TestCode, string Buyer)> keys, int fpsYear, CancellationToken ct = default)
        {
            var keyList = keys.ToList();
            if (keyList.Count == 0)
                return new HashSet<(string, string)>();

            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();

            // Use unnest to match against a set of (testcode, buyer) tuples
            cmd.CommandText = @"
                SELECT t.testcode, t.buyer
                FROM fps.tlkptestreqmt t
                JOIN unnest(@testcodes::text[], @buyers::text[]) AS v(tc, bu)
                  ON t.testcode = v.tc AND t.buyer = v.bu
                WHERE t.fpsyear = @fpsyear;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);
            cmd.Parameters.Add(new NpgsqlParameter("testcodes", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text)
            {
                Value = keyList.Select(k => k.TestCode).ToArray()
            });
            cmd.Parameters.Add(new NpgsqlParameter("buyers", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text)
            {
                Value = keyList.Select(k => k.Buyer).ToArray()
            });

            var result = new HashSet<(string, string)>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                result.Add((reader.GetString(0), reader.GetString(1)));

            return result;
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private static BulkRatesQueueEntry ReadQueueEntry(NpgsqlDataReader reader) =>
            new()
            {
                JobQueueId       = reader.GetGuid(0),
                JobId            = reader.GetInt32(1),
                JobName          = reader.GetString(2),
                StatusId         = reader.GetInt32(3),
                Status           = reader.GetString(4),
                JobExecutionId   = reader.GetGuid(5),
                RequestedBy      = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                RequestedAtUtc   = reader.IsDBNull(7) ? default : reader.GetDateTime(7),
                FpsYear          = reader.IsDBNull(8) ? 0 : reader.GetInt32(8),
                ConfigurationJson= reader.IsDBNull(9) ? null : reader.GetString(9),
                ApprovedBy       = reader.IsDBNull(10) ? null : reader.GetString(10),
                ApprovedAtUtc    = reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                RejectedBy       = reader.IsDBNull(12) ? null : reader.GetString(12),
                RejectedAtUtc    = reader.IsDBNull(13) ? null : reader.GetDateTime(13),
                RejectionReason  = reader.IsDBNull(14) ? null : reader.GetString(14),
                CancelledBy      = reader.IsDBNull(15) ? null : reader.GetString(15),
                CancelledAtUtc   = reader.IsDBNull(16) ? null : reader.GetDateTime(16),
                CancellationReason = reader.IsDBNull(17) ? null : reader.GetString(17),
                TriggeredBy      = reader.IsDBNull(18) ? null : reader.GetString(18),
                TriggeredAtUtc   = reader.IsDBNull(19) ? null : reader.GetDateTime(19),
                StartDateTime    = reader.IsDBNull(20) ? null : reader.GetDateTime(20),
                EndDateTime      = reader.IsDBNull(21) ? null : reader.GetDateTime(21),
                FailureReason    = reader.IsDBNull(22) ? null : reader.GetString(22)
            };

        private static async Task DeleteFromAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx,
            string qualifiedTable, Guid jobQueueId, CancellationToken ct)
        {
            await using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            // Table name is hardcoded in callers — no user input reaches here
            cmd.CommandText = $"DELETE FROM {qualifiedTable} WHERE jobqueueid = @jqid;";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
