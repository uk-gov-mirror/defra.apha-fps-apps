namespace Apha.PACT.Core.Entities
{
    public class TestActualBreakdownView
    {
        public string? Program { get; set; }
        public string Buyer { get; set; } = null!;
        public string? Portfolio { get; set; }
        public string? WorkGroup { get; set; }
        public string TestCode { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public int? Month { get; set; }
        public int FpsYear { get; set; }
        public decimal? PCPrice { get; set; }
        public decimal? PCCost { get; set; }
        public string? ProfitCentre { get; set; }
    }
}
