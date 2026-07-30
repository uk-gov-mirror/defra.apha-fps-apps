namespace Apha.PACT.Core.Entities
{
    public class BatchJobQueue
    {
        public Guid JobqueueId { get; set; }
        public Guid JobExecutionId { get; set; }
        public int JobId { get; set; }
        public int StatusId { get; set; } = 0;
        public string RequestedBy { get; set; } = null!;
        public DateTime? RequestedAtUtc { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int FpsYear { get; set; }
    }
}
