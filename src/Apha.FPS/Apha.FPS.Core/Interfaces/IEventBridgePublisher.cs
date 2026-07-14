using Apha.FPS.Core.Entities.BulkRates;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Contract for publishing a Bulk Rates approval event to EventBridge.
    /// The production implementation sends to the configured EventBridge bus.
    /// The stub (NullEventBridgePublisher) logs the payload and returns successfully.
    /// </summary>
    public interface IEventBridgePublisher
    {
        /// <summary>
        /// Publishes the approval event that triggers the Batch Worker via ECS.
        /// The event must match the input transformer contract expected by the EventBridge rule.
        /// </summary>
        Task PublishApprovalEventAsync(
            BulkRatesEventPayload payload,
            CancellationToken ct = default);
    }
}
