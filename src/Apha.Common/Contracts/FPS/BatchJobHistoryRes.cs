namespace Apha.Common.Contracts.FPS
{
    public class BatchJobHistoryRes
    {
        public int JobId { get; set; }
        public string JobName { get; set; } = null!;
        public Guid JobExecutionId { get; set; }
        public string Status { get; set; } = null!;
        public string RequestedBy { get; set; } = null!;
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
