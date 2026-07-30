namespace Apha.PACT.Application.Dtos
{
    public class TestReqBreakdownDto
    {
        public string TestCode { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public string? Program { get; set; }
        public string Project { get; set; } = null!;
        public string? Pc { get; set; }
        public string? WorkG { get; set; }
        public decimal? WgPrice { get; set; }
        public decimal? TotalCost { get; set; }
        public int FpsYear { get; set; }
    }
}
