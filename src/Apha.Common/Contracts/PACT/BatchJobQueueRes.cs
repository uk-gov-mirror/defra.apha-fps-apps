namespace Apha.Common.Contracts.PACT
{
    public class BatchJobQueueRes
    {
        public Guid JobqueueId { get; set; }
        public Guid JobExecutionId { get; set; }
        public int JobId { get; set; }
        public int StatusId { get; set; }
        public string RequestedBy { get; set; } = null!;
        public DateTime? RequestedAtUtc { get; set; }
        public DateTime StartDateTime { get; set; }
    }
}
