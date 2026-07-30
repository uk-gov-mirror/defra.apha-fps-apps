using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

//   Maps to MaintenanceSettingsDto inflation sub-set (InflationAnimals, InflationExceptionalCosts,
//   InflationStaff, InflationTests, CurrentFinancialYear, WorkingHoursInDay, WorkingDaysInYear)
public class InflationSettingsItem
{
    [Required(ErrorMessage = "Animals inflation rate is required.")]
    [Display(Name = "Animals Inflation (%)")]
    public decimal InflationAnimals { get; set; }

    [Required(ErrorMessage = "Exceptional Costs inflation rate is required.")]
    [Display(Name = "Exceptional Costs Inflation (%)")]
    public decimal InflationExceptionalCosts { get; set; }

    [Required(ErrorMessage = "Staff inflation rate is required.")]
    [Display(Name = "Staff Inflation (%)")]
    public decimal InflationStaff { get; set; }

    [Required(ErrorMessage = "Tests inflation rate is required.")]
    [Display(Name = "Tests Inflation (%)")]
    public decimal InflationTests { get; set; }

    [Required(ErrorMessage = "Current Financial Year is required.")]
    [Display(Name = "Current Financial Year")]
    public int CurrentFinancialYear { get; set; }

    [Required(ErrorMessage = "Working Hours in Day is required.")]
    [Display(Name = "Working Hours in Day")]
    public decimal WorkingHoursInDay { get; set; }

    [Required(ErrorMessage = "Working Days in Year is required.")]
    [Display(Name = "Working Days in Year")]
    public decimal WorkingDaysInYear { get; set; }
}
