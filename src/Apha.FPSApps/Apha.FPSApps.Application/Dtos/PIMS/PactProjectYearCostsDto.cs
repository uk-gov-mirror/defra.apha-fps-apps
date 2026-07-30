namespace Apha.FPSApps.Application.Dtos.PIMS
{
   
    public class PactProjectYearCostsDto
    {
        public string? Project { get; set; }
        public short Year { get; set; }
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
