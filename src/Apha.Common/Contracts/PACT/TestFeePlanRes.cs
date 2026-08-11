namespace Apha.Common.Contracts.PACT
{
    public class TestFeePlanRes
    {
        public string? Version { get; set; }
        public string? Directorate { get; set; }
        public string? Customer { get; set; }
        public string? Program { get; set; }
        public string? Contract { get; set; }
        public string? Project { get; set; }
        public string? Status { get; set; }
        public string TestCode { get; set; } = null!;
        public decimal? UnitPrice { get; set; }
        public double? NoTests { get; set; }
        public double? TestFee { get; set; }
        public string? Owner { get; set; }
    }
}
