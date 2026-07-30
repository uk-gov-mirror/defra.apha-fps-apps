namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for create/update operations on component charges per profit centre (TestRCCost).
    /// Route keys for update/delete: TestCode + ProfitCentre + FpsYear
    /// (composite PK on fps.tbltestrccost).
    /// Consumed by POST /api/v1/testrccost and PUT /api/v1/testrccost/{testCode}/{profitCentre}/{fpsYear}.
    /// </summary>
    public class TestRCCostReq
    {
        public string TestCode { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        public decimal Price { get; set; }
    }
}
