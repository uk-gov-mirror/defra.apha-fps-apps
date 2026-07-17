namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>A single AGRUP staging row for the "Agrup Details" grid — diffed against live data per (TestCode, Buyer), same as FEC.</summary>
    public class BulkRatesStagingAgrupRowDto
    {
        public string Status { get; set; } = string.Empty;
        public string TestCode { get; set; } = string.Empty;
        public string Buyer { get; set; } = string.Empty;
        public decimal? Agrup { get; set; }
        public decimal? AgrupNew { get; set; }
        public double? NoRequired { get; set; }
        public DateTime? DateCreated { get; set; }
        public short? Active { get; set; }
        public string? Comments { get; set; }
    }
}
