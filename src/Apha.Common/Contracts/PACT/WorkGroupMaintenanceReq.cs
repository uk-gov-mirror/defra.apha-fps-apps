namespace Apha.Common.Contracts.PACT
{
    /// <summary>
    /// Request contract for creating or updating a WorkGroup maintenance record.
    /// Contains only the writable fields exposed in the workgroup maintenance modal form.
    /// </summary>
    public class WorkGroupMaintenanceReq
    {
        public string WorkGroupName { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public double? CostCentre { get; set; }
        public string? Owner { get; set; }
        public string? Description { get; set; }
        public decimal? CentralOverhead { get; set; }
    }
}
