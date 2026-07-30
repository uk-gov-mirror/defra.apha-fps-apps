namespace Apha.Common.Utilities.EventPublisher
{
    public interface IEventPublisherService
    {
        public Task<string> PublishAsync(EventDetail detail, CancellationToken cancellationToken = default);
    }
}
