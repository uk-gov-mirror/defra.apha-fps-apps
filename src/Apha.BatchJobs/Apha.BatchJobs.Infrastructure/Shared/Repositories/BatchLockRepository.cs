using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apha.BatchJobs.Infrastructure.Shared.Repositories;

/// <summary>
/// Implementation of batch lock repository using EF Core.
/// </summary>
public class BatchLockRepository : IBatchLockRepository
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<BatchLockRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the BatchLockRepository.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">Optional logger for structured lock lifecycle events.</param>
    public BatchLockRepository(BatchJobsDbContext context, ILogger<BatchLockRepository>? logger = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? NullLogger<BatchLockRepository>.Instance;
    }

    /// <inheritdoc />
    public async Task<bool> TryAcquireLockAsync(string jobName, Guid jobQueueId, int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        if (jobQueueId == Guid.Empty)
            throw new ArgumentException("Job queue ID cannot be empty.", nameof(jobQueueId));

        var now = DateTime.UtcNow;
        _logger.LogInformation(
            "Lock acquisition requested | JobName={JobName} | JobQueueId={JobQueueId} | TimeoutSeconds={TimeoutSeconds}",
            jobName,
            jobQueueId,
            timeoutSeconds);

        // Remove expired locks first so the unique partial index slot is freed.
        await _context.BatchLocks
            .Where(l => l.JobName == jobName && l.ExpiresAt < now)
            .ExecuteDeleteAsync(cancellationToken);

        // Attempt atomic insert without raising an error on contention.
        // With uq_job_lock_job_name_active (partial unique on active rows),
        // this returns 1 when lock is acquired and 0 when already held.
        var expiresAt = timeoutSeconds > 0
            ? now.AddSeconds(timeoutSeconds)
            : DateTime.MaxValue;

        var insertedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.job_lock (acquired_at, expires_at, job_name, jobqueueid, is_active)
            VALUES ({now}, {expiresAt}, {jobName}, {jobQueueId}, TRUE)
            ON CONFLICT DO NOTHING;", cancellationToken);

        if (insertedRows > 0)
        {
            _logger.LogInformation("Lock acquired | JobName={JobName} | JobQueueId={JobQueueId}", jobName, jobQueueId);
            return true;
        }

        _context.ChangeTracker.Clear();
        _logger.LogInformation("Lock contention detected | JobName={JobName} | JobQueueId={JobQueueId}", jobName, jobQueueId);
        return false;
    }

    /// <inheritdoc />
    public async Task ReleaseLockAsync(string jobName, Guid jobQueueId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        if (jobQueueId == Guid.Empty)
            throw new ArgumentException("Job queue ID cannot be empty.", nameof(jobQueueId));

        // Guard against stale tracking state leaking from prior operations in the
        // same scoped DbContext (e.g., failed job step writes).
        _context.ChangeTracker.Clear();

        var lockToRelease = await _context.BatchLocks
            .FirstOrDefaultAsync(l => l.JobName == jobName && l.JobQueueId == jobQueueId, cancellationToken);

        if (lockToRelease != null)
        {
            _context.BatchLocks.Remove(lockToRelease);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Lock released | JobName={JobName} | JobQueueId={JobQueueId}", jobName, jobQueueId);
        }
        else
        {
            _logger.LogInformation("No lock found to release | JobName={JobName} | JobQueueId={JobQueueId}", jobName, jobQueueId);
        }
    }

    /// <inheritdoc />
    public async Task<bool> TryRenewLockAsync(string jobName, Guid jobQueueId, int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        if (jobQueueId == Guid.Empty)
            throw new ArgumentException("Job queue ID cannot be empty.", nameof(jobQueueId));

        // Unlimited timeout uses a non-expiring lease marker and does not require periodic renewal.
        if (timeoutSeconds <= 0)
        {
            return true;
        }

        var nextExpiresAt = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        var updatedRows = await _context.BatchLocks
            .Where(l => l.JobName == jobName && l.JobQueueId == jobQueueId && l.IsActive)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(l => l.ExpiresAt, _ => nextExpiresAt),
                cancellationToken);

        return updatedRows > 0;
    }

    /// <inheritdoc />
    public async Task<BatchLock?> GetActiveLockAsync(string jobName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        var now = DateTime.UtcNow;
        var activeLock = await _context.BatchLocks
            .FirstOrDefaultAsync(l => l.JobName == jobName && l.IsActive && l.ExpiresAt > now, cancellationToken);

        if (activeLock is null)
        {
            return null;
        }

        var queueState = await (
            from q in _context.TblJobQueue
            join s in _context.TblJobStatus on q.StatusId equals s.StatusId
            where q.JobQueueId == activeLock.JobQueueId
            select s.Status)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.Equals(queueState, "Running", StringComparison.OrdinalIgnoreCase)
            || string.Equals(queueState, "Pending", StringComparison.OrdinalIgnoreCase)
            || string.Equals(queueState, "Retry", StringComparison.OrdinalIgnoreCase))
        {
            return activeLock;
        }

        _logger.LogWarning(
            "Detected stale lock; releasing lock row | JobName={JobName} | JobQueueId={JobQueueId} | QueueState={QueueState}",
            jobName,
            activeLock.JobQueueId,
            queueState ?? "MissingQueueRecord");

        _context.BatchLocks.Remove(activeLock);
        await _context.SaveChangesAsync(cancellationToken);
        return null;
    }
}
