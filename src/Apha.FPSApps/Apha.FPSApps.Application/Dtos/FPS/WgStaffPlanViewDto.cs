namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class WgStaffPlanViewDto
    {
        public string? WorkGroup { get; set; }
        public string? GradeCode { get; set; }
        public string? Name { get; set; }
        public string? Manager { get; set; }
        public string? Program { get; set; }
        public string? JobCode { get; set; }
        public string? ProjectStatus { get; set; }
        public double? PlannedHours { get; set; }
        public decimal? Fee { get; set; }
    }
}
