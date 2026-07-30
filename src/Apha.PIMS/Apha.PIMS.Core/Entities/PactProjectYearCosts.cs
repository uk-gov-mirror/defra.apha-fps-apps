namespace Apha.PIMS.Core.Entities
{
    public class PactProjectYearCosts
    {       
        public string Project { get; set; } = null!;
        public double Year { get; set; }
        public double MonthNo { get; set; }
        public decimal? SubContracts { get; set; }
        public decimal? Animals { get; set; }
        public decimal? Tests { get; set; }
        public decimal? Pay { get; set; }
        public decimal? NonPayOH { get; set; }
        public decimal? TotalCosts { get; set; }
        public decimal? TimeCost { get; set; }
        public double? Hours { get; set; }
        public decimal? CustIncome { get; set; }
        public decimal? BudgetCvl { get; set; }
    }
}
