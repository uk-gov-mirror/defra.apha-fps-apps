namespace Apha.FPSApps.Application.Dtos.FPS
{
    // Same shape as backend DTO — all 41 columns from fps.project_log audit trail table
    public class ProjectLogDto
    {
        public int SequenceNo { get; set; }
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
        public decimal? CaseWorkSub { get; set; }
        public decimal? PvsIncome { get; set; }
        public decimal? PlanCaseWorkDebit { get; set; }
        public short? Finished { get; set; }
        public string? OwningRc { get; set; }
        public string? Comments { get; set; }
        public decimal? CarryOver { get; set; }
        public decimal? CarryOverSeed { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        public string JobCode { get; set; } = null!;
        public short? IsDefraProject { get; set; }
        public double? CostCentre { get; set; }
        public string? OracleProjectCode { get; set; }
        public string? SubAccountCode { get; set; }
        public string? ProjectGroup { get; set; }
        public string? IncomeAccountCode { get; set; }
        public int FpsYear { get; set; }
    }
}
