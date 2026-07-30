namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// DTO for a single row in the staff job detail query
    /// (vtblStaffJob_RM LEFT JOIN vtlkpProject_General).
    /// </summary>
    public class ResourceStaffJobDetailDto
    {
        public string? StaffId { get; set; }
        public double? PlannedHours { get; set; }
        public string? JobCode { get; set; }
        public string? JobDescription { get; set; }
        public string? Programme { get; set; }
        public string? ProjectStatus { get; set; }
    }
}
