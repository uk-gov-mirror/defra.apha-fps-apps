namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class BatchJobEventTriggerDto
    {
        public BatchJobQueueDto Jobqueue { get; set; } = null!;
        public string EventId { get; set; } = string.Empty;
    }
}
