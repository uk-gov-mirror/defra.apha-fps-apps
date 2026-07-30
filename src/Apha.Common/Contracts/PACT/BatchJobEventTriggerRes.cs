namespace Apha.Common.Contracts.PACT
{
    public class BatchJobEventTriggerRes
    {
        public BatchJobQueueRes Jobqueue { get; set; } = null!;
        public string EventId { get; set; } = string.Empty;
    }
}
