namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// Input. Everything the shared validator needs, pre-fetched by
    /// the caller — the service itself is side-effect free and does no I/O, so the same
    /// context produces the same result whether built by the API or the worker (determinism).
    /// Lookups must be built via <see cref="BulkRatesValidationKeys"/> so key
    /// normalization matches between context construction and validation.
    /// </summary>
    public sealed record ValidationContext
    {
        public required Guid JobQueueId { get; init; }
        public required int FpsYear { get; init; }

        /// <summary>Null when no download has happened yet for this request.</summary>
        public int? DownloadVersion { get; init; }

        public required int UploadVersion { get; init; }

        /// <summary>Key: <see cref="BulkRatesValidationKeys.TestCode"/>. Every live fps.testorproduct row for TestCodes relevant to this upload.</summary>
        public required IReadOnlyDictionary<string, LiveFecRow> LiveFecLookup { get; init; }

        /// <summary>Key: <see cref="BulkRatesValidationKeys.AgrupKey"/>.</summary>
        public required IReadOnlyDictionary<(string TestCode, string Buyer), LiveAgrupRow> LiveAgrupLookup { get; init; }

        /// <summary>Existing fps.tlkpproject.parentproject codes for FpsYear, keyed by <see cref="BulkRatesValidationKeys.TestCode"/> (reused for any single-string business code).</summary>
        public required IReadOnlySet<string> ProjectLookup { get; init; }

        /// <summary>Existing fps.tlkptestcapability (TestCode, WorkGroup) pairs for FpsYear. Key: <see cref="BulkRatesValidationKeys.CapabilityKey"/>.</summary>
        public required IReadOnlySet<(string TestCode, string WorkGroup)> CapabilityLookup { get; init; }

        public required IReadOnlyList<ValidationFecRow> StagedFecRows { get; init; }
        public required IReadOnlyList<ValidationAgrupRow> StagedAgrupRows { get; init; }

        /// <summary>fps.bulk_rates_downloaded_key rows for DownloadVersion. Empty (not null) when DownloadVersion is null.</summary>
        public required IReadOnlyList<DownloadedSnapshotKey> FrozenSnapshot { get; init; }

        /// <summary>
        /// Gates the BC-05 live-vs-snapshot-absence check — a live positive AGRUP row,
        /// under a FEC TestCode being withdrawn, that was never part of the download snapshot.
        /// The API's own release-time check is deliberately scoped to snapshot-known rows only
        /// ("a live AGRUP row created after download... cannot be caught here — that gap is
        /// the worker's job"): a row invisible at download time is also invisible to whoever is
        /// reviewing the release, so blocking release over it would be unreviewable, not just
        /// unhelpful. Default false — the API's release-time call must leave this off so an
        /// Error-severity finding here can never surface where that check says it shouldn't. The
        /// worker's revalidation pass (run inside the transaction against locked rows)
        /// sets this true — that is exactly the point in the lifecycle where this gap must
        /// be caught and the request failed.
        /// </summary>
        public bool IncludeWorkerOnlyChecks { get; init; }
    }
}
