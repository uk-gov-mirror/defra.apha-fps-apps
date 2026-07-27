namespace Apha.Common.BulkRates.Validation
{
    /// <summary>The current fps.testorproduct row for a TestCode, as known at the moment ValidationContext was built.</summary>
    public sealed record LiveFecRow
    {
        public required string TestCode { get; init; }
        public decimal? UnitPriceVla { get; init; }
        public decimal? DefraUnitPrice { get; init; }
    }
}
