namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// Read model for fps.job_queue augmented with job name and status name from joins.
    /// Used by the FPS API for all Bulk Rates lifecycle operations.
    /// </summary>
    public class BulkRatesQueueEntry
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
        public string? UploadFilename { get; set; }
        public string? UploadChecksumSha256 { get; set; }
        public int? UploadVersion { get; set; }
        public DateTime? UploadValidatedAtUtc { get; set; }
        public string? UploadRowCountsJson { get; set; }
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
}
