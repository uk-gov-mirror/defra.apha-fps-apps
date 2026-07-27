namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// The current fps.tlkptestreqmt row for a TestCode+Buyer, as known at the moment
    /// ValidationContext was built. TestBuyerCode is the raw live column value (see
    /// ValidationAgrupRow) — there is no separate live WorkGroup to compare against.
    /// </summary>
    public sealed record LiveAgrupRow
    {
        public required string TestCode { get; init; }
        public required string Buyer { get; init; }
        public decimal? UnitPrice { get; init; }
        public string? ProjectBuyerCode { get; init; }
        public string? TestBuyerCode { get; init; }
        /// <summary>Carried for testreq_log final-row-image writes (DR-WK-testreq).</summary>
        public double? NoRequired { get; init; }
        /// <summary>Carried for testreq_log final-row-image writes (DR-WK-testreq).</summary>
        public short? Active { get; init; }
    }
}
