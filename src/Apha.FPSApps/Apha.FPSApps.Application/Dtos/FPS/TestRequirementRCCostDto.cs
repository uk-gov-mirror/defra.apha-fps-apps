namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for project-specific component charges (TestRequirementRCCost).
    /// Mirrors Apha.FPS.Application.Dtos.TestRequirementRCCostDto for use in the frontend
    /// application and infrastructure layers.
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
