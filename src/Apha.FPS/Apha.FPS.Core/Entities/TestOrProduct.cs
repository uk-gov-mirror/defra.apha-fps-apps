namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Entity representing a test or product master record.
    /// Maps to fps.testorproduct (composite PK: ItemCode + FpsYear).
    /// The table is partitioned by fpsyear in PostgreSQL; EF maps to the parent table.
    /// </summary>
    public partial class TestOrProduct
    {
        public string ItemCode { get; set; } = null!;

        public int FpsYear { get; set; }

        public string? ItemDescription { get; set; }

        public string? TestManager { get; set; }

        public string? JobStatus { get; set; }

        public decimal? UnitPriceVla { get; set; }

        public decimal? PriceAhvg { get; set; }

        //   CHECK constraint (owner IN ('PT','PA','SD','LT')) enforced at service layer
        public string? Owner { get; set; }

        public string? ChargeMethod { get; set; }

        public string? ShortDescription { get; set; }

        public decimal DefraUnitPrice { get; set; }
    }
}
