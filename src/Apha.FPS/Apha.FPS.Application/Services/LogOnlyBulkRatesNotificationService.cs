using Apha.FPS.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Stub implementation of <see cref="IBulkRatesNotificationService"/>.
    /// Logs the event without sending any message.
    /// Replace with the production email/Teams implementation when notifications are ready.
    /// </summary>
    public class LogOnlyBulkRatesNotificationService : IBulkRatesNotificationService
    {
        private readonly ILogger<LogOnlyBulkRatesNotificationService> _logger;

        public LogOnlyBulkRatesNotificationService(ILogger<LogOnlyBulkRatesNotificationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task NotifyAsync(
            BulkRatesNotificationEvent notificationEvent,
            BulkRatesNotificationContext context,
            CancellationToken ct = default)
        {
            _logger.LogInformation(
                "[LogOnlyBulkRatesNotificationService] Notification not sent (stub). Event={Event} JobQueueId={JobQueueId} RequestedBy={RequestedBy}",
                notificationEvent, context.JobQueueId, context.RequestedBy);
            return Task.CompletedTask;
        }
    }
}
