using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Stub implementation of <see cref="IEventBridgePublisher"/>.
    /// Logs the payload and returns successfully without sending to EventBridge.
    /// Replace with the production AWS SDK implementation when EventBridge integration is ready.
    /// </summary>
    public class NullEventBridgePublisher : IEventBridgePublisher
    {
        private readonly ILogger<NullEventBridgePublisher> _logger;

        public NullEventBridgePublisher(ILogger<NullEventBridgePublisher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task PublishApprovalEventAsync(BulkRatesEventPayload payload, CancellationToken ct = default)
        {
            _logger.LogInformation(
                "[NullEventBridgePublisher] Approval event not sent (stub). JobExecutionId={JobExecutionId} JobName={JobName}",
                payload.JobExecutionId, payload.JobName);
            return Task.CompletedTask;
        }
    }
}
