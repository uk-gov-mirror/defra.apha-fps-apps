namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// DTO for a single row in the Staff-of-Grade allocation grid
    /// (Access fsubResourceTotals2).
    /// </summary>
    public class ResourceStaffAllocationDto
    {
        public string? WorkGroupGrade { get; set; }
        public string? StaffId { get; set; }
        public string? Name { get; set; }
        public double? HrsAvail { get; set; }
        public double ZtHours { get; set; }
        public double AppPlannedHours { get; set; }
        public double PlannedHours { get; set; }
        public double ChargeHours { get; set; }
        public double AppChargeHours { get; set; }
        public double? Allocation { get; set; }
        public double? Utilization { get; set; }
        public double? AppUtilization { get; set; }
    }
}
