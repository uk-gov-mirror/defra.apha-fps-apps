namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for create/update operations on project-specific component charges
    /// (TestRequirementRCCost).
    /// Route keys for update/delete: TestCode + Buyer + ProfitCentre + FpsYear
    /// (composite PK on fps.tbltestrequirementrccost).
    /// Consumed by POST /api/v1/testrequirementrccost and
    /// PUT /api/v1/testrequirementrccost/{testCode}/{buyer}/{profitCentre}/{fpsYear}.
    /// </summary>
    public class TestRequirementRCCostReq
    {
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        public decimal Price { get; set; }
    }
}
