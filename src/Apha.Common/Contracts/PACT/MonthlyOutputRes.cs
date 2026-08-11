namespace Apha.Common.Contracts.PACT
{
    public class MonthlyOutputRes
    {
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public double Month { get; set; }
        public string WorkGroup { get; set; } = null!;
        public double? Volume { get; set; }
        public string? WgBuyer { get; set; }
        public int FpsYear { get; set; }
    }
}
