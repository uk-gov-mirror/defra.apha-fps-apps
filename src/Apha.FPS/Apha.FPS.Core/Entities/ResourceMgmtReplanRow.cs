namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Represents a staff-job re-plan row used in the staged and all-time panels
    /// (frmRM_RePlan). Populated from <c>StaffJobRmView</c>.
    /// </summary>
    public class ResourceMgmtReplanRow
    {
        public string? StaffId { get; set; }

        public string? JobCode { get; set; }

        public double PlannedHours { get; set; }

        public string? Name { get; set; }

        public int? FpsYear { get; set; }
    }
}
