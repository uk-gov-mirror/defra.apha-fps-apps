namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// POCO that maps to the JSON stored in fps.job_queue.configuration_json for Bulk Rates requests.
    /// JSON schema: { filename, checksum_sha256, upload_version, validation_completed_at_utc, row_counts:{...} }
    /// </summary>
    public class BulkRatesUploadMetadata
    {
        public string? Filename { get; set; }
        public string? ChecksumSha256 { get; set; }
        public int UploadVersion { get; set; }
        public DateTime? ValidationCompletedAtUtc { get; set; }
        public BulkRatesRowCounts RowCounts { get; set; } = new();
    }

    public class BulkRatesRowCounts
    {
        public int Total { get; set; }
        public int Valid { get; set; }
        public int Invalid { get; set; }
        public int Insert { get; set; }
        public int Update { get; set; }
        public int Unchanged { get; set; }
    }
}
