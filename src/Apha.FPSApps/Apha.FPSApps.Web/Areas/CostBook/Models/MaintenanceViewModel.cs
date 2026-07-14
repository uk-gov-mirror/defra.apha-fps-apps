using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;


public class MaintenanceViewModel
{
    
    [Display(Name = "Animals Inflation (%)")]
    [Range(0, 999.99, ErrorMessage = "Animals Inflation must be a positive number no greater than 999.99.")]
    public decimal InflationAnimals { get; set; }

    [Display(Name = "Exceptional Costs Inflation (%)")]
    [Range(0, 999.99, ErrorMessage = "Exceptional Costs Inflation must be a positive number no greater than 999.99.")]
    public decimal InflationExceptionalCosts { get; set; }

    [Display(Name = "Staff Inflation (%)")]
    [Range(0, 999.99, ErrorMessage = "Staff Inflation must be a positive number no greater than 999.99.")]
    public decimal InflationStaff { get; set; }

    [Display(Name = "Tests Inflation (%)")]
    [Range(0, 999.99, ErrorMessage = "Tests Inflation must be a positive number no greater than 999.99.")]
    public decimal InflationTests { get; set; }
   
    [Display(Name = "Current Financial Year")]
    public int CurrentFinancialYear { get; set; }

    [Display(Name = "Working Hours in Day")]
    public decimal WorkingHoursInDay { get; set; }

    [Display(Name = "Working Days in Year")]
    public decimal WorkingDaysInYear { get; set; }
   
    public DataGridConfig<AccountCategoryItem> AccountCategoryGrid { get; set; } = new();
    public List<SelectListItem> Csg7GroupList { get; set; } = new();

    // ── Tab 3: CSG7 Inflation Options DataGrid ────────────────────────────────
    public DataGridConfig<Csg7GroupItem> Csg7GroupGrid { get; set; } = new();

    // ── Tab 4: Profit Margins ─────────────────────────────────────────────────
    [Display(Name = "Animals Profit (%)")]
    [Range(0, 999.99, ErrorMessage = "Animals Profit must be a positive number no greater than 999.99.")]
    public decimal ProfitAnimals { get; set; }

    [Display(Name = "Exceptional Costs Profit (%)")]
    [Range(0, 999.99, ErrorMessage = "Exceptional Costs Profit must be a positive number no greater than 999.99.")]
    public decimal ProfitExceptionalCosts { get; set; }

    [Display(Name = "Staff Profit (%)")]
    [Range(0, 999.99, ErrorMessage = "Staff Profit must be a positive number no greater than 999.99.")]
    public decimal ProfitStaff { get; set; }

    [Display(Name = "Tests Profit (%)")]
    [Range(0, 999.99, ErrorMessage = "Tests Profit must be a positive number no greater than 999.99.")]
    public decimal ProfitTests { get; set; }

    // ── Tab 5: CAPS Staff DataGrid ────────────────────────────────────────────
    public DataGridConfig<CapsStaffItem> CapsStaffGrid { get; set; } = new();
}
