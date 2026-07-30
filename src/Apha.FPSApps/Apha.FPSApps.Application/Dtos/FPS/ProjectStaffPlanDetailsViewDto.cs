namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class ProjectStaffPlanDetailsViewDto
    {
        public string? Program { get; set; }
        public string? Name { get; set; }
        public string? Manager { get; set; }
        public string? ProjectStatus { get; set; }
        public double? PlannedHours { get; set; }
        public decimal? ChargeRate { get; set; }
        public decimal? Cost { get; set; }
        public string? ProfitCentre { get; set; }
        public string? WorkGroup { get; set; }
        public string? GradeCode { get; set; }
        public int? FpsYear { get; set; }
    }
}
