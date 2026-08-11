namespace Apha.Common.Contracts.FPS
{
    public class ProjectRes
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
        public string? CostBookNo { get; set; }
        public decimal? Profit { get; set; }
        public string? Disease { get; set; }
        public string? Contract { get; set; }
        public string? ShortTitle { get; set; }
        public string? ProjectParent { get; set; }
        public short? Finished { get; set; }
        public string? Comments { get; set; }
        public decimal? CarryOver { get; set; }
        public decimal? CarryOverSeed { get; set; }
        public short IsDefraProject { get; set; }
        public double? CostCentre { get; set; }
        public string? OwningRc { get; set; }
        public string? OracleProjectCode { get; set; }
        public string? SubAccountCode { get; set; }
        public string? ProjectGroup { get; set; }
        public decimal? PlanCaseWorkDebit { get; set; }
        public string? IncomeAccountCode { get; set; }
        public decimal? CaseWorkSub { get; set; }

    }
}
