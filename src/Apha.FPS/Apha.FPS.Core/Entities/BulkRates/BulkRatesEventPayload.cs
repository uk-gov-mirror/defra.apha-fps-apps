namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// EventBridge event payload published by the API when a Bulk Rates request is approved.
    /// The worker treats this as a trigger only; the authoritative data source is fps.job_queue.
    /// </summary>
    public class BulkRatesEventPayload
    {
        public Guid JobExecutionId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public string RunMode { get; set; } = "Manual";
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestedAtUtc { get; set; }
        public BulkRatesEventParameters ParametersJson { get; set; } = new();
    }

    public class BulkRatesEventParameters
    {
        public int Year { get; set; }
    }
}
