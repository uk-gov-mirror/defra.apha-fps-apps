namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// DTO for a staff-job re-plan record. Maps from <c>StaffJobRmView</c>.
    /// </summary>
    public class ResourceMgmtReplanDto
    {
        public string? StaffId { get; set; }

        public string? JobCode { get; set; }

        public double PlannedHours { get; set; }

        public string? Name { get; set; }

        public int? FpsYear { get; set; }
    }
}
