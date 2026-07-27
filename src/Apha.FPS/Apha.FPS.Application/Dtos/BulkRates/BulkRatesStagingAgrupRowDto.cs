namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>A single AGRUP staging row for the "Agrup Details" grid. See <see cref="BulkRatesStagingFecRowDto"/> for what Status means.</summary>
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
