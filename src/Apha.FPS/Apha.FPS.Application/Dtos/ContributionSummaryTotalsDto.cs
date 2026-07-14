namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// DTO for the footer totals section of the Income/Contribution from Time Sales form (frmTimeSellerPC).
    /// </summary>
    public class ContributionSummaryTotalsDto
    {
        /// <summary>Selling profit centre code.</summary>
        public string SellingPc { get; set; } = null!;

        /// <summary>Contribution target for this profit centre (from tblkpprofitcentre.conttarget).</summary>
        public decimal? ContTarget { get; set; }

        /// <summary>Total general budget bids for this profit centre (Sum(GenBid) from tblbid).</summary>
        public decimal? SumOfGenBid { get; set; }

        /// <summary>Sum of FEC across all rows: Sum(Hrs * ChargeRate).</summary>
        public decimal TotalFec { get; set; }

        /// <summary>Sum of Contribution across all rows: Sum(OHR * Hrs).</summary>
        public decimal TotalContribution { get; set; }

        /// <summary>Sum of Assured FEC across all rows: Sum(AppHours * ChargeRate).</summary>
        public decimal TotalAppFec { get; set; }

        /// <summary>Total to Recover = ContTarget + SumOfGenBid.</summary>
        public decimal TotalToRecover { get; set; }

        /// <summary>
        /// Surplus/Shortfall (Total Time panel) = TotalFec - TotalToRecover + AnimalCosts.
        /// AnimalCosts is non-zero only for SellingPC = "ASU".
        /// </summary>
        public decimal Surplus { get; set; }

        /// <summary>
        /// Surplus/Shortfall (Assured Time panel) = TotalAppFec - TotalToRecover.
        /// </summary>
        public decimal AssuredSurplus { get; set; }

        /// <summary>
        /// Total animal costs for the year — non-zero only when SellingPC = "ASU".
        /// </summary>
        public decimal AnimalCosts { get; set; }

        /// <summary>True when SellingPC = "ASU" and animal costs are included in the surplus calculation.</summary>
        public bool IsAsuMode { get; set; }
    }
}
