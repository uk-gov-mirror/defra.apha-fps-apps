namespace Apha.Common.Contracts.FPS
{
    // Source: fps.additionalcosts_log table + initializeExceptionalCostChangesTable() in projectaudit_trail.js
    public class AdditionalCostLogRes
    {
        public string JobCode { get; set; } = null!;

        public string Account { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal ItemCost { get; set; }

        public string? Freq { get; set; }

        public string? Supplier { get; set; }

        public DateTime? DateTime { get; set; }

        public string? UserId { get; set; }

        public string? UserEmail { get; set; }

        public string? InsertDelete { get; set; }
    }
}
