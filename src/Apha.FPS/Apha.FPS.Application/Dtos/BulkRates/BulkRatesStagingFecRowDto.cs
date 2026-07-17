namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// A single FEC staging row for the "FEC Data (Staging)" grid, classified against
    /// the live fps.testorproduct data for the request's FPS year.
    /// Status is one of: Inserted (TestCode not in live data), Updated (TestCode exists
    /// and the staged FecNewRate differs from the live DefraUnitPrice), or Deleted
    /// (TestCode exists live but is absent from this upload). Unchanged rows are omitted.
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
