namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// A single FEC staging row for the "FEC Data (Staging)" grid.
    /// Status is DR-VAL-01's calculated action for this row — "No Change", "Insert",
    /// "Update", or "Zero-Rate Withdrawal" (frozen at release time, DR-API-07; computed live
    /// via the same shared validator before release) — or "Deleted" for a live TestCode
    /// absent from this upload entirely, a separate concept DR-VAL-01 has no row to classify.
    /// </summary>
    public class BulkRatesStagingFecRowDto
    {
        public string Status { get; set; } = string.Empty;
        public string TestCode { get; set; } = string.Empty;
        public decimal? UnitPriceVla { get; set; }
        public decimal? DefraUnitPrice { get; set; }
        public decimal? FecNewRate { get; set; }
        public string? ItemDescription { get; set; }
        public string? ShortDescription { get; set; }
        public string? Owner { get; set; }
        public string? Comments { get; set; }
    }
}
