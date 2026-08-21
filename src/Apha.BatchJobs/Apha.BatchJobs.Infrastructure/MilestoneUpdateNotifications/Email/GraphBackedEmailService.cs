using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Apha.BatchJobs.Infrastructure.Email;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Infrastructure.MilestoneUpdateNotifications.Email;

/// <summary>
/// Implementation of <see cref="IEmailService"/>. Thin adapter over Apha.Common's
/// existing <see cref="IGraphEmailService"/> (plan section 10.1) rather than a second,
/// parallel Graph integration. Converts send failures into a <see cref="EmailSendResult"/>
/// instead of throwing, so a per-recipient loop doesn't need its own try/catch around
/// every call (spec section 14: "one recipient failure must not stop the remaining
/// recipients").
/// </summary>
public sealed class GraphBackedEmailService : IEmailService
{
    private readonly IGraphEmailService _graphEmailService;
    private readonly ILogger<GraphBackedEmailService> _logger;

    public GraphBackedEmailService(IGraphEmailService graphEmailService, ILogger<GraphBackedEmailService> logger)
    {
        _graphEmailService = graphEmailService ?? throw new ArgumentNullException(nameof(graphEmailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        var graphMessage = new EmailMessageModel
        {
            To = message.To.ToList(),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true
        };

        try
        {
            await _graphEmailService.SendEmailAsync(graphMessage, cancellationToken);
            return EmailSendResult.Sent();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                "Failed to send milestone notification email to {RecipientCount} recipient(s)",
                message.To.Count);
            return EmailSendResult.Failed(ex.Message);
        }
    }
}
