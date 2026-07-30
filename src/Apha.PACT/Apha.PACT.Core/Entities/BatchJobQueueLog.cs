namespace Apha.PACT.Core.Entities
{
    public class BatchJobQueueLog
    {
        public int JobqueueLogId { get; set; }
        public Guid JobqueueId { get; set; }
        public int StatusId { get; set; }
        public string PerformedBy { get; set; } = null!;
        public DateTime LogTime { get; set; }
        public string? Note { get; set; }
    }
}
