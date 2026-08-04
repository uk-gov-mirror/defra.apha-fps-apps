namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// Staging row for fps.tblstagingprofitcentregrade.
    /// Written by the API after parsing the Staff rates upload.
    /// PK: (jobqueueid, pcgrade).
    /// </summary>
    public class StaffStagingRow
    {
        public Guid JobQueueId { get; set; }
        public string PcGrade { get; set; } = string.Empty;
        public decimal? PayRate { get; set; }
        public decimal? Npr { get; set; }
        public decimal? Ohr { get; set; }
        public string? ValidationComments { get; set; }
    }
}
