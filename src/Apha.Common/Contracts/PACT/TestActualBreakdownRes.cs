namespace Apha.Common.Contracts.PACT
{
    public class TestActualBreakdownRes
    {
        public string? Program { get; set; }
        public string Buyer { get; set; } = null!;
        public string? Portfolio { get; set; }
        public string? WorkGroup { get; set; }
        public string TestCode { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public int? Month { get; set; }
        public decimal? PCPrice { get; set; }
        public decimal? PCCost { get; set; }
        public string? ProfitCentre { get; set; }
        public decimal? Volume { get; set; }
    }
}
