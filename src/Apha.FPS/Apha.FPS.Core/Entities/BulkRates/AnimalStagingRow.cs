namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// Staging row for fps.tblstaginganimals.
    /// Written by the API after parsing the Animal rates upload.
    /// PK: (jobqueueid, animaltype).
    /// </summary>
    public class AnimalStagingRow
    {
        public Guid JobQueueId { get; set; }
        public string AnimalType { get; set; } = string.Empty;
        public string? Species { get; set; }
        public string? SecurityLevel { get; set; }
        public decimal? DailyRate { get; set; }
        public decimal? DefraDailyRate { get; set; }
        public bool? PlanByWeek { get; set; }
        public string? ValidationComments { get; set; }
    }
}
