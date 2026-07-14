namespace Apha.FPS.Core.Entities
{
    public partial class ContributionSummaryView
    {
        public decimal? ContTarget { get; set; }

        public string SellingPc { get; set; } = null!;

        public decimal? ChargeRate { get; set; }

        public decimal? Ohr { get; set; }

        public decimal? SumOfGenBid { get; set; }

        public string? WorkGroup { get; set; }

        public string? ProfitCentreGrade { get; set; }

        public string? WgGrade { get; set; }

        public double? AppHours { get; set; }

        public double? Hrs { get; set; }

        public double? AvHrs { get; set; }

        public decimal? Fec { get; set; }

        public decimal? AppFec { get; set; }

        public decimal? Contribution { get; set; }

        public int FpsYear { get; set; }

        public int? UserId { get; set; }

        public string? Dt2Username { get; set; }

        public string? UserEmail { get; set; }
    }
}
