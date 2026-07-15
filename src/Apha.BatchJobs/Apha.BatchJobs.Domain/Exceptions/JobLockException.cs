namespace Apha.BatchJobs.Domain.Exceptions;

/// <summary>
/// Thrown when the distributed lock for a batch job cannot be acquired (e.g. the job
/// is already running or the lock table is unavailable).
/// Maps to exit code <see cref="Constants.BatchExitCodes.LockFailure"/> (30).
/// </summary>
public sealed class JobLockException : Exception
{
    public JobLockException(string message) : base(message) { }

    public JobLockException(string message, Exception innerException)
        : base(message, innerException) { }
}
