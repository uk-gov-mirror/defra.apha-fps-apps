namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// Upload/validation summary for a Bulk Rates request, assembled from the typed
    /// upload_* columns on fps.job_queue (configuration_json retired).
    /// RowCounts is the one field still backed by jsonb (upload_row_counts_json),
    /// since it's a purely descriptive nested breakdown with no business-gating role.
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
