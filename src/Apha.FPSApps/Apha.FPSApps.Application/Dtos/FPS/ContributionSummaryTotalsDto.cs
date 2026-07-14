namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// DTO for the footer totals section of the Income/Contribution from Time Sales form (frmTimeSellerPC).
    /// </summary>
    public class ContributionSummaryTotalsDto
    {
        public string SellingPc { get; set; } = null!;
        public decimal? ContTarget { get; set; }
        public decimal? SumOfGenBid { get; set; }
        public decimal TotalFec { get; set; }
        public decimal TotalContribution { get; set; }
        public decimal TotalAppFec { get; set; }

        /// <summary>Total To Recover = ContTarget + SumOfGenBid.</summary>
        public decimal TotalToRecover { get; set; }

        /// <summary>Surplus/Shortfall (Total Time) = TotalFec - TotalToRecover + AnimalCosts.</summary>
        public decimal Surplus { get; set; }

        /// <summary>Surplus/Shortfall (Assured Time) = TotalAppFec - TotalToRecover.</summary>
        public decimal AssuredSurplus { get; set; }

        /// <summary>Non-zero only when SellingPC = "ASU".</summary>
        public decimal AnimalCosts { get; set; }

        /// <summary>True when SellingPC = "ASU" and animal costs are included.</summary>
        public bool IsAsuMode { get; set; }
    }
}
