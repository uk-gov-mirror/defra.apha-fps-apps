namespace Apha.PACT.Core.Entities
{
    public class BatchJobStatus
    {
        public int StatusId { get; set; }
        public int JobId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
