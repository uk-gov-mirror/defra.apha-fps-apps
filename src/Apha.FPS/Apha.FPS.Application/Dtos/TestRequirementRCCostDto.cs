namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Internal DTO for project-specific component charges (TestRequirementRCCost).
    /// Used as the service-layer transfer object between repository and API controller.
    /// Maps to fps.tbltestrequirementrccost (composite PK: TestCode + Buyer + ProfitCentre + FpsYear).
    /// </summary>
    public class TestRequirementRCCostDto
    {
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        public decimal Price { get; set; }
    }
}
