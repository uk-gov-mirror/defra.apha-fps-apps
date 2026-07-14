namespace Apha.Common.Contracts.FPS
{
    public class ContributionSummaryTotalsRes
    {
        public string SellingPc { get; set; } = null!;

        public decimal? ContTarget { get; set; }

        public decimal? SumOfGenBid { get; set; }

        public decimal TotalFec { get; set; }

        public decimal TotalContribution { get; set; }

        public decimal TotalAppFec { get; set; }

        public decimal TotalToRecover { get; set; }

        /// <summary>Surplus/Shortfall (Total Time panel) = TotalFec - TotalToRecover + AnimalCosts.</summary>
        public decimal Surplus { get; set; }

        /// <summary>Surplus/Shortfall (Assured Time panel) = TotalAppFec - TotalToRecover.</summary>
        public decimal AssuredSurplus { get; set; }

        /// <summary>Total animal costs — non-zero only for SellingPC = "ASU".</summary>
        public decimal AnimalCosts { get; set; }

        /// <summary>True when SellingPC = "ASU" and animal costs are included.</summary>
        public bool IsAsuMode { get; set; }
    }
}
