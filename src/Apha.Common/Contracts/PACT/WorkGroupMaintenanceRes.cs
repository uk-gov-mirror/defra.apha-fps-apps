namespace Apha.Common.Contracts.PACT
{
    /// <summary>
    /// Response contract for WorkGroup maintenance CRUD operations.
    /// Covers the full workgroup column surface plus a synthetic Id for DataGrid row binding.
    /// </summary>
    public class WorkGroupMaintenanceRes
    {
        public int Id { get; set; }
        public string WorkGroupName { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public double? CostCentre { get; set; }
        public string? Owner { get; set; }
        public string? Description { get; set; }
        public decimal? CentralOverhead { get; set; }
        public short? SendEmail { get; set; }
        public short? Cos90 { get; set; }
        public string? EmailRecipient { get; set; }
        public int FpsYear { get; set; }
    }
}
