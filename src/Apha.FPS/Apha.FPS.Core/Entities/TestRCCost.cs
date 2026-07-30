namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Entity representing a component charge per profit centre for a test/product.
    /// Maps to fps.tbltestrccost (composite PK: TestCode + ProfitCentre + FpsYear).
    /// The table is partitioned by fpsyear in PostgreSQL; EF maps to the parent table.
    /// </summary>
    public partial class TestRCCost
    {
        public string TestCode { get; set; } = null!;

        public string ProfitCentre { get; set; } = null!;

        public int FpsYear { get; set; }

        public decimal Price { get; set; }
    }
}
