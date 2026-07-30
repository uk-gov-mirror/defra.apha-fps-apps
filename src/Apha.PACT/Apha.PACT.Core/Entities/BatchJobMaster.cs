namespace Apha.PACT.Core.Entities
{
    public class BatchJobMaster
    {
        public int JobId { get; set; }
        public string JobName { get; set; } = null!;
        public string? Frequency { get; set; }
        public string? Note { get; set; }
        public int Timetolive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
