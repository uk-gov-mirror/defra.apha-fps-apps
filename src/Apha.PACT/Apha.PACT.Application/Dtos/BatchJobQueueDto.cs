namespace Apha.PACT.Application.Dtos
{
    public class BatchJobQueueDto
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
