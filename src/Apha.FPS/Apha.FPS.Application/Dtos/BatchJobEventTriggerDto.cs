namespace Apha.FPS.Application.Dtos
{
    public class BatchJobEventTriggerDto
    {
        public BatchJobQueueDto Jobqueue { get; set; } = null!;
        public string EventId { get; set; } = string.Empty;
    }
}
