namespace Apha.PACT.Application.Dtos
{
    public class WorkGroupDto
    {
        public string WorkGroupName { get; set; } = null!;
        public string? Description { get; set; }
        public int FpsYear { get; set; }
        public string? ProfitCentre { get; set; }
        public double? CostCentre { get; set; }
        public string? Owner { get; set; }
        public decimal? CentralOverhead { get; set; }
        public short? SendEmail { get; set; }
        public short? Cos90 { get; set; }
        public double? CostCentreOld { get; set; }
        public string? EmailRecipient { get; set; }
    }
}
