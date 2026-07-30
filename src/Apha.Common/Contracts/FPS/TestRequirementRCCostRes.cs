namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for a project-specific component charge (TestRequirementRCCost).
    /// Maps to fps.tbltestrequirementrccost
    /// (composite PK: TestCode + Buyer + ProfitCentre + FpsYear).
    /// Consumed by GET /api/v1/testrequirementrccost/{testCode}/{fpsYear}.
    /// </summary>
    public class TestRequirementRCCostRes
    {
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        public decimal Price { get; set; }
    }
}
