namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for a component charge per profit centre (TestRCCost).
    /// Maps to fps.tbltestrccost (composite PK: TestCode + ProfitCentre + FpsYear).
    /// Consumed by GET /api/v1/testrccost/{testCode}/{fpsYear}.
    /// </summary>
    public class TestRCCostRes
    {
        public string TestCode { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        public decimal Price { get; set; }
    }
}
