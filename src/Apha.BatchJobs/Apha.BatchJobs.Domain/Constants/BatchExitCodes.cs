namespace Apha.BatchJobs.Domain.Constants;

/// <summary>
/// Canonical process exit codes for the batch worker. Each code maps to a specific
/// failure category so CloudWatch metric filters can distinguish outcomes precisely.
/// </summary>
public static class BatchExitCodes
{
    /// <summary>Job completed without error.</summary>
    public const int Success = 0;

    /// <summary>Input failed validation (<see cref="Exceptions.JobValidationException"/>).</summary>
    public const int ValidationFailure = 10;

    /// <summary>PostgreSQL / database error.</summary>
    public const int DatabaseFailure = 20;

    /// <summary>Failed to acquire distributed lock (<see cref="Exceptions.JobLockException"/>).</summary>
    public const int LockFailure = 30;

    /// <summary>Invalid startup context or missing required configuration.</summary>
    public const int ConfigurationFailure = 40;

    /// <summary>Business notification email error (<see cref="Exceptions.BusinessEmailException"/>).</summary>
    public const int EmailFailure = 50;

    /// <summary>Job was cancelled via <see cref="System.OperationCanceledException"/>.</summary>
    public const int Cancelled = 60;

    /// <summary>Uncategorised / unhandled exception.</summary>
    public const int UnhandledFailure = 99;
}
