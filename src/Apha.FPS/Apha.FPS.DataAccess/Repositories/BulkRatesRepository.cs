using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
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

            return await GetRequestAsync(jobExecutionId, ct)
                ?? throw new InvalidOperationException($"Row just inserted (jobExecutionId={jobExecutionId}) could not be read back.");
        }

        public async Task<BulkRatesQueueEntry?> GetRequestAsync(Guid jobExecutionId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT q.jobqueueid, q.jobid, m.jobname, q.statusid, s.status,
                       q.jobexecutionid, q.requestedby, q.requested_at_utc, q.fpsyear,
                       q.upload_filename, q.upload_checksum_sha256, q.upload_version,
                       q.upload_validated_at_utc, q.upload_row_counts_json,
                       q.approved_by, q.approved_at_utc,
                       q.rejected_by, q.rejected_at_utc, q.rejection_reason,
                       q.cancelled_by, q.cancelled_at_utc, q.cancellation_reason,
                       q.triggered_by, q.triggered_at_utc,
                       q.startdatetime, q.enddatetime, q.errormessage,
                       q.active_download_version
                FROM fps.job_queue q
                JOIN fps.job_master m ON m.jobid = q.jobid
                JOIN fps.job_status s ON s.statusid = q.statusid AND s.jobid = q.jobid
                WHERE q.jobexecutionid = @jobexecutionid;";
            cmd.Parameters.AddWithValue("jobexecutionid", jobExecutionId);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return null;

            return ReadQueueEntry(reader);
        }

        // Whitelist mapping from client-facing sort keys to literal SQL column expressions —
        // sortBy is user input (via the DataGrid's sortable-header clicks) and must never be
        // interpolated directly into SQL text.
        private static readonly Dictionary<string, string> SortColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            ["jobname"] = "m.jobname",
            ["fpsyear"] = "q.fpsyear",
            ["status"] = "s.status",
            ["requestedby"] = "q.requestedby",
            ["requestedatutc"] = "q.requested_at_utc"
        };

        public async Task<PagedData<BulkRatesQueueEntry>> GetRequestsAsync(
            string? jobName, int? fpsYear, string? status,
            int page, int pageSize, string? sortBy, bool descending,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);

            var where = new StringBuilder(" WHERE 1=1");
            if (jobName != null) where.Append(" AND m.jobname = @jobname");
            if (fpsYear.HasValue) where.Append(" AND q.fpsyear = @fpsyear");
            if (status != null) where.Append(" AND s.status = @status");

            void AddFilterParameters(NpgsqlCommand filterCmd)
            {
                if (jobName != null) filterCmd.Parameters.AddWithValue("jobname", jobName);
                if (fpsYear.HasValue) filterCmd.Parameters.AddWithValue("fpsyear", fpsYear.Value);
                if (status != null) filterCmd.Parameters.AddWithValue("status", status);
            }

            int totalRecords;
            await using (var countCmd = conn.CreateCommand())
            {
                countCmd.CommandText = @"
                    SELECT COUNT(*)
                    FROM fps.job_queue q
                    JOIN fps.job_master m ON m.jobid = q.jobid
                    JOIN fps.job_status s ON s.statusid = q.statusid AND s.jobid = q.jobid" + where;
                AddFilterParameters(countCmd);
                totalRecords = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct));
            }

            var sortColumn = sortBy != null && SortColumns.TryGetValue(sortBy, out var col)
                ? col
                : "q.requested_at_utc";
            var sortDirection = descending ? "DESC" : "ASC";

            var results = new List<BulkRatesQueueEntry>();
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT q.jobqueueid, q.jobid, m.jobname, q.statusid, s.status,
                           q.jobexecutionid, q.requestedby, q.requested_at_utc, q.fpsyear,
                           q.upload_filename, q.upload_checksum_sha256, q.upload_version,
                           q.upload_validated_at_utc, q.upload_row_counts_json,
                           q.approved_by, q.approved_at_utc,
                           q.rejected_by, q.rejected_at_utc, q.rejection_reason,
                           q.cancelled_by, q.cancelled_at_utc, q.cancellation_reason,
                           q.triggered_by, q.triggered_at_utc,
                           q.startdatetime, q.enddatetime, q.errormessage,
                           q.active_download_version
                    FROM fps.job_queue q
                    JOIN fps.job_master m ON m.jobid = q.jobid
                    JOIN fps.job_status s ON s.statusid = q.statusid AND s.jobid = q.jobid"
                    + where + $" ORDER BY {sortColumn} {sortDirection} LIMIT @pagesize OFFSET @offset;";
                AddFilterParameters(cmd);
                cmd.Parameters.AddWithValue("pagesize", pageSize);
                cmd.Parameters.AddWithValue("offset", (page - 1) * pageSize);

                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                    results.Add(ReadQueueEntry(reader));
            }

            return new PagedData<BulkRatesQueueEntry>(results, new PaginationData
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalRecords / (double)pageSize) : 0,
                TotalRecords = totalRecords
            });
        }

        public async Task<BulkRatesQueueEntry?> GetActiveRequestAsync(string jobName, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT q.jobqueueid, q.jobid, m.jobname, q.statusid, s.status,
                       q.jobexecutionid, q.requestedby, q.requested_at_utc, q.fpsyear,
                       q.upload_filename, q.upload_checksum_sha256, q.upload_version,
                       q.upload_validated_at_utc, q.upload_row_counts_json,
                       q.approved_by, q.approved_at_utc,
                       q.rejected_by, q.rejected_at_utc, q.rejection_reason,
                       q.cancelled_by, q.cancelled_at_utc, q.cancellation_reason,
                       q.triggered_by, q.triggered_at_utc,
                       q.startdatetime, q.enddatetime, q.errormessage,
                       q.active_download_version
                FROM fps.job_queue q
                JOIN fps.job_master m ON m.jobid = q.jobid
                JOIN fps.job_status s ON s.statusid = q.statusid AND s.jobid = q.jobid
                WHERE m.jobname = @jobname
                  AND s.status IN ('Initiated','ReleasedForApproval','Approved','Running')
                ORDER BY q.requested_at_utc DESC
                LIMIT 1;";
            cmd.Parameters.AddWithValue("jobname", jobName);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
                return null;

            return ReadQueueEntry(reader);
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

        // ── Upload metadata ──────────────────────────────────────────────────────

        public async Task UpdateUploadMetadataAsync(
            Guid jobQueueId, string filename, string checksumSha256, int uploadVersion,
            DateTime validatedAtUtc, string rowCountsJson, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE fps.job_queue
                SET upload_filename        = @upload_filename,
                    upload_checksum_sha256 = @upload_checksum_sha256,
                    upload_version          = @upload_version,
                    upload_validated_at_utc = @upload_validated_at_utc,
                    upload_row_counts_json  = @upload_row_counts_json::jsonb,
                    updated_at              = NOW()
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("upload_filename",        filename);
            cmd.Parameters.AddWithValue("upload_checksum_sha256", checksumSha256);
            cmd.Parameters.AddWithValue("upload_version",         uploadVersion);
            cmd.Parameters.AddWithValue("upload_validated_at_utc", validatedAtUtc);
            cmd.Parameters.AddWithValue("upload_row_counts_json", rowCountsJson);
            cmd.Parameters.AddWithValue("jobqueueid",             jobQueueId);
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
                         norequired, datecreated, active, comments,
                         projectbuyercode, testbuyercode, testbuyerworkgroup)
                    VALUES
                        (@jobqueueid, @testcode, @buyer, @agrup, @agrupnew, @change,
                         @norequired, @datecreated, @active, @comments,
                         @projectbuyercode, @testbuyercode, @testbuyerworkgroup);";
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
                ins.Parameters.AddWithValue("projectbuyercode",   (object?)row.ProjectBuyerCode ?? DBNull.Value);
                ins.Parameters.AddWithValue("testbuyercode",      (object?)row.TestBuyerCode ?? DBNull.Value);
                ins.Parameters.AddWithValue("testbuyerworkgroup", (object?)row.TestBuyerWorkGroup ?? DBNull.Value);
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
                       itemdescription, shortdescription, owner, comments,
                       calculated_action, effective_new_rate::numeric, source_current_rate::numeric,
                       validation_version
                FROM fps.tblstagingtestorproduct
                WHERE jobqueueid = @jobqueueid
                ORDER BY testcode;";
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
                    Comments         = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CalculatedAction  = reader.IsDBNull(10) ? null : reader.GetString(10),
                    EffectiveNewRate  = reader.IsDBNull(11) ? null : reader.GetDecimal(11),
                    SourceCurrentRate = reader.IsDBNull(12) ? null : reader.GetDecimal(12),
                    ValidationVersion = reader.IsDBNull(13) ? null : reader.GetInt32(13)
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
                       norequired, datecreated, active, comments,
                       projectbuyercode, testbuyercode, testbuyerworkgroup,
                       calculated_action, effective_new_rate::numeric, source_current_rate::numeric,
                       validation_version
                FROM fps.tblstagingtlkptestreqmt
                WHERE jobqueueid = @jobqueueid
                ORDER BY testcode, buyer;";
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
                    Comments    = reader.IsDBNull(9) ? null : reader.GetString(9),
                    ProjectBuyerCode   = reader.IsDBNull(10) ? null : reader.GetString(10),
                    TestBuyerCode      = reader.IsDBNull(11) ? null : reader.GetString(11),
                    TestBuyerWorkGroup = reader.IsDBNull(12) ? null : reader.GetString(12),
                    CalculatedAction   = reader.IsDBNull(13) ? null : reader.GetString(13),
                    EffectiveNewRate   = reader.IsDBNull(14) ? null : reader.GetDecimal(14),
                    SourceCurrentRate  = reader.IsDBNull(15) ? null : reader.GetDecimal(15),
                    ValidationVersion  = reader.IsDBNull(16) ? null : reader.GetInt32(16)
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
                        (jobqueueid, uploadversion, sourcerownumber, fieldname,
                         validationcode, severity, validationmessage,
                         sheetname, testcode, buyer, currentvalue, expectedvalue, is_request_level)
                    VALUES
                        (@jobqueueid, @uploadversion, @sourcerownumber, @fieldname,
                         @validationcode, @severity, @validationmessage,
                         @sheetname, @testcode, @buyer, @currentvalue, @expectedvalue, @isrequestlevel);";
                ins.Parameters.AddWithValue("jobqueueid",       jobQueueId);
                ins.Parameters.AddWithValue("uploadversion",    err.UploadVersion);
                ins.Parameters.AddWithValue("sourcerownumber",  err.SourceRowNumber);
                ins.Parameters.AddWithValue("fieldname",        (object?)err.FieldName ?? DBNull.Value);
                ins.Parameters.AddWithValue("validationcode",   (object?)err.ValidationCode ?? DBNull.Value);
                ins.Parameters.AddWithValue("severity",         err.Severity);
                ins.Parameters.AddWithValue("validationmessage",err.ValidationMessage);
                ins.Parameters.AddWithValue("sheetname",        (object?)err.SheetName ?? DBNull.Value);
                ins.Parameters.AddWithValue("testcode",         (object?)err.TestCode ?? DBNull.Value);
                ins.Parameters.AddWithValue("buyer",            (object?)err.Buyer ?? DBNull.Value);
                ins.Parameters.AddWithValue("currentvalue",     (object?)err.CurrentValue ?? DBNull.Value);
                ins.Parameters.AddWithValue("expectedvalue",    (object?)err.ExpectedValue ?? DBNull.Value);
                ins.Parameters.AddWithValue("isrequestlevel",   err.IsRequestLevel);
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
                SELECT id, jobqueueid, uploadversion, sourcerownumber, fieldname,
                       validationcode, severity, validationmessage,
                       sheetname, testcode, buyer, currentvalue, expectedvalue, is_request_level
                FROM fps.staging_validation_error
                WHERE jobqueueid = @jobqueueid
                ORDER BY sourcerownumber, id;";
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
                    ValidationMessage = reader.GetString(7),
                    SheetName         = reader.IsDBNull(8) ? null : reader.GetString(8),
                    TestCode          = reader.IsDBNull(9) ? null : reader.GetString(9),
                    Buyer             = reader.IsDBNull(10) ? null : reader.GetString(10),
                    CurrentValue      = reader.IsDBNull(11) ? null : reader.GetString(11),
                    ExpectedValue     = reader.IsDBNull(12) ? null : reader.GetString(12),
                    IsRequestLevel    = reader.GetBoolean(13)
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

        public async Task<IReadOnlySet<string>> GetExistingProjectCodesAsync(
            IEnumerable<string> parentProjectCodes, int fpsYear, CancellationToken ct = default)
        {
            var codeList = parentProjectCodes.ToList();
            if (codeList.Count == 0)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT parentproject FROM fps.tlkpproject
                WHERE fpsyear = @fpsyear AND parentproject = ANY(@codes);";
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

        public async Task<IReadOnlySet<(string TestCode, string WorkGroup)>> GetExistingCapabilityPairsAsync(
            IEnumerable<(string TestCode, string WorkGroup)> pairs, int fpsYear, CancellationToken ct = default)
        {
            var pairList = pairs.ToList();
            if (pairList.Count == 0)
                return new HashSet<(string, string)>();

            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();

            // Two real columns (testcode, workgroup) — never a concatenated string.
            cmd.CommandText = @"
                SELECT c.testcode, c.workgroup
                FROM fps.tlkptestcapability c
                JOIN unnest(@testcodes::text[], @workgroups::text[]) AS v(tc, wg)
                  ON c.testcode = v.tc AND c.workgroup = v.wg
                WHERE c.fpsyear = @fpsyear;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);
            cmd.Parameters.Add(new NpgsqlParameter("testcodes", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text)
            {
                Value = pairList.Select(p => p.TestCode).ToArray()
            });
            cmd.Parameters.Add(new NpgsqlParameter("workgroups", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text)
            {
                Value = pairList.Select(p => p.WorkGroup).ToArray()
            });

            var result = new HashSet<(string, string)>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                result.Add((reader.GetString(0), reader.GetString(1)));

            return result;
        }

        // ── Download snapshot ─────────────────────────────────────────────────────

        public async Task<int> GetNextDownloadVersionAsync(Guid jobQueueId, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT COALESCE(MAX(download_version), 0) + 1
                FROM fps.bulk_rates_download
                WHERE jobqueueid = @jobqueueid;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            return (int)(await cmd.ExecuteScalarAsync(ct))!;
        }

        public async Task CreateDownloadSnapshotAsync(
            Guid jobQueueId, int downloadVersion,
            IReadOnlyList<FecStagingRow> fecRows, IReadOnlyList<AgrupStagingRow> agrupRows,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var ins = conn.CreateCommand())
            {
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_download (jobqueueid, download_version, status)
                    VALUES (@jobqueueid, @downloadversion, 'Generating');";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                await ins.ExecuteNonQueryAsync(ct);
            }

            // source_rate carries defraunitprice for FEC rows, unitprice for AGRUP rows —
            // the single "current rate" value ValidationContext.FrozenSnapshot
            // reads (reconciliation §2.6). unitpricevla/norequired/datecreated/
            // active/itemdescription/shortdescription/owner exist only to let the
            // workbook be regenerated from the snapshot without a second live query.
            foreach (var row in fecRows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_downloaded_key
                        (jobqueueid, download_version, sheetname, testcode, source_rate,
                         unitpricevla, itemdescription, shortdescription, owner)
                    VALUES
                        (@jobqueueid, @downloadversion, 'FEC', @testcode, @sourcerate,
                         @unitpricevla, @itemdescription, @shortdescription, @owner);";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                ins.Parameters.AddWithValue("testcode", row.TestCode);
                ins.Parameters.AddWithValue("sourcerate", (object?)row.DefraUnitPrice ?? DBNull.Value);
                ins.Parameters.AddWithValue("unitpricevla", (object?)row.UnitPriceVla ?? DBNull.Value);
                ins.Parameters.AddWithValue("itemdescription", (object?)row.ItemDescription ?? DBNull.Value);
                ins.Parameters.AddWithValue("shortdescription", (object?)row.ShortDescription ?? DBNull.Value);
                ins.Parameters.AddWithValue("owner", (object?)row.Owner ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            foreach (var row in agrupRows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_downloaded_key
                        (jobqueueid, download_version, sheetname, testcode, buyer, source_rate,
                         norequired, datecreated, active, projectbuyercode, testbuyercode)
                    VALUES
                        (@jobqueueid, @downloadversion, 'AGRUP', @testcode, @buyer, @sourcerate,
                         @norequired, @datecreated, @active, @projectbuyercode, @testbuyercode);";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                ins.Parameters.AddWithValue("testcode", row.TestCode);
                ins.Parameters.AddWithValue("buyer", row.Buyer);
                ins.Parameters.AddWithValue("sourcerate", (object?)row.Agrup ?? DBNull.Value);
                ins.Parameters.AddWithValue("norequired", (object?)row.NoRequired ?? DBNull.Value);
                ins.Parameters.AddWithValue("datecreated", (object?)row.DateCreated ?? DBNull.Value);
                ins.Parameters.AddWithValue("active", (object?)row.Active ?? DBNull.Value);
                ins.Parameters.AddWithValue("projectbuyercode", (object?)row.ProjectBuyerCode ?? DBNull.Value);
                ins.Parameters.AddWithValue("testbuyercode", (object?)row.TestBuyerCode ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "CreateDownloadSnapshot | JobQueueId={JobQueueId} | DownloadVersion={DownloadVersion} | FecRows={FecRows} | AgrupRows={AgrupRows}",
                jobQueueId, downloadVersion, fecRows.Count, agrupRows.Count);
        }

        public async Task MarkDownloadReadyAsync(Guid jobQueueId, int downloadVersion, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var upd = conn.CreateCommand())
            {
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.bulk_rates_download
                    SET status = 'Ready', ready_at_utc = now()
                    WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion;";
                upd.Parameters.AddWithValue("jobqueueid", jobQueueId);
                upd.Parameters.AddWithValue("downloadversion", downloadVersion);
                await upd.ExecuteNonQueryAsync(ct);
            }

            // Guard against an older Generating download
            // finishing after a newer one has already activated — the WHERE clause is the
            // concurrency-safety mechanism ensuring active_download_version can never be
            // overwritten back to itself. A late-finishing older
            // version still marks its own header row Ready above (an accurate historical
            // record of that version), it just never regresses the active pointer. Shared by
            // FEC/AGRUP and Staff/Animal alike — all three call this same method.
            await using (var upd = conn.CreateCommand())
            {
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.job_queue
                    SET active_download_version = @downloadversion
                    WHERE jobqueueid = @jobqueueid
                      AND (active_download_version IS NULL OR active_download_version < @downloadversion);";
                upd.Parameters.AddWithValue("jobqueueid", jobQueueId);
                upd.Parameters.AddWithValue("downloadversion", downloadVersion);
                await upd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
        }

        public async Task MarkDownloadFailedAsync(Guid jobQueueId, int downloadVersion, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE fps.bulk_rates_download
                SET status = 'Failed'
                WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion AND status = 'Generating';";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("downloadversion", downloadVersion);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<IReadOnlyList<FecStagingRow>> GetFecSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT testcode, unitpricevla, source_rate, itemdescription, shortdescription, owner
                FROM fps.bulk_rates_downloaded_key
                WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion AND sheetname = 'FEC'
                ORDER BY id;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("downloadversion", downloadVersion);

            var result = new List<FecStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new FecStagingRow
                {
                    JobQueueId = jobQueueId,
                    TestCode = reader.GetString(0),
                    UnitPriceVla = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
                    DefraUnitPrice = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    ItemDescription = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ShortDescription = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Owner = reader.IsDBNull(5) ? null : reader.GetString(5),
                });
            }
            return result;
        }

        public async Task<IReadOnlyList<AgrupStagingRow>> GetAgrupSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT testcode, buyer, source_rate, norequired, datecreated, active,
                       projectbuyercode, testbuyercode
                FROM fps.bulk_rates_downloaded_key
                WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion AND sheetname = 'AGRUP'
                ORDER BY id;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("downloadversion", downloadVersion);

            var result = new List<AgrupStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new AgrupStagingRow
                {
                    JobQueueId = jobQueueId,
                    TestCode = reader.GetString(0),
                    Buyer = reader.GetString(1),
                    Agrup = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    NoRequired = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                    DateCreated = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Active = reader.IsDBNull(5) ? null : reader.GetInt16(5),
                    ProjectBuyerCode = reader.IsDBNull(6) ? null : reader.GetString(6),
                    TestBuyerCode = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }
            return result;
        }

        // ── Download snapshot — Staff/Animal ──────────────────────────────────────

        public async Task CreateStaffDownloadSnapshotAsync(
            Guid jobQueueId, int downloadVersion,
            IReadOnlyList<StaffStagingRow> rows,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var ins = conn.CreateCommand())
            {
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_download (jobqueueid, download_version, status)
                    VALUES (@jobqueueid, @downloadversion, 'Generating');";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                await ins.ExecuteNonQueryAsync(ct);
            }

            foreach (var row in rows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_staff_downloaded_key
                        (jobqueueid, download_version, pcgrade, source_payrate, source_npr, source_ohr)
                    VALUES
                        (@jobqueueid, @downloadversion, @pcgrade, @sourcepayrate, @sourcenpr, @sourceohr);";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                ins.Parameters.AddWithValue("pcgrade", row.PcGrade);
                ins.Parameters.AddWithValue("sourcepayrate", (object?)row.PayRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("sourcenpr", (object?)row.Npr ?? DBNull.Value);
                ins.Parameters.AddWithValue("sourceohr", (object?)row.Ohr ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "CreateStaffDownloadSnapshot | JobQueueId={JobQueueId} | DownloadVersion={DownloadVersion} | StaffRows={StaffRows}",
                jobQueueId, downloadVersion, rows.Count);
        }

        public async Task<IReadOnlyList<StaffStagingRow>> GetStaffSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT pcgrade, source_payrate, source_npr, source_ohr
                FROM fps.bulk_rates_staff_downloaded_key
                WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion
                ORDER BY id;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("downloadversion", downloadVersion);

            var result = new List<StaffStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new StaffStagingRow
                {
                    JobQueueId = jobQueueId,
                    PcGrade    = reader.GetString(0),
                    PayRate    = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
                    Npr        = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    Ohr        = reader.IsDBNull(3) ? null : reader.GetDecimal(3)
                });
            }
            return result;
        }

        public async Task CreateAnimalDownloadSnapshotAsync(
            Guid jobQueueId, int downloadVersion,
            IReadOnlyList<AnimalStagingRow> rows,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            await using (var ins = conn.CreateCommand())
            {
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_download (jobqueueid, download_version, status)
                    VALUES (@jobqueueid, @downloadversion, 'Generating');";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                await ins.ExecuteNonQueryAsync(ct);
            }

            foreach (var row in rows)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;
                ins.CommandText = @"
                    INSERT INTO fps.bulk_rates_animal_downloaded_key
                        (jobqueueid, download_version, animaltype, source_dailyrate,
                         source_defradailyrate, source_planbyweek, source_species, source_securitylevel)
                    VALUES
                        (@jobqueueid, @downloadversion, @animaltype, @sourcedailyrate,
                         @sourcedefradailyrate, @sourceplanbyweek, @sourcespecies, @sourcesecuritylevel);";
                ins.Parameters.AddWithValue("jobqueueid", jobQueueId);
                ins.Parameters.AddWithValue("downloadversion", downloadVersion);
                ins.Parameters.AddWithValue("animaltype", row.AnimalType);
                ins.Parameters.AddWithValue("sourcedailyrate", (object?)row.DailyRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("sourcedefradailyrate", (object?)row.DefraDailyRate ?? DBNull.Value);
                ins.Parameters.AddWithValue("sourceplanbyweek", (object?)row.PlanByWeek ?? DBNull.Value);
                ins.Parameters.AddWithValue("sourcespecies", (object?)row.Species ?? DBNull.Value);
                ins.Parameters.AddWithValue("sourcesecuritylevel", (object?)row.SecurityLevel ?? DBNull.Value);
                await ins.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "CreateAnimalDownloadSnapshot | JobQueueId={JobQueueId} | DownloadVersion={DownloadVersion} | AnimalRows={AnimalRows}",
                jobQueueId, downloadVersion, rows.Count);
        }

        public async Task<IReadOnlyList<AnimalStagingRow>> GetAnimalSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT animaltype, source_species, source_securitylevel, source_dailyrate,
                       source_defradailyrate, source_planbyweek
                FROM fps.bulk_rates_animal_downloaded_key
                WHERE jobqueueid = @jobqueueid AND download_version = @downloadversion
                ORDER BY id;";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("downloadversion", downloadVersion);

            var result = new List<AnimalStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Add(new AnimalStagingRow
                {
                    JobQueueId     = jobQueueId,
                    AnimalType     = reader.GetString(0),
                    Species        = reader.IsDBNull(1) ? null : reader.GetString(1),
                    SecurityLevel  = reader.IsDBNull(2) ? null : reader.GetString(2),
                    DailyRate      = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    DefraDailyRate = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    PlanByWeek     = reader.IsDBNull(5) ? null : reader.GetBoolean(5)
                });
            }
            return result;
        }

        // ── Export: live table reads ──────────────────────────────────────────────

        public async Task<IReadOnlyList<FecStagingRow>> GetFecRowsForExportAsync(
            int fpsYear, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            // fecnewrate is pre-populated with the current Defra Unit Price (not left NULL) so a
            // downloaded workbook shows "FEC New" already carrying the current rate for every
            // row — an untouched row then classifies as No Change rather than the existing-row
            // blank/zero rule (Zero-Rate Withdrawal), and the Change formula the caller writes
            // over this data starts from a real number instead of blank.
            cmd.CommandText = @"
                SELECT itemcode, unitpricevla::numeric, defraunitprice::numeric,
                       defraunitprice::numeric AS fecnewrate, NULL::numeric AS change,
                       itemdescription, shortdescription, owner, NULL::text AS comments
                FROM fps.testorproduct
                WHERE fpsyear = @fpsyear
                ORDER BY itemcode;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);

            var rows = new List<FecStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new FecStagingRow
                {
                    JobQueueId       = Guid.Empty,
                    TestCode         = reader.GetString(0),
                    UnitPriceVla     = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
                    DefraUnitPrice   = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    FecNewRate       = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Change           = null,
                    ItemDescription  = reader.IsDBNull(5) ? null : reader.GetString(5),
                    ShortDescription = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Owner            = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Comments         = reader.IsDBNull(8) ? null : reader.GetString(8)
                });
            }
            return rows;
        }

        public async Task<IReadOnlyList<AgrupStagingRow>> GetAgrupRowsForExportAsync(
            int fpsYear, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            // agrupnew is pre-populated with the current unit price (not left NULL) — same
            // rationale as fecnewrate above.
            cmd.CommandText = @"
                SELECT testcode, buyer,
                       unitprice::numeric AS agrup, unitprice::numeric AS agrupnew, NULL::numeric AS change,
                       norequired, datecreated, active, NULL::text AS comments,
                       projectbuyercode, testbuyercode
                FROM fps.tlkptestreqmt
                WHERE fpsyear = @fpsyear
                ORDER BY testcode, buyer;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);

            var rows = new List<AgrupStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new AgrupStagingRow
                {
                    JobQueueId  = Guid.Empty,
                    TestCode    = reader.GetString(0),
                    Buyer       = reader.GetString(1),
                    Agrup       = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    AgrupNew    = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    Change      = null,
                    NoRequired  = reader.IsDBNull(5) ? null : reader.GetDouble(5),
                    DateCreated = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                    Active      = reader.IsDBNull(7) ? null : reader.GetInt16(7),
                    Comments    = reader.IsDBNull(8) ? null : reader.GetString(8),
                    ProjectBuyerCode = reader.IsDBNull(9) ? null : reader.GetString(9),
                    TestBuyerCode    = reader.IsDBNull(10) ? null : reader.GetString(10)
                });
            }
            return rows;
        }

        public async Task<IReadOnlyList<StaffStagingRow>> GetStaffRowsForExportAsync(
            int fpsYear, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT pcgrade, payrate::numeric, npr::numeric, ohr::numeric
                FROM fps.profitcentregrade
                WHERE fpsyear = @fpsyear
                ORDER BY pcgrade;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);

            var rows = new List<StaffStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new StaffStagingRow
                {
                    JobQueueId = Guid.Empty,
                    PcGrade    = reader.GetString(0),
                    PayRate    = reader.IsDBNull(1) ? null : reader.GetDecimal(1),
                    Npr        = reader.IsDBNull(2) ? null : reader.GetDecimal(2),
                    Ohr        = reader.IsDBNull(3) ? null : reader.GetDecimal(3)
                });
            }
            return rows;
        }

        public async Task<IReadOnlyList<AnimalStagingRow>> GetAnimalRowsForExportAsync(
            int fpsYear, CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT animaltype, species, security_level,
                       dailyrate::numeric, defradailyrate::numeric, planbyweek
                FROM fps.tblanimals
                WHERE fpsyear = @fpsyear
                ORDER BY animaltype;";
            cmd.Parameters.AddWithValue("fpsyear", fpsYear);

            var rows = new List<AnimalStagingRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                rows.Add(new AnimalStagingRow
                {
                    JobQueueId     = Guid.Empty,
                    AnimalType     = reader.GetString(0),
                    Species        = reader.IsDBNull(1) ? null : reader.GetString(1),
                    SecurityLevel  = reader.IsDBNull(2) ? null : reader.GetString(2),
                    DailyRate      = reader.IsDBNull(3) ? null : reader.GetDecimal(3),
                    DefraDailyRate = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                    PlanByWeek     = reader.IsDBNull(5) ? null : reader.GetBoolean(5)
                });
            }
            return rows;
        }

        // ── Freeze reviewed classification onto staging ───────────────────────────

        public async Task FreezeStagingCalculatedActionsAsync(
            Guid jobQueueId, int validationVersion,
            IReadOnlyList<BulkRatesFreezeEntry> fecFreezes,
            IReadOnlyList<BulkRatesFreezeEntry> agrupFreezes,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            foreach (var entry in fecFreezes)
            {
                await using var upd = conn.CreateCommand();
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.tblstagingtestorproduct
                    SET calculated_action    = @calculated_action,
                        effective_new_rate   = @effective_new_rate,
                        source_current_rate  = @source_current_rate,
                        validation_version   = @validation_version
                    WHERE jobqueueid = @jobqueueid AND testcode = @testcode;";
                upd.Parameters.AddWithValue("jobqueueid",          jobQueueId);
                upd.Parameters.AddWithValue("testcode",            entry.TestCode);
                upd.Parameters.AddWithValue("calculated_action",   entry.CalculatedAction);
                upd.Parameters.AddWithValue("effective_new_rate",  (object?)entry.EffectiveNewRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("source_current_rate", (object?)entry.SourceCurrentRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("validation_version",  validationVersion);
                await upd.ExecuteNonQueryAsync(ct);
            }

            foreach (var entry in agrupFreezes)
            {
                await using var upd = conn.CreateCommand();
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.tblstagingtlkptestreqmt
                    SET calculated_action    = @calculated_action,
                        effective_new_rate   = @effective_new_rate,
                        source_current_rate  = @source_current_rate,
                        validation_version   = @validation_version
                    WHERE jobqueueid = @jobqueueid AND testcode = @testcode AND buyer = @buyer;";
                upd.Parameters.AddWithValue("jobqueueid",          jobQueueId);
                upd.Parameters.AddWithValue("testcode",            entry.TestCode);
                upd.Parameters.AddWithValue("buyer",               (object?)entry.Buyer ?? DBNull.Value);
                upd.Parameters.AddWithValue("calculated_action",   entry.CalculatedAction);
                upd.Parameters.AddWithValue("effective_new_rate",  (object?)entry.EffectiveNewRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("source_current_rate", (object?)entry.SourceCurrentRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("validation_version",  validationVersion);
                await upd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "FreezeStagingCalculatedActions | JobQueueId={JobQueueId} | ValidationVersion={ValidationVersion} | FecRows={FecRows} | AgrupRows={AgrupRows}",
                jobQueueId, validationVersion, fecFreezes.Count, agrupFreezes.Count);
        }

        public async Task FreezeStaffStagingAsync(
            Guid jobQueueId, int validationVersion,
            IReadOnlyList<StaffFreezeEntry> freezes,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            foreach (var entry in freezes)
            {
                await using var upd = conn.CreateCommand();
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.tblstagingprofitcentregrade
                    SET source_payrate     = @source_payrate,
                        source_npr         = @source_npr,
                        source_ohr         = @source_ohr,
                        effective_payrate  = @effective_payrate,
                        effective_npr      = @effective_npr,
                        effective_ohr      = @effective_ohr,
                        calculated_action  = @calculated_action,
                        validation_version = @validation_version
                    WHERE jobqueueid = @jobqueueid AND pcgrade = @pcgrade;";
                upd.Parameters.AddWithValue("jobqueueid", jobQueueId);
                upd.Parameters.AddWithValue("pcgrade", entry.PcGrade);
                upd.Parameters.AddWithValue("source_payrate", (object?)entry.SourcePayRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("source_npr", (object?)entry.SourceNpr ?? DBNull.Value);
                upd.Parameters.AddWithValue("source_ohr", (object?)entry.SourceOhr ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_payrate", (object?)entry.EffectivePayRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_npr", (object?)entry.EffectiveNpr ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_ohr", (object?)entry.EffectiveOhr ?? DBNull.Value);
                upd.Parameters.AddWithValue("calculated_action", entry.CalculatedAction);
                upd.Parameters.AddWithValue("validation_version", validationVersion);
                await upd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "FreezeStaffStaging | JobQueueId={JobQueueId} | ValidationVersion={ValidationVersion} | StaffRows={StaffRows}",
                jobQueueId, validationVersion, freezes.Count);
        }

        public async Task FreezeAnimalStagingAsync(
            Guid jobQueueId, int validationVersion,
            IReadOnlyList<AnimalFreezeEntry> freezes,
            CancellationToken ct = default)
        {
            var conn = await OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            foreach (var entry in freezes)
            {
                await using var upd = conn.CreateCommand();
                upd.Transaction = tx;
                upd.CommandText = @"
                    UPDATE fps.tblstaginganimals
                    SET source_dailyrate         = @source_dailyrate,
                        source_defradailyrate    = @source_defradailyrate,
                        source_planbyweek        = @source_planbyweek,
                        source_species           = @source_species,
                        source_securitylevel     = @source_securitylevel,
                        effective_dailyrate      = @effective_dailyrate,
                        effective_defradailyrate = @effective_defradailyrate,
                        effective_planbyweek     = @effective_planbyweek,
                        effective_species        = @effective_species,
                        effective_securitylevel  = @effective_securitylevel,
                        calculated_action        = @calculated_action,
                        validation_version       = @validation_version
                    WHERE jobqueueid = @jobqueueid AND animaltype = @animaltype;";
                upd.Parameters.AddWithValue("jobqueueid", jobQueueId);
                upd.Parameters.AddWithValue("animaltype", entry.AnimalType);
                upd.Parameters.AddWithValue("source_dailyrate", (object?)entry.SourceDailyRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("source_defradailyrate", (object?)entry.SourceDefraDailyRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("source_planbyweek", (object?)entry.SourcePlanByWeek ?? DBNull.Value);
                upd.Parameters.AddWithValue("source_species", (object?)entry.SourceSpecies ?? DBNull.Value);
                upd.Parameters.AddWithValue("source_securitylevel", (object?)entry.SourceSecurityLevel ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_dailyrate", (object?)entry.EffectiveDailyRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_defradailyrate", (object?)entry.EffectiveDefraDailyRate ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_planbyweek", (object?)entry.EffectivePlanByWeek ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_species", (object?)entry.EffectiveSpecies ?? DBNull.Value);
                upd.Parameters.AddWithValue("effective_securitylevel", (object?)entry.EffectiveSecurityLevel ?? DBNull.Value);
                upd.Parameters.AddWithValue("calculated_action", entry.CalculatedAction);
                upd.Parameters.AddWithValue("validation_version", validationVersion);
                await upd.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "FreezeAnimalStaging | JobQueueId={JobQueueId} | ValidationVersion={ValidationVersion} | AnimalRows={AnimalRows}",
                jobQueueId, validationVersion, freezes.Count);
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
                UploadFilename       = reader.IsDBNull(9) ? null : reader.GetString(9),
                UploadChecksumSha256 = reader.IsDBNull(10) ? null : reader.GetString(10),
                UploadVersion        = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                UploadValidatedAtUtc = reader.IsDBNull(12) ? null : reader.GetDateTime(12),
                UploadRowCountsJson  = reader.IsDBNull(13) ? null : reader.GetString(13),
                ApprovedBy       = reader.IsDBNull(14) ? null : reader.GetString(14),
                ApprovedAtUtc    = reader.IsDBNull(15) ? null : reader.GetDateTime(15),
                RejectedBy       = reader.IsDBNull(16) ? null : reader.GetString(16),
                RejectedAtUtc    = reader.IsDBNull(17) ? null : reader.GetDateTime(17),
                RejectionReason  = reader.IsDBNull(18) ? null : reader.GetString(18),
                CancelledBy      = reader.IsDBNull(19) ? null : reader.GetString(19),
                CancelledAtUtc   = reader.IsDBNull(20) ? null : reader.GetDateTime(20),
                CancellationReason = reader.IsDBNull(21) ? null : reader.GetString(21),
                TriggeredBy      = reader.IsDBNull(22) ? null : reader.GetString(22),
                TriggeredAtUtc   = reader.IsDBNull(23) ? null : reader.GetDateTime(23),
                StartDateTime          = reader.IsDBNull(24) ? null : reader.GetDateTime(24),
                EndDateTime            = reader.IsDBNull(25) ? null : reader.GetDateTime(25),
                FailureReason          = reader.IsDBNull(26) ? null : reader.GetString(26),
                ActiveDownloadVersion  = reader.IsDBNull(27) ? null : reader.GetInt32(27)
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
