namespace Apha.FPSApps.Application.Dtos.FPS.BulkRates
{
    /// <summary>
    /// Web-side DTO matching the JSON shape returned by the FPS API Bulk Rates endpoints
    /// (create, release, approve, reject, cancel, get). Mirrors <c>BulkRatesRequestDto</c>
    /// as serialised by the FPS API action filter into <c>ApiResponse&lt;T&gt;.Data</c>.
    /// </summary>
    public class BulkRatesRequestDetailDto
    {
        public BulkRatesQueueEntryDto Entry { get; set; } = new();
        public BulkRatesUploadMetadataDto? UploadMetadata { get; set; }
        public List<BulkRatesQueueLogDto> Log { get; set; } = [];
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
    }

    /// <summary>
    /// Mirrors <c>BulkRatesQueueEntry</c> (Apha.FPS.Core) as serialised over the wire.
    /// </summary>
    public class BulkRatesQueueEntryDto
    {
        public Guid JobQueueId { get; set; }
        public int JobId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid JobExecutionId { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestedAtUtc { get; set; }
        public int FpsYear { get; set; }
        public string? ConfigurationJson { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedAtUtc { get; set; }
        public string? RejectionReason { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? CancelledAtUtc { get; set; }
        public string? CancellationReason { get; set; }
        public string? TriggeredBy { get; set; }
        public DateTime? TriggeredAtUtc { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FailureReason { get; set; }
    }

    /// <summary>
    /// Mirrors <c>BulkRatesUploadMetadata</c> as serialised over the wire.
    /// </summary>
    public class BulkRatesUploadMetadataDto
    {
        public string? Filename { get; set; }
        public string? ChecksumSha256 { get; set; }
        public int UploadVersion { get; set; }
        public DateTime? ValidationCompletedAtUtc { get; set; }
        public BulkRatesRowCountsDto RowCounts { get; set; } = new();
    }

    public class BulkRatesRowCountsDto
    {
        public int Total { get; set; }
        public int Valid { get; set; }
        public int Invalid { get; set; }
        public int Insert { get; set; }
        public int Update { get; set; }
        public int Unchanged { get; set; }
    }

    /// <summary>
    /// Mirrors <c>BulkRatesQueueLog</c> as serialised over the wire.
    /// </summary>
    public class BulkRatesQueueLogDto
    {
        public long LogId { get; set; }
        public Guid JobQueueId { get; set; }
        public string Note { get; set; } = string.Empty;
        public string? Actor { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
