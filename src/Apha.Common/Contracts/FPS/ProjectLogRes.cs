namespace Apha.Common.Contracts.FPS
{
    // Source: fps.project_log table + initializeProjectAuditTrailTable() in projectaudit_trail.js
    public class ProjectLogRes
    {
        public string ParentProject { get; set; } = null!;

        public string ProjectTitle { get; set; } = null!;

        public string Program { get; set; } = null!;

        public string Customer { get; set; } = null!;

        public string? Manager { get; set; }

        public decimal TransferIncome { get; set; }

        public decimal CustIncome { get; set; }

        public decimal? WipEoy { get; set; }

        public decimal? WipLimit { get; set; }

        public decimal? WipCurrent { get; set; }

        public string ProjectStatus { get; set; } = null!;

        public string? CostBookNo { get; set; }

        public DateTime? DateCreated { get; set; }

        public decimal? FecCost { get; set; }

        public decimal? Profit { get; set; }

        public decimal? BudgetCvl { get; set; }

        public DateTime? DateCosted { get; set; }

        public string Disease { get; set; } = null!;

        public string Contract { get; set; } = null!;

        public string? ProjectParent { get; set; }

        public string? ShortTitle { get; set; }

        public decimal? CaseworkSub { get; set; }

        public decimal? PvsIncome { get; set; }

        public decimal? PlanCaseworkDebit { get; set; }

        public short? Finished { get; set; }

        public string? OwningRc { get; set; }

        public string? Comments { get; set; }

        public decimal? CarryOver { get; set; }

        public decimal? CarryOverSeed { get; set; }

        public DateTime? DateTime { get; set; }

        public string? UserId { get; set; }

        // (legacy: resolved client-side via hardcoded auditUserEmailById map in projectaudit_trail.js)
        public string? UserEmail { get; set; }

        public string? InsertDelete { get; set; }
    }
}
