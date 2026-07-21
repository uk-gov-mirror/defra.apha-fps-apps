using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Infrastructure.Repositories.MilestoneUpdateNotifications;

/// <summary>
/// Implementation of <see cref="IEmailService"/> that wraps another <see cref="IEmailService"/>
/// and redirects every recipient to a configured test mailbox list in non-production
/// environments (spec section 22: "non-production environments must support recipient
/// override" / "real users must not receive test emails" — plan section 10.4). Wraps the
/// abstraction, not <c>IGraphEmailService</c> directly, so redirect logic is independent
/// of which provider actually sends the message.
/// </summary>
public sealed class NonProdEmailRedirectDecorator : IEmailService
{
    private readonly IEmailService _inner;
    private readonly MilestoneNotificationsSettings _settings;
    private readonly string _environmentName;
    private readonly ILogger<NonProdEmailRedirectDecorator> _logger;

    public NonProdEmailRedirectDecorator(
        IEmailService inner,
        IOptions<MilestoneNotificationsSettings> settings,
        ILogger<NonProdEmailRedirectDecorator> logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _settings = settings?.Value ?? new MilestoneNotificationsSettings();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environmentName = EnvironmentResolver.GetEnvironmentName("Development");
    }

    /// <inheritdoc />
    public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!ShouldRedirect())
            return _inner.SendAsync(message, cancellationToken);

        if (_settings.NonProdRedirectRecipients.Count == 0)
        {
            throw new InvalidOperationException(
                $"Non-production email redirect is enabled for environment '{_environmentName}' but " +
                "MilestoneNotifications:NonProdRedirectRecipients is empty — refusing to send to the real " +
                "recipient list.");
        }

        var originalRecipients = string.Join(", ", message.To);
        var redirected = message with
        {
            To = _settings.NonProdRedirectRecipients,
            Subject = $"[TEST - would send to: {originalRecipients}] {message.Subject}"
        };

        _logger.LogInformation(
            "Non-prod redirect active ({Environment}): redirecting email from {OriginalRecipientCount} real " +
            "recipient(s) to {RedirectRecipientCount} test recipient(s)",
            _environmentName, message.To.Count, _settings.NonProdRedirectRecipients.Count);

        return _inner.SendAsync(redirected, cancellationToken);
    }

    private bool ShouldRedirect()
    {
        if (!_settings.NonProdRedirectEnabled)
            return false;

        return !string.Equals(_environmentName, "Production", StringComparison.OrdinalIgnoreCase);
    }
}
