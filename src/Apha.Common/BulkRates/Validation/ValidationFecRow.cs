namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// A staged FEC row, in a shape neither Apha.FPS.Core.Entities.BulkRates.FecStagingRow
    /// nor Apha.BatchJobs.Domain.Entities.BulkRates.FecStagingRow can express directly —
    /// Apha.Common cannot see either project's own staging types, so each caller maps its
    /// native row into this before calling DR-VAL-01, and maps ValidationFinding back out.
    /// </summary>
    public sealed record ValidationFecRow
    {
        public required string TestCode { get; init; }
        public decimal? FecNewRate { get; init; }
        public string? ItemDescription { get; init; }
        public string? ShortDescription { get; init; }
        public string? Owner { get; init; }
        public string? Comments { get; init; }

        /// <summary>1-based worksheet row number (row 1 is the header), for finding attribution.</summary>
        public required int SourceRow { get; init; }
    }
}
