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

        // ── Routing fields. On a staged (uploaded) row these are
        // whatever the workbook supplied for this upload (null until the workbook adds
        // support for it); on a live row (GetAgrupRowsForExportAsync) these are the current
        // fps.tlkptestreqmt values. TestBuyerWorkGroup has no live counterpart — the live
        // column is the single testbuyercode string (concatenation rule unconfirmed)
        // — so it is only ever populated on staged rows. ─────────────────────────
        public string? ProjectBuyerCode { get; set; }
        public string? TestBuyerCode { get; set; }
        public string? TestBuyerWorkGroup { get; set; }

        // ── Frozen at release time, compared against the worker's
        // re-derived result for drift detection. Null until release. ──────────
        public string? CalculatedAction { get; set; }
        public decimal? EffectiveNewRate { get; set; }
        public decimal? SourceCurrentRate { get; set; }
        public int? ValidationVersion { get; set; }
    }
}
