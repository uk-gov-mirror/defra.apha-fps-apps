namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// API response contract for a jobs-for-staff row in the Stage 2
    /// Check Resource Allocation grid (frmResourceDetail2).
    /// </summary>
    public class ResourceStaffJobRes
    {
        public int? StaffId { get; set; }
        public string? Project { get; set; }
        public string? Description { get; set; }
        public double? Hour { get; set; }
        public string? Status { get; set; }
    }
}
