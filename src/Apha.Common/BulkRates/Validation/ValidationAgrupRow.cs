namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// A staged AGRUP row. See ValidationFecRow for why this neutral shape exists.
    ///
    /// TestBuyerCode vs TestBuyerWorkGroup: the live
    /// fps.tlkptestreqmt.testbuyercode column stores a single string whose exact
    /// TestCode+WorkGroup concatenation rule is unconfirmed (reconciliation §2.4) — the shared
    /// validator never derives or produces that value. Instead:
    ///   - TestBuyerCode carries the raw existing value, used only to detect whether an
    ///     existing row's protected routing cell was changed (immutability).
    ///   - TestBuyerWorkGroup carries the real, separately-captured WorkGroup value used for
    ///     new-row "at least one routing field populated" and capability-existence validation
    ///     — never a concatenation the caller had to construct.
    /// </summary>
    public sealed record ValidationAgrupRow
    {
        public required string TestCode { get; init; }
        public required string Buyer { get; init; }
        public decimal? AgrupNew { get; init; }
        public string? ProjectBuyerCode { get; init; }
        public string? TestBuyerCode { get; init; }
        public string? TestBuyerWorkGroup { get; init; }
        public string? Comments { get; init; }

        /// <summary>1-based worksheet row number (row 1 is the header), for finding attribution.</summary>
        public required int SourceRow { get; init; }
    }
}
