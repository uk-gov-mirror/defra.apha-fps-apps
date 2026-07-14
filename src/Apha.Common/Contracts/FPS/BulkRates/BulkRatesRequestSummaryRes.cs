namespace Apha.Common.Contracts.FPS.BulkRates
{
    public class BulkRatesRequestSummaryRes
    {
        public Guid JobQueueId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public int FpsYear { get; set; }
        public string Status { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestedAtUtc { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string? RejectedBy { get; set; }
        public string? RejectionReason { get; set; }
        public string? CancelledBy { get; set; }
    }
}
