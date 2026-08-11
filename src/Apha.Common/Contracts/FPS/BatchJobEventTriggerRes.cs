namespace Apha.Common.Contracts.FPS
{
    public class BatchJobEventTriggerRes
    {
        public BatchJobQueueRes Jobqueue { get; set; } = null!;
        public string EventId { get; set; } = string.Empty;
    }
}
