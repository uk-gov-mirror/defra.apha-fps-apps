namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for component charges per profit centre (TestRCCost).
    /// Mirrors Apha.FPS.Application.Dtos.TestRCCostDto for use in the frontend
    /// application and infrastructure layers.
    /// Maps to fps.tbltestrccost (composite PK: TestCode + ProfitCentre + FpsYear).
    /// </summary>
    public class TestRCCostDto
    {
        public string TestCode { get; set; } = null!;
        public string ProfitCentre { get; set; } = null!;
        public int FpsYear { get; set; }

        public decimal Price { get; set; }
    }
}
