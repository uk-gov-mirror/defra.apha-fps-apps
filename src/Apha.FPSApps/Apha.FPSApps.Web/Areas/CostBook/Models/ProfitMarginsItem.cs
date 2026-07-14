using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;
public class ProfitMarginsItem
{
    
    [Required(ErrorMessage = "Animals profit margin is required.")]
    [Display(Name = "Animals Profit (%)")]
    public decimal ProfitAnimals { get; set; }

    
    [Required(ErrorMessage = "Exceptional Costs profit margin is required.")]
    [Display(Name = "Exceptional Costs Profit (%)")]
    public decimal ProfitExceptionalCosts { get; set; }

   
    [Required(ErrorMessage = "Staff profit margin is required.")]
    [Display(Name = "Staff Profit (%)")]
    public decimal ProfitStaff { get; set; }

    
    [Required(ErrorMessage = "Tests profit margin is required.")]
    [Display(Name = "Tests Profit (%)")]
    public decimal ProfitTests { get; set; }
}
