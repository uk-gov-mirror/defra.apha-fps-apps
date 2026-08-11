namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class ProjectSnapshotViewDto
    {
        public string ParentProject { get; set; } = null!;
        public string ProjectTitle { get; set; } = null!;
        public string? Program { get; set; }
        public string? Customer { get; set; }
        public string? Manager { get; set; }
        public decimal TransferIncome { get; set; }
        public decimal? BudgetCvl { get; set; }
        public decimal? BudgetExt { get; set; }
        public decimal CustIncome { get; set; }
        public decimal? PvsIncome { get; set; }
        public decimal? WipEoy { get; set; }
        public decimal? WipLimit { get; set; }
        public decimal? WipCurrent { get; set; }
        public decimal? FecCost { get; set; }
        public string? ProjectStatus { get; set; }
        public decimal? Profit { get; set; }
        public string? Disease { get; set; }
        public string? Contract { get; set; }
        public decimal? CaseWorkSub { get; set; }
        public decimal? PlanCaseWorkDebit { get; set; }
    }
}
