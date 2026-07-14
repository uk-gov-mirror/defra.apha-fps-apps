namespace Apha.Common.Contracts.FPS
{
    public class ContributionSummaryRowRes
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

        /// <summary>Hrs / AvHrs. Null when AvHrs is zero (rendered as "!" in the original form).</summary>
        public double? PctPlanned { get; set; }

        /// <summary>AppHours / AvHrs. Null when AvHrs is zero.</summary>
        public double? PctAssuredPlanned { get; set; }
    }
}
