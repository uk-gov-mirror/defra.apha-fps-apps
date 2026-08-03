using Apha.Common.Utilities.EventPublisher;
using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Application.Services
{
    public class EventBridgePublisher : IEventBridgePublisher
    {
        private readonly IEventPublisherService _publisher;

        public EventBridgePublisher(IEventPublisherService publisher)
        {
            _publisher = publisher;
        }

        public async Task PublishApprovalEventAsync(BulkRatesEventPayload payload, CancellationToken ct = default)
        {
            var detail = new EventDetail
            {
                JobExecutionId = payload.JobExecutionId.ToString(),
                JobName        = payload.JobName,
                RunMode        = payload.RunMode,
                RequestedBy    = payload.RequestedBy,
                RequestedAtUtc = payload.RequestedAtUtc,
                ParametersJson = payload.ParametersJson
            };

            await _publisher.PublishAsync(detail, ct);
        }
    }
}
