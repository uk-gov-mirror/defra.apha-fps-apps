namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// Staging row for fps.tblstagingtestorproduct.
    /// Written by the API after parsing the FEC Excel upload; read by both API and worker.
    /// PK: (jobqueueid, testcode).
    /// </summary>
    public class FecStagingRow
    {
        public Guid JobQueueId { get; set; }
        public string TestCode { get; set; } = string.Empty;
        public decimal? UnitPriceVla { get; set; }
        public decimal? DefraUnitPrice { get; set; }
        public decimal? FecNewRate { get; set; }
        public decimal? Change { get; set; }
        public string? ItemDescription { get; set; }
        public string? ShortDescription { get; set; }
        public string? Owner { get; set; }
        public string? Comments { get; set; }
        public string? ValidationComments { get; set; }
    }
}
