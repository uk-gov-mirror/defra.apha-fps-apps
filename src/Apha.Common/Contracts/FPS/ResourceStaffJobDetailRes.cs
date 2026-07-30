namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// API response contract for a staff job detail row
    /// (vtblStaffJob_RM LEFT JOIN vtlkpProject_General).
    /// </summary>
    public class ResourceStaffJobDetailRes
    {
        public string? StaffId { get; set; }
        public double? PlannedHours { get; set; }
        public string? JobCode { get; set; }
        public string? JobDescription { get; set; }
        public string? Programme { get; set; }
        public string? ProjectStatus { get; set; }
    }
}
