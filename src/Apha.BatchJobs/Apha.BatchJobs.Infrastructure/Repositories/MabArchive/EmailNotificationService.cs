using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Apha.BatchJobs.Domain.Configuration;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive;

/// <summary>
/// Implementation of IEmailNotificationService. Sends failure-alert emails through the existing
/// Graph-backed <see cref="IEmailService"/> (the same path MilestoneUpdateNotifications uses) —
/// no separate SMTP integration needed. Resolves <see cref="IEmailService"/> lazily via a
/// factory delegate, not as a plain constructor dependency: <c>IEmailService</c>'s DI chain
/// (GraphBackedEmailService → IGraphEmailService → GraphServiceClient) throws if
/// GraphEmailSettings isn't configured, which is true everywhere except local dev today. Eagerly
/// depending on it would break every job that depends on this service (constructor injection
/// resolves eagerly), even when notifications are disabled or the recipient is unset.
/// </summary>
public sealed class EmailNotificationService : IEmailNotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly BatchAlertingSettings _settings;
    private readonly AwsLoggingSettings _awsLoggingSettings;
    private readonly Func<IEmailService> _emailServiceFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailNotificationService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="settings">Shared batch-alerting settings (not MABArchive-specific — this service is invoked with the failing job's name as a parameter).</param>
    /// <param name="awsLoggingSettings">Shared AWS logging settings (CloudWatch log group referenced in the alert email body).</param>
    /// <param name="emailServiceFactory">Lazy resolver for <see cref="IEmailService"/> — only invoked once a notification is actually about to send.</param>
    public EmailNotificationService(
        ILogger<EmailNotificationService> logger,
        IOptions<BatchAlertingSettings> settings,
        IOptions<AwsLoggingSettings> awsLoggingSettings,
        Func<IEmailService> emailServiceFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? new BatchAlertingSettings();
        _awsLoggingSettings = awsLoggingSettings?.Value ?? new AwsLoggingSettings();
        _emailServiceFactory = emailServiceFactory ?? throw new ArgumentNullException(nameof(emailServiceFactory));
    }

    /// <summary>
    /// Sends a failure notification for a MABArchive execution.
    /// </summary>
    /// <param name="correlationId">Correlation identifier.</param>
    /// <param name="jobName">Job name.</param>
    /// <param name="errorMessage">Error message text.</param>
    /// <param name="timestamp">Failure timestamp (UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SendFailureNotificationAsync(
        string correlationId,
        string jobName,
        string errorMessage,
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        if (!_settings.EnableEmailNotifications)
        {
            _logger.LogInformation("Email notifications disabled. Skipping failure notification for CorrelationId={CorrelationId}", correlationId);
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.AdminNotificationEmail))
        {
            _logger.LogWarning("AdminNotificationEmail not configured. Cannot send failure notification for CorrelationId={CorrelationId}", correlationId);
            return;
        }

        try
        {
            _logger.LogInformation(
                "Sending failure notification | CorrelationId={CorrelationId} | Job={JobName} | To={Email}",
                correlationId,
                jobName,
                _settings.AdminNotificationEmail);

            var subject = $"[ALERT] {jobName} Job Failed - {timestamp:yyyy-MM-dd HH:mm:ss}";
            var body = $@"
Job Failure Notification

Job Name: {jobName}
Correlation ID: {correlationId}
Failure Time: {timestamp:yyyy-MM-dd HH:mm:ss} UTC
Error Message: {errorMessage}

For detailed diagnostics, check:
- CloudWatch Logs (group: {_awsLoggingSettings.LogGroupName}) filtered by CorrelationId={correlationId}
- Database lock table (tbl_job_queue) for lock status
- Recent batch job execution records

Contact your system administrator for assistance.
";

            var message = new EmailMessage([_settings.AdminNotificationEmail], subject, body);
            var result = await _emailServiceFactory().SendAsync(message, cancellationToken);

            if (result.Succeeded)
            {
                _logger.LogInformation("Failure notification sent | Subject={Subject} | To={Email}", subject, _settings.AdminNotificationEmail);
            }
            else
            {
                _logger.LogWarning(
                    "Failure notification could not be sent | Subject={Subject} | To={Email} | Reason={Reason}",
                    subject, _settings.AdminNotificationEmail, result.FailureMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send failure notification for CorrelationId={CorrelationId}", correlationId);
            throw;
        }
    }
}
