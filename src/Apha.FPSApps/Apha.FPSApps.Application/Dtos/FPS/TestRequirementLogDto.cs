namespace Apha.FPSApps.Application.Dtos.FPS
{
    // Same shape as backend DTO — all 13 columns from fps.testreq_log audit trail table
    public class TestRequirementLogDto
    {
        public int SequenceNo { get; set; }
        public string? TestCode { get; set; }
        public string? Buyer { get; set; }
        public decimal? UnitPrice { get; set; }
        public double? NoRequired { get; set; }
        public string? ProjectBuyerCode { get; set; }
        public string? TestBuyerCode { get; set; }
        public short? Active { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        public string? JobCode { get; set; }
        public int FpsYear { get; set; }
    }
}
