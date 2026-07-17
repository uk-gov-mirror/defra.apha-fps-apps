using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data;
using System.Data.Common;

namespace Apha.BatchJobs.Infrastructure.Repositories;

/// <summary>
/// Implementation of job execution repository using EF Core.
/// </summary>
public class JobExecutionRepository : IJobExecutionRepository
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<JobExecutionRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the JobExecutionRepository.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for structured execution record events.</param>
    public JobExecutionRepository(BatchJobsDbContext context, ILogger<JobExecutionRepository>? logger = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? NullLogger<JobExecutionRepository>.Instance;
    }

    /// <inheritdoc />
    public async Task<int> CreateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        var now = DateTime.UtcNow;
        _logger.LogInformation(
            "Create execution record requested | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | UserId={UserId} | Status={Status} | FpsYear={FpsYear}",
            record.JobName,
            record.JobExecutionId,
            record.JobQueueId,
            record.UserId,
            record.Status,
            record.FpsYear);

        var existingRow = await _context.TblJobQueue
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.JobExecutionId == record.JobExecutionId, cancellationToken);

        if (existingRow != null)
        {
            var expectedPickupStatus = IsApprovalBasedJob(record.JobName)
                ? JobStatus.Approved
                : JobStatus.Initiated;

            var expectedStatusId = await EnsureStatusAsync(existingRow.JobId, expectedPickupStatus.ToString(), cancellationToken);
            var runningStatusId = await EnsureStatusAsync(existingRow.JobId, JobStatus.Running.ToString(), cancellationToken);

            var updateRows = await _context.TblJobQueue
                .Where(q => q.JobExecutionId == record.JobExecutionId && q.StatusId == expectedStatusId)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(q => q.StatusId, _ => runningStatusId)
                    .SetProperty(q => q.StartDateTime, _ => record.StartedAt)
                    .SetProperty(q => q.RequestedBy, _ => record.UserId)
                    .SetProperty(q => q.UpdatedAt, _ => now)
                    .SetProperty(
                        q => q.FpsYear,
                        q => record.FpsYear.HasValue ? record.FpsYear.Value : q.FpsYear),
                    cancellationToken);

            if (updateRows == 0)
            {
                var currentStatus = await (
                    from q in _context.TblJobQueue
                    join s in _context.TblJobStatus on q.StatusId equals s.StatusId
                    where q.JobExecutionId == record.JobExecutionId
                    select s.Status)
                    .FirstOrDefaultAsync(cancellationToken);

                throw new InvalidOperationException(
                    $"Cannot transition JobExecutionId {record.JobExecutionId} to Running. " +
                    $"Expected status '{expectedPickupStatus}' but found '{currentStatus ?? "Unknown"}'.");
            }

            var claimedRow = await _context.TblJobQueue
                .AsNoTracking()
                .FirstAsync(q => q.JobExecutionId == record.JobExecutionId, cancellationToken);

            record.JobQueueId = claimedRow.JobQueueId;

            _context.TblJobQueueLog.Add(new TblJobQueueLog
            {
                JobQueueId = claimedRow.JobQueueId,
                StatusId = runningStatusId,
                PerformedBy = record.UserId,
                LogTime = now,
                Note = BuildStartTransitionNote(expectedPickupStatus)
            });

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[Worker → DB] ✓ {ExpectedPickupStatus} → Running transition complete | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | StartDateTime={StartDateTime}",
                expectedPickupStatus,
                record.JobName,
                record.JobExecutionId,
                record.JobQueueId,
                record.StartedAt);

            return 0;
        }

        _logger.LogError(
            "Execution start rejected: no pre-created Initiated row | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId}",
            record.JobName,
            record.JobExecutionId,
            record.JobQueueId);

        throw new InvalidOperationException(
            $"No pre-created Initiated record exists for JobExecutionId {record.JobExecutionId}. API must insert Initiated before worker execution starts.");
    }

    /// <inheritdoc />
    public async Task UpdateExecutionRecordAsync(JobExecutionRecord record, CancellationToken cancellationToken = default)
    {
        if (record == null)
            throw new ArgumentNullException(nameof(record));

        // The worker uses a shared DbContext across repositories. Do not clear ChangeTracker here to avoid nested transaction errors.

        _logger.LogInformation(
            "Update execution record requested | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | UserId={UserId} | Status={Status}",
            record.JobName,
            record.JobExecutionId,
            record.JobQueueId,
            record.UserId,
            record.Status);

        var queueRow = await _context.TblJobQueue
            .FirstOrDefaultAsync(q => q.JobQueueId == record.JobQueueId, cancellationToken);

        if (queueRow == null)
        {
            _logger.LogInformation(
                "Execution record not found for update | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | UserId={UserId}",
                record.JobName,
                record.JobExecutionId,
                record.JobQueueId,
                record.UserId);
            return;
        }

        var now = DateTime.UtcNow;
        var statusId = await EnsureStatusAsync(queueRow.JobId, record.Status.ToString(), cancellationToken);

        queueRow.StatusId = statusId;
        queueRow.RequestedBy = record.UserId;
        queueRow.EndDateTime = record.CompletedAt;
        queueRow.ErrorMessage = record.ErrorMessage;
        queueRow.UpdatedAt = now;

        _context.TblJobQueueLog.Add(new TblJobQueueLog
        {
            JobQueueId = record.JobQueueId,
            StatusId = statusId,
            PerformedBy = record.UserId,
            LogTime = now,
            Note = BuildStatusNote(record.Status)
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Execution record updated | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | UserId={UserId} | Status={Status}",
            record.JobName,
            record.JobExecutionId,
            record.JobQueueId,
            record.UserId,
            record.Status);
    }

    /// <inheritdoc />
    public async Task<JobExecutionRecord?> GetLastExecutionAsync(string jobName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        var last = await (
            from q in _context.TblJobQueue
            join m in _context.TblJobMaster on q.JobId equals m.JobId
            join s in _context.TblJobStatus on q.StatusId equals s.StatusId
            where m.JobName == jobName
            orderby q.StartDateTime descending
            select new
            {
                m.JobName,
                q.JobExecutionId,
                q.JobQueueId,
                q.RequestedBy,
                q.RequestedAtUtc,
                q.FpsYear,
                q.StartDateTime,
                q.EndDateTime,
                s.Status,
                q.ErrorMessage
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (last == null)
            return null;

        var parsedStatus = Enum.TryParse<JobStatus>(last.Status, true, out var status)
            ? status
            : JobStatus.Failed;

        return new JobExecutionRecord
        {
            ExecutionId = 0,
            JobName = last.JobName,
            JobExecutionId = last.JobExecutionId,
            JobQueueId = last.JobQueueId,
            UserId = last.RequestedBy,
            JobType = JobType.Unknown,
            RunMode = RunMode.Manual,
            Status = parsedStatus,
            RequestedAtUtc = last.RequestedAtUtc,
            FpsYear = last.FpsYear,
            StartedAt = last.StartDateTime ?? DateTime.UtcNow,
            CompletedAt = last.EndDateTime,
            DurationSeconds = last.EndDateTime.HasValue && last.StartDateTime.HasValue
                ? (int)(last.EndDateTime.Value - last.StartDateTime.Value).TotalSeconds
                : null,
            ErrorMessage = last.ErrorMessage,
            RetryAttempts = 0
        };
    }

    /// <inheritdoc />
    public async Task<JobExecutionRecord?> GetLastExecutionByFpsYearAsync(string jobName, int fpsYear, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        var last = await (
            from q in _context.TblJobQueue
            join m in _context.TblJobMaster on q.JobId equals m.JobId
            join s in _context.TblJobStatus on q.StatusId equals s.StatusId
            where m.JobName == jobName && q.FpsYear == fpsYear
            orderby q.StartDateTime descending
            select new
            {
                m.JobName,
                q.JobExecutionId,
                q.JobQueueId,
                q.RequestedBy,
                q.RequestedAtUtc,
                q.FpsYear,
                q.StartDateTime,
                q.EndDateTime,
                s.Status,
                q.ErrorMessage
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (last == null)
            return null;

        var parsedStatus = Enum.TryParse<JobStatus>(last.Status, true, out var status)
            ? status
            : JobStatus.Failed;

        return new JobExecutionRecord
        {
            ExecutionId = 0,
            JobName = last.JobName,
            JobExecutionId = last.JobExecutionId,
            JobQueueId = last.JobQueueId,
            UserId = last.RequestedBy,
            JobType = JobType.Unknown,
            RunMode = RunMode.Manual,
            Status = parsedStatus,
            RequestedAtUtc = last.RequestedAtUtc,
            FpsYear = last.FpsYear,
            StartedAt = last.StartDateTime ?? DateTime.UtcNow,
            CompletedAt = last.EndDateTime,
            DurationSeconds = last.EndDateTime.HasValue && last.StartDateTime.HasValue
                ? (int)(last.EndDateTime.Value - last.StartDateTime.Value).TotalSeconds
                : null,
            ErrorMessage = last.ErrorMessage,
            RetryAttempts = 0
        };
    }

    /// <inheritdoc />
    public async Task<JobExecutionRecord?> GetExecutionByJobExecutionIdAsync(Guid jobExecutionId, CancellationToken cancellationToken = default)
    {
        var execution = await (
            from q in _context.TblJobQueue
            join m in _context.TblJobMaster on q.JobId equals m.JobId
            join s in _context.TblJobStatus on q.StatusId equals s.StatusId
            where q.JobExecutionId == jobExecutionId
            orderby q.StartDateTime descending
            select new
            {
                m.JobName,
                q.JobExecutionId,
                q.JobQueueId,
                q.RequestedBy,
                q.RequestedAtUtc,
                q.FpsYear,
                q.StartDateTime,
                q.EndDateTime,
                s.Status,
                q.ErrorMessage
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (execution == null)
            return null;

        if (!Enum.TryParse<JobStatus>(execution.Status, true, out var status))
        {
            _logger.LogError(
                "Status parsing failed for JobExecutionId | JobExecutionId={JobExecutionId} | StatusFromDb={StatusValue}",
                execution.JobExecutionId,
                execution.Status);
            throw new InvalidOperationException(
                $"Invalid status '{execution.Status}' in database for JobExecutionId '{execution.JobExecutionId}'.");
        }

        return new JobExecutionRecord
        {
            ExecutionId = 0,
            JobName = execution.JobName,
            JobExecutionId = execution.JobExecutionId,
            JobQueueId = execution.JobQueueId,
            UserId = execution.RequestedBy,
            JobType = JobType.Unknown,
            RunMode = RunMode.Manual,
            Status = status,
            RequestedAtUtc = execution.RequestedAtUtc,
            FpsYear = execution.FpsYear,
            StartedAt = execution.StartDateTime ?? DateTime.UtcNow,
            CompletedAt = execution.EndDateTime,
            DurationSeconds = execution.EndDateTime.HasValue && execution.StartDateTime.HasValue
                ? (int)(execution.EndDateTime.Value - execution.StartDateTime.Value).TotalSeconds
                : null,
            ErrorMessage = execution.ErrorMessage,
            RetryAttempts = 0
        };
    }

    /// <inheritdoc />
    public async Task<Guid> CreateInitiatedRecordAsync(
        string jobName,
        Guid jobExecutionId,
        string requestedBy,
        DateTime requestedAtUtc,
        RunMode runMode,
        CancellationToken cancellationToken = default,
        int? fpsYear = null)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name is required.", nameof(jobName));
        if (string.IsNullOrWhiteSpace(requestedBy))
            throw new ArgumentException("RequestedBy is required.", nameof(requestedBy));

        var jobId = await EnsureJobMasterAsync(jobName, cancellationToken);
        var statusId = await EnsureStatusAsync(jobId, nameof(JobStatus.Initiated), cancellationToken);

        if (fpsYear.HasValue)
        {
            var activeExecution = await (
                from q in _context.TblJobQueue
                join s in _context.TblJobStatus on q.StatusId equals s.StatusId
                where q.JobId == jobId
                      && q.FpsYear == fpsYear.Value
                      && (s.Status == nameof(JobStatus.Initiated) || s.Status == nameof(JobStatus.Running))
                orderby q.UpdatedAt descending
                select new { q.JobExecutionId, q.JobQueueId, s.Status })
                .FirstOrDefaultAsync(cancellationToken);

            if (activeExecution is not null)
            {
                throw new InvalidOperationException(
                    $"Cannot start '{jobName}' for fpsyear {fpsYear.Value}. Active execution exists " +
                    $"(JobExecutionId={activeExecution.JobExecutionId}, JobQueueId={activeExecution.JobQueueId}, Status={activeExecution.Status}).");
            }
        }

        var jobQueueId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        _context.TblJobQueue.Add(new TblJobQueue
        {
            JobQueueId = jobQueueId,
            JobExecutionId = jobExecutionId,
            JobId = jobId,
            StatusId = statusId,
            RequestedBy = requestedBy,
            RequestedAtUtc = requestedAtUtc,
            FpsYear = fpsYear,
            // Keep compatibility with environments where startdatetime is still NOT NULL.
            // Status remains Initiated; worker updates the value when transitioning to Running.
            StartDateTime = requestedAtUtc,
            EndDateTime = null,
            ErrorMessage = null,
            CreatedAt = now,
            UpdatedAt = now
        });

        _context.TblJobQueueLog.Add(new TblJobQueueLog
        {
            JobQueueId = jobQueueId,
            StatusId = statusId,
            PerformedBy = requestedBy,
            LogTime = now,
            Note = "Job accepted by API - Initiated"
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "[API → DB] ✓ Initiated record created | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | RunMode={RunMode} | RequestedBy={RequestedBy} | RequestedAtUtc={RequestedAtUtc} | FpsYear={FpsYear}",
            jobName, jobExecutionId, jobQueueId, runMode, requestedBy, requestedAtUtc, fpsYear);

        return jobQueueId;
    }

    /// <inheritdoc />
    public async Task<bool> TouchRunningExecutionAsync(Guid jobQueueId, CancellationToken cancellationToken = default)
    {
        if (jobQueueId == Guid.Empty)
            throw new ArgumentException("Job queue ID cannot be empty.", nameof(jobQueueId));

        var runningStatusId = await (
            from q in _context.TblJobQueue
            join s in _context.TblJobStatus on q.StatusId equals s.StatusId
            where q.JobQueueId == jobQueueId && s.Status == nameof(JobStatus.Running)
            select q.StatusId)
            .FirstOrDefaultAsync(cancellationToken);

        if (runningStatusId == 0)
        {
            return false;
        }

        var updatedRows = await _context.TblJobQueue
            .Where(q => q.JobQueueId == jobQueueId && q.StatusId == runningStatusId)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(q => q.UpdatedAt, _ => DateTime.UtcNow),
                cancellationToken);

        return updatedRows > 0;
    }

    /// <inheritdoc />
    public async Task<bool> IsExecutionMarkedFailedAsync(Guid jobExecutionId, CancellationToken cancellationToken = default)
    {
        if (jobExecutionId == Guid.Empty)
            throw new ArgumentException("Job execution ID cannot be empty.", nameof(jobExecutionId));

        return await (
            from q in _context.TblJobQueue
            join s in _context.TblJobStatus on q.StatusId equals s.StatusId
            where q.JobExecutionId == jobExecutionId && s.Status == nameof(JobStatus.Failed)
            select q.JobQueueId)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<JobQueueApprovalMetadata?> GetApprovalMetadataAsync(Guid jobExecutionId, CancellationToken cancellationToken = default)
    {
        if (jobExecutionId == Guid.Empty)
            throw new ArgumentException("Job execution ID cannot be empty.", nameof(jobExecutionId));

        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        if (!await AllApprovalColumnsExistAsync(connection, cancellationToken))
        {
            _logger.LogInformation(
                "Year End job_queue approval metadata columns are not yet provisioned in this environment (see CR025) | JobExecutionId={JobExecutionId}",
                jobExecutionId);
            return null;
        }

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT approved_by, approved_at_utc,
                   rejected_by, rejected_at_utc, rejection_reason,
                   triggered_by, triggered_at_utc
            FROM fps.job_queue
            WHERE jobexecutionid = @jobexecutionid;";
        AddParameter(command, "jobexecutionid", jobExecutionId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new JobQueueApprovalMetadata(
            reader.IsDBNull(0) ? null : reader.GetString(0),
            reader.IsDBNull(1) ? null : reader.GetDateTime(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetDateTime(3),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetDateTime(6));
    }

    private static readonly string[] ApprovalMetadataColumns =
    {
        "approved_by",
        "approved_at_utc",
        "rejected_by",
        "rejected_at_utc",
        "rejection_reason",
        "triggered_by",
        "triggered_at_utc"
    };

    private static async Task<bool> AllApprovalColumnsExistAsync(DbConnection connection, CancellationToken cancellationToken)
    {
        foreach (var column in ApprovalMetadataColumns)
        {
            if (!await ColumnExistsAsync(connection, column, cancellationToken))
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<bool> ColumnExistsAsync(DbConnection connection, string column, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = 'fps'
                  AND table_name = 'job_queue'
                  AND column_name = @column
            );";
        AddParameter(command, "column", column);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is bool exists && exists;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private async Task<int> EnsureJobMasterAsync(string jobName, CancellationToken cancellationToken)
    {
        var existing = await _context.TblJobMaster
            .FirstOrDefaultAsync(j => j.JobName == jobName, cancellationToken);

        if (existing != null)
            return existing.JobId;

        throw new InvalidOperationException(
            $"Job master record not found for job name '{jobName}'. " +
            $"Job must be created and configured in the database before execution.");
    }

    private async Task<int> EnsureStatusAsync(int jobId, string status, CancellationToken cancellationToken)
    {
        var existing = await _context.TblJobStatus
            .FirstOrDefaultAsync(s => s.JobId == jobId && s.Status == status, cancellationToken);

        if (existing != null)
            return existing.StatusId;

        throw new InvalidOperationException(
            $"Job status row not found for JobId '{jobId}' and Status '{status}'. " +
            "Status catalog must be provisioned via approved DBA migration/CR scripts before worker execution.");
    }

    private static string BuildStatusNote(JobStatus status) => status switch
    {
        JobStatus.Completed => "Execution completed",
        JobStatus.Failed => "Execution failed",
        _ => $"Status changed to {status}"
    };

    private static string BuildStartTransitionNote(JobStatus previousStatus)
    {
        return previousStatus switch
        {
            JobStatus.Initiated => "Worker started execution - Initiated → Running",
            JobStatus.Approved => "Worker started execution - Approved → Running",
            _ => $"Worker started execution - {previousStatus} → Running"
        };
    }

    private static bool IsYearEndJob(string jobName)
    {
        return string.Equals(jobName, BatchJobNames.YearEndDataSetup, StringComparison.OrdinalIgnoreCase)
            || string.Equals(jobName, BatchJobNames.YearEndCutover, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsApprovalBasedJob(string jobName)
    {
        return IsYearEndJob(jobName)
            || string.Equals(jobName, BatchJobNames.BulkTestRatesUpdate, StringComparison.OrdinalIgnoreCase)
            || string.Equals(jobName, BatchJobNames.BulkStaffRatesUpdate, StringComparison.OrdinalIgnoreCase)
            || string.Equals(jobName, BatchJobNames.BulkAnimalRatesUpdate, StringComparison.OrdinalIgnoreCase);
    }
}
