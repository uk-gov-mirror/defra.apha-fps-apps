using Apha.FPS.Core.Entities.BulkRates;

namespace Apha.FPS.Core.Interfaces
{
    public enum BulkRatesNotificationEvent
    {
        ReleasedForApproval,
        Rejected,
        Completed,
        Failed,
        Cancelled
    }

    public class BulkRatesNotificationContext
    {
        public Guid JobQueueId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public int FpsYear { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string? ApprovedBy { get; set; }
        public string? Reason { get; set; }
        public BulkRatesRowCounts? RowCounts { get; set; }
    }

    /// <summary>
    /// Contract for sending lifecycle notifications (release, rejection, completion, failure, cancellation).
    /// The stub (LogOnlyBulkRatesNotificationService) logs the event without sending any message.
    /// </summary>
    public interface IBulkRatesNotificationService
    {
        Task NotifyAsync(
            BulkRatesNotificationEvent notificationEvent,
            BulkRatesNotificationContext context,
            CancellationToken ct = default);
    }
}
