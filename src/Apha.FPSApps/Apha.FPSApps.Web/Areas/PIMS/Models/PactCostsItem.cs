using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class PactCostsItem
    {
       
        public string? Project { get; set; }
        public short Year { get; set; }

        [Display(Name = "Project Specific")]
        public decimal? SubContracts { get; set; }

      
        [Display(Name = "Animal")]
        public decimal? Animals { get; set; }

       
        [Display(Name = "Test")]
        public decimal? Tests { get; set; }

        
        [Display(Name = "Pay")]
        public decimal? Pay { get; set; }

       
        [Display(Name = "NonPayOH")]
        public decimal? NonPayOH { get; set; }

        
        [Display(Name = "Total Costs")]
        public decimal? TotalCosts { get; set; }

        
        [Display(Name = "Total Time Costs")]
        public decimal? TimeCost { get; set; }

       
        [Display(Name = "Man Hours")]
        public double? Hours { get; set; }

        
        [Display(Name = "Customer Income")]
        public decimal? CustIncome { get; set; }

        
        [Display(Name = "VLA Budget")]
        public decimal? BudgetCvl { get; set; }
    }
}
