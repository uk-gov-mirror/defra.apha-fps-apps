using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Interfaces.MilestoneUpdateNotifications;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Infrastructure.Repositories.MilestoneUpdateNotifications;

/// <summary>
/// Builds and sends the post-run CAPS completion email (spec §15) and
/// records the outcome in <c>fps.notification_run_summary</c> via
/// <see cref="INotificationDeliveryRepository.FinalizeRunSummaryAsync"/>.
///
/// Non-fatal by design: any exception (other than <see cref="OperationCanceledException"/>)
/// is caught, logged as a warning, and the run summary is finalized as <c>Failed</c>.
/// A CAPS send failure must never propagate as a job failure.
/// </summary>
public sealed class CapsSummaryService : ICapsSummaryService
{
    // Subject preserved from the legacy PIMS batch job (spec §15).
    private const string CapsEmailSubject = "PIMS Monthly Milestone Edit Links";

    private readonly IEmailService _emailService;
    private readonly INotificationDeliveryRepository _deliveryRepository;
    private readonly MilestoneNotificationsSettings _settings;
    private readonly ILogger<CapsSummaryService> _logger;

    public CapsSummaryService(
        IEmailService emailService,
        INotificationDeliveryRepository deliveryRepository,
        IOptions<MilestoneNotificationsSettings> settings,
        ILogger<CapsSummaryService> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task SendAndRecordAsync(
        Guid runSummaryId,
        Guid jobQueueId,
        int fpsYear,
        int monthNumber,
        NotificationRunSummaryCounters counters,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "CAPSSummaryStarted | RunSummaryId={RunSummaryId} | JobQueueId={JobQueueId} | Year={Year} | Month={Month}",
            runSummaryId, jobQueueId, fpsYear, monthNumber);

        try
        {
            var capsMailbox = _settings.CapsMailbox;
            if (string.IsNullOrWhiteSpace(capsMailbox))
            {
                _logger.LogWarning(
                    "MilestoneNotifications:CapsMailbox is not configured — CAPS summary email cannot be sent. " +
                    "Finalizing run summary as NotAttempted | RunSummaryId={RunSummaryId}",
                    runSummaryId);
                await _deliveryRepository.FinalizeRunSummaryAsync(
                    runSummaryId,
                    "NotAttempted",
                    "CapsMailbox configuration is missing.",
                    null,
                    cancellationToken);
                return;
            }

            var htmlBody = BuildHtmlBody(jobQueueId, fpsYear, monthNumber, counters);
            var message = new EmailMessage(
                To: [capsMailbox],
                Subject: CapsEmailSubject,
                HtmlBody: htmlBody);

            var result = await _emailService.SendAsync(message, cancellationToken);

            if (result.Succeeded)
            {
                var sentAt = DateTime.UtcNow;
                _logger.LogInformation(
                    "CAPSSummarySent | RunSummaryId={RunSummaryId} | SentAtUtc={SentAtUtc:O}",
                    runSummaryId, sentAt);
                await _deliveryRepository.FinalizeRunSummaryAsync(
                    runSummaryId, "Sent", null, sentAt, cancellationToken);
            }
            else
            {
                _logger.LogWarning(
                    "CAPS summary email send failed — job status not affected | " +
                    "RunSummaryId={RunSummaryId} | Reason={Reason}",
                    runSummaryId, result.FailureMessage);
                await _deliveryRepository.FinalizeRunSummaryAsync(
                    runSummaryId, "Failed", result.FailureMessage, null, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "CAPS summary step threw an unhandled exception — non-fatal, job status not affected | " +
                "RunSummaryId={RunSummaryId}",
                runSummaryId);

            // Best-effort finalize — ignore secondary failures so the outer catch
            // does not shadow the primary exception with a finalize failure.
            try
            {
                await _deliveryRepository.FinalizeRunSummaryAsync(
                    runSummaryId, "Failed", ex.Message, null, cancellationToken);
            }
            catch (Exception finalizeEx) when (finalizeEx is not OperationCanceledException)
            {
                _logger.LogWarning(finalizeEx,
                    "Could not finalize run summary after CAPS send failure | RunSummaryId={RunSummaryId}",
                    runSummaryId);
            }
        }
    }

    /// <summary>
    /// Builds the HTML body for the CAPS completion email (spec §15 modern summary content).
    /// </summary>
    private static string BuildHtmlBody(
        Guid jobQueueId,
        int fpsYear,
        int monthNumber,
        NotificationRunSummaryCounters counters)
    {
        var unresolvedLine = counters.DiagnosticAvailable
            ? $"<li>Recipients unresolved (excluded before view join): <strong>{counters.UnresolvedRecipientCount ?? 0}</strong> manager(s) / <strong>{counters.UnresolvedProjectCount ?? 0}</strong> project(s)</li>"
            : "<li>Recipients unresolved: <em>diagnostic unavailable — query failed, see worker logs</em></li>";

        // No-links skips = total skipped − disabled − missing-email
        var noLinksSkipped = counters.ManagerEmailSkippedCount
            - counters.DisabledRecipientCount
            - counters.MissingEmailRecipientCount;

        return $@"<html><body>
<h2>PIMS Monthly Milestone Notification Run Summary</h2>
<p>Project Manager milestone edit links were mailed out. See run details below.</p>
<table border=""0"" cellpadding=""4"" cellspacing=""0"">
  <tr><td><strong>Job Queue ID</strong></td><td>{jobQueueId:D}</td></tr>
  <tr><td><strong>Reporting Year</strong></td><td>{fpsYear}</td></tr>
  <tr><td><strong>Calendar Month</strong></td><td>{monthNumber}</td></tr>
</table>
<h3>Delivery Summary</h3>
<ul>
  <li>Candidates loaded: <strong>{counters.CandidateCount}</strong> row(s) / <strong>{counters.CandidateProjectCount}</strong> distinct project(s)</li>
  <li>Managers identified (recipient groups): <strong>{counters.IdentifiedRecipientCount}</strong></li>
  <li>Manager deliveries (attempts + skips): <strong>{counters.ManagerDeliveryCount}</strong></li>
  <li>Email attempts: <strong>{counters.ManagerEmailAttemptCount}</strong>
    &mdash; Sent: <strong>{counters.ManagerEmailSentCount}</strong>
    / Failed: <strong>{counters.ManagerEmailFailedCount}</strong></li>
  <li>Skipped (total): <strong>{counters.ManagerEmailSkippedCount}</strong>
    &mdash; Disabled: <strong>{counters.DisabledRecipientCount}</strong>
    / Missing email: <strong>{counters.MissingEmailRecipientCount}</strong>
    / No valid project links: <strong>{noLinksSkipped}</strong></li>
  {unresolvedLine}
  <li>Attempts with unknown delivery outcome (require operational review): <strong>{counters.OutcomeUnknownRecipientCount}</strong></li>
  <li>Duplicate sends prevented: <strong>{counters.DuplicateSkippedCount}</strong></li>
</ul>
<p><em>This email was generated automatically by the FPS Batch Jobs worker.</em></p>
</body></html>";
    }
}
