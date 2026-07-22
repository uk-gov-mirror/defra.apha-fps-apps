namespace Apha.BatchJobs.Application.Interfaces;

/// <summary>
/// Service for sending email notifications on job failure. Job-agnostic — callers pass the
/// failing job's name as a parameter rather than this being scoped to any one job.
/// </summary>
public interface IEmailNotificationService
{
    /// <summary>
    /// Sends a failure notification email to the configured administrator.
    /// </summary>
    /// <param name="correlationId">The correlation identifier for this execution.</param>
    /// <param name="jobName">The name of the job that failed.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="timestamp">The timestamp when the failure occurred.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendFailureNotificationAsync(string correlationId, string jobName, string errorMessage, DateTime timestamp, CancellationToken cancellationToken);
}
