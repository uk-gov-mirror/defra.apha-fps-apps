namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Entity representing a project-specific component charge (per buyer/profit centre) for a test.
    /// Maps to fps.tbltestrequirementrccost
    /// (composite PK: TestCode + Buyer + ProfitCentre + FpsYear).
    /// The table is partitioned by fpsyear in PostgreSQL; EF maps to the parent table.
    /// </summary>
    public partial class TestRequirementRCCost
    {
        //   per pk_tbltestrequirementrccost constraint
        public string TestCode { get; set; } = null!;

        //   FK to fps.tlkptestreqmt(testcode, buyer, fpsyear)
        public string Buyer { get; set; } = null!;

        //   FK to fps.tbltestrccost(testcode, profitcentre, fpsyear)
        public string ProfitCentre { get; set; } = null!;

        public int FpsYear { get; set; }

        public decimal Price { get; set; }
    }
}
