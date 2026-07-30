namespace Apha.FPS.Core.Entities
{
    // Jobs-for-staff grid (Access subform frmResourceDetail2).
    // TODO: Subform .frm source is missing; columns inferred from
    // source/ui/fps/stage2_Check_resource_allocation.js. Confirm the backing
    // PostgreSQL view/table and real column names with the team.
    public partial class ResourceStaffJobView
    {
        public int? StaffId { get; set; }

        public string? Project { get; set; }

        public string? Description { get; set; }

        public double? Hour { get; set; }

        public string? Status { get; set; }
    }
}
