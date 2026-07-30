namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class WorkGroupDto
    {
        public string WorkGroupName { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public double? CostCentre { get; set; }
        public string? Owner { get; set; }
        public string? Description { get; set; }
        public decimal? CentralOverhead { get; set; }
        public short? SendEmail { get; set; }
        public short? Cos90 { get; set; }
        public double? CostCentreOld { get; set; }
        public string? EmailRecipient { get; set; }
        public int? FpsYear { get; set; }
    }
}
