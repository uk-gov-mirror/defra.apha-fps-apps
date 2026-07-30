namespace Apha.FPS.Core.Entities
{
    // Staff-of-grade grid (Access subform fsubResourceTotals2).
    // TODO: Subform .frm source is missing; columns inferred from
    // source/ui/fps/stage2_Check_resource_allocation.js. Confirm the backing
    // PostgreSQL view/table and real column names with the team.
    public partial class ResourceStaffAllocationView
    {
        public string? WorkGroupGrade { get; set; }

        public int? StaffId { get; set; }

        public string? Name { get; set; }

        public double? HoursAvailable { get; set; }

        public double? PlannedHours { get; set; }

        public double? AllocationPct { get; set; }

        public double? AssuredChargeHours { get; set; }

        public double? AssuredUtilisationPct { get; set; }

        public double? ChargeHours { get; set; }

        public double? UtilisationPct { get; set; }
    }
}
