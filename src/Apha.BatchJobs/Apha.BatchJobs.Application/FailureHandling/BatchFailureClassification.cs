namespace Apha.BatchJobs.Application.FailureHandling;

/// <summary>
/// Result of classifying a non-cancellation exception: the process exit code, the coarse
/// failure category for the structured run summary, and the CloudWatch <c>ErrorType</c>
/// marker token (already resolved from <c>ExceptionTypes</c> configuration).
/// </summary>
public sealed record BatchFailureClassification(int ExitCode, BatchFailureCategory Category, string ErrorType);
