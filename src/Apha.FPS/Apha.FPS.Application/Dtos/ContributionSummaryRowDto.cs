namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// DTO for a single row in the Income/Contribution from Time Sales grid (frmTimeSellerPC).
    /// </summary>
    public class ContributionSummaryRowDto
    {
        public string? WgGrade { get; set; }

        public string? WorkGroup { get; set; }

        public string? ProfitCentreGrade { get; set; }

        public double? Hrs { get; set; }

        public double? AvHrs { get; set; }

        public decimal? ChargeRate { get; set; }

        public decimal? Ohr { get; set; }

        public decimal? Fec { get; set; }

        public decimal? Contribution { get; set; }

        public double? AppHours { get; set; }

        public decimal? AppFec { get; set; }

        /// <summary>
        /// % Planned = Hrs / AvHrs. Null when AvHrs is zero (form showed "!" in that case).
        /// </summary>
        public double? PctPlanned { get; set; }

        /// <summary>
        /// % Assured Planned = AppHours / AvHrs. Null when AvHrs is zero.
        /// </summary>
        public double? PctAssuredPlanned { get; set; }
    }
}
