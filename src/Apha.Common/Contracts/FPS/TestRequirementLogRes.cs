namespace Apha.Common.Contracts.FPS
{
    // Source: fps.testreq_log table + initializeTestRequirementChangesTable() in projectaudit_trail.js
    public class TestRequirementLogRes
    {
        public string? TestCode { get; set; }

        public string? Buyer { get; set; }

        public double? UnitPrice { get; set; }

        public int? NoRequired { get; set; }

        public string? ProjectBuyerCode { get; set; }

        public string? TestBuyerCode { get; set; }

        public short? Active { get; set; }

        public DateTime? DateTime { get; set; }

        public string? UserId { get; set; }

        public string? UserEmail { get; set; }

        public string? InsertDelete { get; set; }
    }
}
