using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;

/// <summary>
/// Spec section 14's suggested email abstraction — implemented as a thin adapter over
/// Apha.Common's existing Microsoft Graph integration (plan section 10.1), so this job
/// depends on the agreed common email service rather than a second, parallel one.
/// </summary>
public interface IEmailService
{
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
