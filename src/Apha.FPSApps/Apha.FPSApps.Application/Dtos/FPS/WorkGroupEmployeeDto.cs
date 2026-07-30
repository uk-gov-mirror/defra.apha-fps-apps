namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class WorkGroupEmployeeDto
    {
        public string PactId { get; set; } = null!;
        public string SpNumber { get; set; } = null!;
        public string WorkGroupGrade { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PersonStatus { get; set; } = null!;
        public string? PersonClass { get; set; }
        public double HrsPaid { get; set; }
        public double Leave { get; set; }
        public double SickSpecial { get; set; }
        public double HrsAvail { get; set; }
        public int MakeAvailable { get; set; }
    }
}