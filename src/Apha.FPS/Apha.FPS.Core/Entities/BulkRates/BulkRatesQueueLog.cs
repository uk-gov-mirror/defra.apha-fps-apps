namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// Read model for fps.job_queue_log — one log entry per lifecycle transition or notable event.
    /// </summary>
    public class BulkRatesQueueLog
    {
        public long LogId { get; set; }
        public Guid JobQueueId { get; set; }
        public string Note { get; set; } = string.Empty;
        public string? Actor { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
