namespace Apha.Common.Contracts.FPS.BulkRates
{
    /// <summary>
    /// Returned by the Upload endpoint after parsing, validation and staging replace.
    /// </summary>
    public class BulkRatesUploadRes
    {
        public Guid JobQueueId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int UploadVersion { get; set; }
        public string? Filename { get; set; }
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public int InsertRows { get; set; }
        public int UpdateRows { get; set; }
        public int UnchangedRows { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public bool CanRelease { get; set; }
    }
}
