namespace Apha.Common.Contracts.FPS.BulkRates
{
    public class BulkRatesRequestDetailRes
    {
        public Guid JobQueueId { get; set; }
        public Guid JobExecutionId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public int FpsYear { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestedAtUtc { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedAtUtc { get; set; }
        public string? RejectionReason { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? CancelledAtUtc { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? ErrorMessage { get; set; }
        public BulkRatesUploadMetadataRes? UploadMetadata { get; set; }
        public IReadOnlyList<BulkRatesLogEntryRes> Log { get; set; } = [];
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
    }

    public class BulkRatesUploadMetadataRes
    {
        public string? Filename { get; set; }
        public int UploadVersion { get; set; }
        public DateTime? ValidationCompletedAtUtc { get; set; }
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public int InsertRows { get; set; }
        public int UpdateRows { get; set; }
        public int UnchangedRows { get; set; }
    }

    public class BulkRatesLogEntryRes
    {
        public long LogId { get; set; }
        public string Note { get; set; } = string.Empty;
        public string? Actor { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
