namespace Apha.BatchJobs.Application.FailureHandling;

/// <summary>
/// Coarse classification of a batch job failure, used for the structured run summary.
/// Distinct from <see cref="Apha.BatchJobs.Domain.Constants.BatchExitCodes"/> (the process
/// exit code) and from the CloudWatch <c>ErrorType</c> marker
/// (<see cref="BatchFailureClassification.ErrorType"/>) —
/// only <c>General</c> and <c>Sql</c> markers have a wired CloudWatch alarm today, but the
/// exit code and this category stay more granular for diagnostics.
/// </summary>
public enum BatchFailureCategory
{
    Configuration,
    Concurrency,
    Email,
    Sql,
    DependencyOutage,
    Timeout,
    Authorization,
    Business
}
