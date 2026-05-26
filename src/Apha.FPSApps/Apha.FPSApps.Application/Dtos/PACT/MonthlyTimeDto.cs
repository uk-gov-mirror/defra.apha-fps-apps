namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class MonthlyTimeDto
    {
        public string PactStaffId { get; set; } = null!;
        public string TimeCode { get; set; } = null!;
        public double Month { get; set; }
        public string ParentProject { get; set; } = null!;
        public string? WorkGroup { get; set; }
        public double? Hours { get; set; }
        public int FpsYear { get; set; }
    }
}
