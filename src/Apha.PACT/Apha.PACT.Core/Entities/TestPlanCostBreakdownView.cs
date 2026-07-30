namespace Apha.PACT.Core.Entities
{
    public class TestPlanCostBreakdownView
    {
        public string TestCode { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public double? PlanTotal { get; set; }
        public double? ReqTotalCost { get; set; }
        public int FpsYear { get; set; }
    }
}
