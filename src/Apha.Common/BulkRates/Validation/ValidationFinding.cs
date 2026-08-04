namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// Common result model, one per finding. The API validator,
    /// the worker's revalidation pass, and the UI all consume this
    /// shape rather than re-deriving classifications from raw staging rows.
    /// </summary>
    public sealed record ValidationFinding
    {
        public required string ValidationCode { get; init; }

        /// <summary>One of <see cref="ValidationSeverity"/>.</summary>
        public required string Severity { get; init; }

        /// <summary>"FEC" or "AGRUP" — the worksheet/category this finding belongs to.</summary>
        public required string Sheet { get; init; }

        /// <summary>e.g. a TestCode, or "TestCode/Buyer" for AGRUP rows. Null for findings with no single business key (e.g. unknown-job errors).</summary>
        public string? BusinessKey { get; init; }

        /// <summary>Staging column the finding is attributed to (e.g. "fecnewrate"), for inline UI error display — matches fps.staging_validation_error.fieldname. Null for row-level or request-level findings with no single field.</summary>
        public string? Field { get; init; }

        /// <summary>1-based upload worksheet row number. Null for request-level findings (reconciliation §2.6) or for findings about live/frozen data with no corresponding uploaded row.</summary>
        public int? SourceRow { get; init; }

        /// <summary>
        /// True when this finding has no single uploaded row to attach to (e.g. a downloaded
        /// key missing from the re-upload, reconciliation §2.6) and must be surfaced as a
        /// request-level message, not rendered like an ordinary per-row error.
        /// </summary>
        public bool IsRequestLevel { get; init; }

        public required string Message { get; init; }

        /// <summary>One of <see cref="ValidationCalculatedAction"/>. Null for findings that are not row classifications (e.g. duplicate-key errors).</summary>
        public string? CalculatedAction { get; init; }

        /// <summary>
        /// The value CalculatedAction implies applying — e.g. 0 for ZeroRateWithdrawal — so
        /// blank-to-zero handling has one unambiguous source. Null when
        /// CalculatedAction is null or the value is not yet determinable (e.g. a blocking
        /// Error on a new row with no rate supplied).
        /// </summary>
        public decimal? EffectiveNewRate { get; init; }
    }
}
