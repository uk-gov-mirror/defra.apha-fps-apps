/*
 * TRANSFORMENGINE MIGRATION — InflationSettingsItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend Item model created for frmMaintainance Tab 1 (Inflation Figures)
 *   - Properties derived from HTML prototype form fields: inflAnimals, inflExceptionalCosts,
 *     inflStaff, inflTests, inflCurrentFinancialYear, inflWorkingHoursInDay, inflWorkingDaysInYear
 *   - Property names match MaintenanceSettingsDto exactly (inflation sub-set)
 *   - Tab 1 is a static form (not a DataGrid) — no GridColumn attributes
 *   - All fields are numeric and required (per JS bindStaticFormValidation)
 *
 * PRESERVED:
 *   - All 7 inflation/system fields from HTML prototype formInflation
 *   - Required validation matches JS isNumericValue guard
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm decimal precision requirements align with fnInflation() rounding
 *   - TRANSFORMENGINE TODO: CurrentFinancialYear is int — confirm whether a year range dropdown is needed
 */

using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

// TRANSFORMENGINE: Item model for frmMaintainance Tab 1 (Inflation Figures)
//   Maps to MaintenanceSettingsDto inflation sub-set (InflationAnimals, InflationExceptionalCosts,
//   InflationStaff, InflationTests, CurrentFinancialYear, WorkingHoursInDay, WorkingDaysInYear)
public class InflationSettingsItem
{
    // TRANSFORMENGINE: HTML id=inflAnimals → MaintenanceSettingsDto.InflationAnimals
    [Required(ErrorMessage = "Animals inflation rate is required.")]
    [Display(Name = "Animals Inflation (%)")]
    public decimal InflationAnimals { get; set; }

    // TRANSFORMENGINE: HTML id=inflExceptionalCosts → MaintenanceSettingsDto.InflationExceptionalCosts
    [Required(ErrorMessage = "Exceptional Costs inflation rate is required.")]
    [Display(Name = "Exceptional Costs Inflation (%)")]
    public decimal InflationExceptionalCosts { get; set; }

    // TRANSFORMENGINE: HTML id=inflStaff → MaintenanceSettingsDto.InflationStaff
    [Required(ErrorMessage = "Staff inflation rate is required.")]
    [Display(Name = "Staff Inflation (%)")]
    public decimal InflationStaff { get; set; }

    // TRANSFORMENGINE: HTML id=inflTests → MaintenanceSettingsDto.InflationTests
    [Required(ErrorMessage = "Tests inflation rate is required.")]
    [Display(Name = "Tests Inflation (%)")]
    public decimal InflationTests { get; set; }

    // TRANSFORMENGINE: HTML id=inflCurrentFinancialYear → MaintenanceSettingsDto.CurrentFinancialYear
    [Required(ErrorMessage = "Current Financial Year is required.")]
    [Display(Name = "Current Financial Year")]
    public int CurrentFinancialYear { get; set; }

    // TRANSFORMENGINE: HTML id=inflWorkingHoursInDay → MaintenanceSettingsDto.WorkingHoursInDay
    [Required(ErrorMessage = "Working Hours in Day is required.")]
    [Display(Name = "Working Hours in Day")]
    public decimal WorkingHoursInDay { get; set; }

    // TRANSFORMENGINE: HTML id=inflWorkingDaysInYear → MaintenanceSettingsDto.WorkingDaysInYear
    [Required(ErrorMessage = "Working Days in Year is required.")]
    [Display(Name = "Working Days in Year")]
    public decimal WorkingDaysInYear { get; set; }
}
