using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.Repositories;

/// <summary>
/// Implementation of job execution repository using EF Core.
/// </summary>
public class JobExecutionRepository : IJobExecutionRepository
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<JobExecutionRepository> _logger;
    private const int DefaultTimeToLiveSeconds = 3600;
    private const string CancellationRequestedStatus = "CancelRequested";
    private const string CancellationRequestedNotePrefix = "Cancellation requested";
    private const string CancellationPendingState = "Pending";
    private const string CancellationConsumedState = "Consumed";
    private const string CancellationTerminalizedState = "Terminalized";
    private const string UndefinedTableSqlState = "42P01";

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
            "Create execution record requested | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | UserId={UserId} | Status={Status}",
            record.JobName,
            record.JobExecutionId,
            record.JobQueueId,
            record.UserId,
            record.Status);

        // Check if an Initiated row already exists for this jobExecutionId (manual job path)
        var existingRow = await _context.TblJobQueue
            .FirstOrDefaultAsync(q => q.JobExecutionId == record.JobExecutionId, cancellationToken);

        if (existingRow != null)
        {
            // UPDATE the Initiated row to Running
            var statusId = await EnsureStatusAsync(existingRow.JobId, record.Status.ToString(), cancellationToken);
            var previousStatus = existingRow.StatusId;
            existingRow.StatusId = statusId;
            existingRow.StartDateTime = record.StartedAt;
            existingRow.RequestedBy = record.UserId;
            existingRow.UpdatedAt = now;

            // Ensure the record's JobQueueId matches the persisted row
            record.JobQueueId = existingRow.JobQueueId;

            _context.TblJobQueueLog.Add(new TblJobQueueLog
            {
                JobQueueId = existingRow.JobQueueId,
                StatusId = statusId,
                PerformedBy = record.UserId,
                LogTime = now,
                Note = "Worker started execution - Initiated → Running"
            });

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "[Worker → DB] ✓ Initiated → Running transition complete | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | StartDateTime={StartDateTime}",
                record.JobName,
                record.JobExecutionId,
                record.JobQueueId,
                record.StartedAt);

            return 0;
        }

        // No existing row — scheduled job path: INSERT a new Running row
        var jobId = await EnsureJobMasterAsync(record.JobName, cancellationToken);
        var newStatusId = await EnsureStatusAsync(jobId, record.Status.ToString(), cancellationToken);

        var queueRow = new TblJobQueue
        {
            JobQueueId = record.JobQueueId,
            JobExecutionId = record.JobExecutionId,
            JobId = jobId,
            StatusId = newStatusId,
            RequestedBy = record.UserId,
            RequestedAtUtc = record.RequestedAtUtc,
            StartDateTime = record.StartedAt,
            EndDateTime = null,
            ErrorMessage = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.TblJobQueue.Add(queueRow);
        _context.TblJobQueueLog.Add(new TblJobQueueLog
        {
            JobQueueId = record.JobQueueId,
            StatusId = newStatusId,
            PerformedBy = record.UserId,
            LogTime = now,
            Note = "Execution started"
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Execution record created | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | UserId={UserId} | Status={Status}",
            record.JobName,
            record.JobExecutionId,
            record.JobQueueId,
            record.UserId,
            record.Status);

        return 0;
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

        if (record.Status == JobStatus.Cancelled)
        {
            await TryMarkCancellationTerminalizedAsync(record.JobExecutionId, cancellationToken);
        }

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
                q.StartDateTime,
                q.EndDateTime,
                s.Status,
                q.ErrorMessage
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (execution == null)
            return null;

        var parsedStatus = Enum.TryParse<JobStatus>(execution.Status, true, out var status)
            ? status
            : JobStatus.Failed;

        return new JobExecutionRecord
        {
            ExecutionId = 0,
            JobName = execution.JobName,
            JobExecutionId = execution.JobExecutionId,
            JobQueueId = execution.JobQueueId,
            UserId = execution.RequestedBy,
            JobType = JobType.Unknown,
            RunMode = RunMode.Manual,
            Status = parsedStatus,
            RequestedAtUtc = execution.RequestedAtUtc,
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
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name is required.", nameof(jobName));
        if (string.IsNullOrWhiteSpace(requestedBy))
            throw new ArgumentException("RequestedBy is required.", nameof(requestedBy));

        var jobId = await EnsureJobMasterAsync(jobName, cancellationToken);
        var statusId = await EnsureStatusAsync(jobId, nameof(JobStatus.Initiated), cancellationToken);

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
            StartDateTime = null,   // Not started; worker sets this when transitioning to Running
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
            "[API → DB] ✓ Initiated record created | JobName={JobName} | JobExecutionId={JobExecutionId} | JobQueueId={JobQueueId} | RunMode={RunMode} | RequestedBy={RequestedBy} | RequestedAtUtc={RequestedAtUtc} | StartDateTime=null (pending worker lock acquisition)",
            jobName, jobExecutionId, jobQueueId, runMode, requestedBy, requestedAtUtc);

        return jobQueueId;
    }

    /// <inheritdoc />
    public async Task EnsureJobStatusCatalogAsync(CancellationToken cancellationToken = default)
    {
        var jobIds = await _context.TblJobMaster
            .Select(j => j.JobId)
            .ToListAsync(cancellationToken);

        if (jobIds.Count == 0)
            return;

        var baselineStatuses = new[]
        {
            nameof(JobStatus.Initiated),
            nameof(JobStatus.Running),
            nameof(JobStatus.Completed),
            nameof(JobStatus.Failed),
            nameof(JobStatus.Cancelled)
        };

        var existing = await _context.TblJobStatus
            .Where(s => jobIds.Contains(s.JobId) && baselineStatuses.Contains(s.Status))
            .Select(s => new { s.JobId, s.Status })
            .ToListAsync(cancellationToken);

        var existingSet = existing
            .Select(s => (s.JobId, s.Status))
            .ToHashSet();

        var now = DateTime.UtcNow;
        var toInsert = new List<TblJobStatus>();

        foreach (var jobId in jobIds)
        {
            foreach (var status in baselineStatuses)
            {
                if (existingSet.Contains((jobId, status)))
                    continue;

                toInsert.Add(new TblJobStatus
                {
                    JobId = jobId,
                    Status = status,
                    CreatedAt = now
                });
            }
        }

        if (toInsert.Count == 0)
            return;

        _context.TblJobStatus.AddRange(toInsert);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seeded baseline job statuses | Jobs={JobCount} | RowsInserted={RowsInserted}",
            jobIds.Count,
            toInsert.Count);
    }

    /// <inheritdoc />
    public async Task<bool> TryRequestCancellationAsync(Guid jobExecutionId, string requestedBy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestedBy))
            throw new ArgumentException("RequestedBy cannot be null or empty.", nameof(requestedBy));

        var created = await UpsertCancellationRequestAsync(jobExecutionId, requestedBy, "pact.api", cancellationToken);
        if (!created)
        {
            _logger.LogInformation(
                "Cancellation request already exists | JobExecutionId={JobExecutionId} | RequestedBy={RequestedBy}",
                jobExecutionId,
                requestedBy);
            return false;
        }

        var queueProjection = await _context.TblJobQueue
            .Where(q => q.JobExecutionId == jobExecutionId)
            .Select(q => new { q.JobQueueId, q.JobId })
            .FirstOrDefaultAsync(cancellationToken);

        if (queueProjection is null)
        {
            _logger.LogInformation(
                "Cancellation request persisted before execution row exists | JobExecutionId={JobExecutionId} | RequestedBy={RequestedBy}",
                jobExecutionId,
                requestedBy);
            return true;
        }

        var now = DateTime.UtcNow;
        var cancellationRequestedStatusId = await EnsureStatusAsync(
            queueProjection.JobId,
            CancellationRequestedStatus,
            cancellationToken);

        _context.TblJobQueueLog.Add(new TblJobQueueLog
        {
            JobQueueId = queueProjection.JobQueueId,
            StatusId = cancellationRequestedStatusId,
            PerformedBy = requestedBy.Trim(),
            LogTime = now,
            Note = $"{CancellationRequestedNotePrefix} by {requestedBy.Trim()}"
        });

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Cancellation request stored in queue log | JobExecutionId={JobExecutionId} | RequestedBy={RequestedBy}",
            jobExecutionId,
            requestedBy);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> UpsertCancellationRequestAsync(
        Guid jobExecutionId,
        string requestedBy,
        string? source = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestedBy))
            throw new ArgumentException("RequestedBy cannot be null or empty.", nameof(requestedBy));

        try
        {
            var now = DateTime.UtcNow;
            var inserted = await _context.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO fps.job_cancellation_request (jobexecutionid, requested_by, requested_at_utc, status, source)
VALUES ({jobExecutionId}, {requestedBy.Trim()}, {now}, {CancellationPendingState}, {source})
ON CONFLICT (jobexecutionid) DO NOTHING;", cancellationToken);

            return inserted > 0;
        }
        catch (PostgresException ex) when (ex.SqlState == UndefinedTableSqlState)
        {
            // Temporary fallback while waiting for DBA table rollout.
            return await LegacyTryRequestCancellationInQueueLogAsync(jobExecutionId, requestedBy, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<CancellationRequestRecord?> GetCancellationRequestAsync(Guid jobExecutionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var row = await _context.Set<TblJobCancellationRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.JobExecutionId == jobExecutionId, cancellationToken);

            if (row is null)
                return null;

            return new CancellationRequestRecord
            {
                JobExecutionId = row.JobExecutionId,
                RequestedBy = row.RequestedBy,
                RequestedAtUtc = row.RequestedAtUtc,
                Status = row.Status,
                ConsumedAtUtc = row.ConsumedAtUtc,
                ConsumedBy = row.ConsumedBy,
                TerminalizedAtUtc = row.TerminalizedAtUtc
            };
        }
        catch (PostgresException ex) when (ex.SqlState == UndefinedTableSqlState)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task MarkCancellationConsumedAsync(Guid jobExecutionId, string consumedBy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(consumedBy))
            throw new ArgumentException("ConsumedBy cannot be null or empty.", nameof(consumedBy));

        try
        {
            var row = await _context.Set<TblJobCancellationRequest>()
                .FirstOrDefaultAsync(c => c.JobExecutionId == jobExecutionId, cancellationToken);

            if (row is null)
                return;

            row.Status = CancellationConsumedState;
            row.ConsumedBy = consumedBy.Trim();
            row.ConsumedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == UndefinedTableSqlState)
        {
            // No-op until DBA table exists.
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsCancellationRequestedAsync(Guid jobExecutionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Set<TblJobCancellationRequest>()
                .AsNoTracking()
                .AnyAsync(c => c.JobExecutionId == jobExecutionId, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == UndefinedTableSqlState)
        {
            return await LegacyIsCancellationRequestedAsync(jobExecutionId, cancellationToken);
        }
    }

    private Task<bool> LegacyIsCancellationRequestedAsync(Guid jobExecutionId, CancellationToken cancellationToken)
        => (
            from q in _context.TblJobQueue
            join l in _context.TblJobQueueLog on q.JobQueueId equals l.JobQueueId
            join s in _context.TblJobStatus on l.StatusId equals s.StatusId
            where q.JobExecutionId == jobExecutionId
                && (
                    s.Status == CancellationRequestedStatus
                    || (l.Note != null && EF.Functions.ILike(l.Note, $"{CancellationRequestedNotePrefix}%"))
                )
            select l.JobQueueLogId
        ).AnyAsync(cancellationToken);

    private async Task<bool> LegacyTryRequestCancellationInQueueLogAsync(Guid jobExecutionId, string requestedBy, CancellationToken cancellationToken)
    {
        var queueProjection = await _context.TblJobQueue
            .Where(q => q.JobExecutionId == jobExecutionId)
            .Select(q => new { q.JobQueueId, q.JobId })
            .FirstOrDefaultAsync(cancellationToken);

        if (queueProjection is null)
            return false;

        var alreadyRequested = await _context.TblJobQueueLog
            .AnyAsync(l => l.JobQueueId == queueProjection.JobQueueId
                && l.Note != null
                && EF.Functions.ILike(l.Note, $"{CancellationRequestedNotePrefix}%"),
                cancellationToken);

        if (alreadyRequested)
            return false;

        var cancellationRequestedStatusId = await EnsureStatusAsync(
            queueProjection.JobId,
            CancellationRequestedStatus,
            cancellationToken);

        _context.TblJobQueueLog.Add(new TblJobQueueLog
        {
            JobQueueId = queueProjection.JobQueueId,
            StatusId = cancellationRequestedStatusId,
            PerformedBy = requestedBy.Trim(),
            LogTime = DateTime.UtcNow,
            Note = $"{CancellationRequestedNotePrefix} by {requestedBy.Trim()}"
        });

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task TryMarkCancellationTerminalizedAsync(Guid jobExecutionId, CancellationToken cancellationToken)
    {
        try
        {
            var row = await _context.Set<TblJobCancellationRequest>()
                .FirstOrDefaultAsync(c => c.JobExecutionId == jobExecutionId, cancellationToken);

            if (row is null)
                return;

            row.Status = CancellationTerminalizedState;
            row.TerminalizedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == UndefinedTableSqlState)
        {
            // No-op until DBA table exists.
        }
    }

    private async Task<int> EnsureJobMasterAsync(string jobName, CancellationToken cancellationToken)
    {
        var existing = await _context.TblJobMaster
            .FirstOrDefaultAsync(j => j.JobName == jobName, cancellationToken);

        if (existing != null)
            return existing.JobId;

        var now = DateTime.UtcNow;
        var row = new TblJobMaster
        {
            JobName = jobName,
            Frequency = null,
            Note = "Auto-created by worker runtime",
            TimeToLive = DefaultTimeToLiveSeconds,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.TblJobMaster.Add(row);
        await _context.SaveChangesAsync(cancellationToken);

        return row.JobId;
    }

    private async Task<int> EnsureStatusAsync(int jobId, string status, CancellationToken cancellationToken)
    {
        var existing = await _context.TblJobStatus
            .FirstOrDefaultAsync(s => s.JobId == jobId && s.Status == status, cancellationToken);

        if (existing != null)
            return existing.StatusId;

        var row = new TblJobStatus
        {
            JobId = jobId,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        _context.TblJobStatus.Add(row);
        await _context.SaveChangesAsync(cancellationToken);

        return row.StatusId;
    }

    private static string BuildStatusNote(JobStatus status) => status switch
    {
        JobStatus.Completed => "Execution completed",
        JobStatus.Failed => "Execution failed",
        JobStatus.Cancelled => "Execution cancelled",
        _ => $"Status changed to {status}"
    };
}
