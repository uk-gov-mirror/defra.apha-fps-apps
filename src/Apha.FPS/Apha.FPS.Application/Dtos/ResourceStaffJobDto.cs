namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// DTO for a single row in the Jobs-for-Staff grid
    /// (Access frmResourceDetail2).
    /// </summary>
    public class ResourceStaffJobDto
    {
        public int? StaffId { get; set; }
        public string? Project { get; set; }
        public string? Description { get; set; }
        public double? Hour { get; set; }
        public string? Status { get; set; }
    }
}
