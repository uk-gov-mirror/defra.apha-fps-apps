namespace Apha.FPSApps.Application.Dtos.FPS
{
    // Same shape as backend DTO — all 9 columns from fps.animalreq_log audit trail table
    public class AnimalRequestLogDto
    {
        public int SequenceNo { get; set; }
        public string JobCode { get; set; } = null!;
        public string AnimalType { get; set; } = null!;
        public double NumberOfDays { get; set; }
        public double NumberOfAnimals { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        public int FpsYear { get; set; }
    }
}
