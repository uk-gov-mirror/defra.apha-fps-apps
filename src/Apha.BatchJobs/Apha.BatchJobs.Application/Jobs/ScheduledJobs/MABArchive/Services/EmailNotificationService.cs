using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Application.Configuration;
using Apha.BatchJobs.Domain.Entities.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;

/// <summary>
/// Sends failure-alert emails through <see cref="IEmailService"/>.
/// Resolves <see cref="IEmailService"/> lazily: eager resolution would throw wherever
/// GraphEmailSettings is unconfigured, breaking every job that depends on this service.
/// </summary>
public sealed class EmailNotificationService : IEmailNotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly BatchAlertingSettings _settings;
    private readonly AwsLoggingSettings _awsLoggingSettings;
    private readonly Func<IEmailService> _emailServiceFactory;

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
