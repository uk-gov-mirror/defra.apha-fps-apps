using System.Text.Json.Serialization;

namespace Apha.Common.Utilities.EventPublisher
{
    public class EventDetail
    {
        [JsonPropertyName("jobExecutionId")]
        public string JobExecutionId { get; set; } = string.Empty;

        [JsonPropertyName("jobName")]
        public string JobName { get; set; } = string.Empty;

        [JsonPropertyName("runMode")]
        public string RunMode { get; set; } = string.Empty;

        [JsonPropertyName("requestedBy")]
        public string RequestedBy { get; set; } = string.Empty;     

        [JsonPropertyName("requestedAtUtc")]
        public DateTime RequestedAtUtc { get; set; }

        [JsonPropertyName("parametersJson")]
        public string? ParametersJson { get; set; }
    }
}
