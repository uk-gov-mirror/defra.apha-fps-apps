namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// Staging row for fps.tblstagingtlkptestreqmt.
    /// Written by the API after parsing the AGRUP worksheet; read by both API and worker.
    /// PK: (jobqueueid, buyer, testcode).
    /// Parent FK: (jobqueueid, testcode) → tblstagingtestorproduct.
    /// </summary>
    public class AgrupStagingRow
    {
        public Guid JobQueueId { get; set; }
        public string TestCode { get; set; } = string.Empty;
        public string Buyer { get; set; } = string.Empty;
        public decimal? Agrup { get; set; }
        public decimal? AgrupNew { get; set; }
        public decimal? Change { get; set; }
        public double? NoRequired { get; set; }
        public DateTime? DateCreated { get; set; }
        public short? Active { get; set; }
        public string? Comments { get; set; }
        public string? ValidationComments { get; set; }
    }
}
