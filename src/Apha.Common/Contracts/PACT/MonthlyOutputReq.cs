namespace Apha.Common.Contracts.PACT
{
    public class MonthlyOutputReq
    {
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public double Month { get; set; }
        public string WorkGroup { get; set; } = null!;
        public double? Volume { get; set; }

        public string? OriginalTestCode { get; set; }
        public string? OriginalBuyer { get; set; }
        public double? OriginalMonth { get; set; }
        public string? OriginalWorkGroup { get; set; }
    }
}
