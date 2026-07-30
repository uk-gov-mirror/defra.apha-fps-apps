namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Internal DTO for component charges per profit centre (TestRCCost).
    /// Used as the service-layer transfer object between repository and API controller.
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
