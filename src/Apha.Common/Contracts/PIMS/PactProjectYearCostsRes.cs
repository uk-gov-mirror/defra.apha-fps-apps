
namespace Apha.Common.Contracts.PIMS
{
    public class PactProjectYearCostsRes
    {
        public string? Project { get; set; }
        public short Year { get; set; }

        public decimal? SubContracts { get; set; }          // sum(subcontracts)
        public decimal? Animals { get; set; }               // sum(animals) — maps to AnimalCosts in main record
        public decimal? Tests { get; set; }                 // sum(transfercosts) — maps to TestCosts in main record
        public decimal? Pay { get; set; }                   // sum(vtcc_summary.pay) — maps to PayCosts in main record
        public decimal? NonPayOH { get; set; }              // sum(nonpay+overhead) — maps to NonPayOhCosts in main record
        public decimal? TotalCosts { get; set; }            // sum(totalcost) — used by btnFixCosting to set ActualExpenditure
        public decimal? TimeCost { get; set; }             // sum(timecosts) — display only
       
        public double? Hours { get; set; }                 // sum(totalhours) — maps to ManHours; ManDays/ManYears derived
      
        public decimal? CustIncome { get; set; }            // MY_tlkpProject.CustIncome — display label: "Customer Income"
        public decimal? BudgetCvl { get; set; }             // MY_tlkpProject.Budget_CVL — display label: "VLA Budget"
    }
}
