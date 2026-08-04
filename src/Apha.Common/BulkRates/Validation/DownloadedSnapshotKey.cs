namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// One row from fps.bulk_rates_downloaded_key for the request's active
    /// download version — the immutable record of what was actually downloaded, never a live
    /// requery (reconciliation §2.6).
    /// </summary>
    public sealed record DownloadedSnapshotKey
    {
        /// <summary>"FEC" or "AGRUP".</summary>
        public required string Sheet { get; init; }

        public required string TestCode { get; init; }

        /// <summary>Null for FEC rows.</summary>
        public string? Buyer { get; init; }

        public decimal? SourceRate { get; init; }
    }
}
