using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Apha.Common.Utilities.EventPublisher
{
    public class EventBridgePublisherService : IEventPublisherService
    {
        private readonly IAmazonEventBridge _eventBridge;
        private readonly IConfiguration _configuration;
 
        public EventBridgePublisherService(IAmazonEventBridge eventBridge,IConfiguration configuration)
        {
            _eventBridge = eventBridge;
            _configuration = configuration;
        }

        public async Task<string> PublishAsync(EventDetail detail, CancellationToken cancellationToken = default)
        {
           var _eventBusArn = _configuration["EventBridge:EventBusArn"]
                ?? throw new InvalidOperationException("EventBridge:EventBusArn is not configured.");

            var _eventSource = _configuration["EventBridge:Source"]
                ?? throw new InvalidOperationException("EventBridge:EventBusArn is not configured.");

            var _eventType = _configuration["EventBridge:DetailType"]
                ?? throw new InvalidOperationException("EventBridge:EventBusArn is not configured.");

            var payload = JsonSerializer.Serialize(detail);

            var request = new PutEventsRequest
            {
                Entries =
                [
                    new PutEventsRequestEntry
                    {
                        EventBusName = _eventBusArn,
                        Source = _eventSource,
                        DetailType = _eventType,
                        Detail = payload
                    }
                ]
            };

            var response = await _eventBridge.PutEventsAsync(request, cancellationToken);

            if (response.FailedEntryCount > 0)
            {
                var failure = response.Entries.FirstOrDefault(
                    entry => !string.IsNullOrWhiteSpace(entry.ErrorCode));

                throw new InvalidOperationException(
                    $"Failed to publish event: " + $"{failure?.ErrorCode} - {failure?.ErrorMessage}");
            }

            return response.Entries[0].EventId;
        }
    }
}
