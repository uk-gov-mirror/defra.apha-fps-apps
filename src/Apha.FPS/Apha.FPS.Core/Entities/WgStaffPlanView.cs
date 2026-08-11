namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Entity representing the fps.vpvtworkgroupstaffplan view.
    /// Provides a pivot summary of planned staff costs grouped by workgroup.
    /// </summary>
    public class WgStaffPlanView
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
        public int? FpsYear { get; set; }
    }
}
