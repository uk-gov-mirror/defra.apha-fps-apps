/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: grid row item for the ContributionSummary data grid (frmTimeSellerPC).
 *   - Properties derived from the JS DataGridComponent({ columns: [...] }) array in
 *     contribution_summary.js — each column { field, header, width, render } maps to one property.
 *   - Id (int) is the row key (KeyProperty = "Id"); it is NOT a visible JS column, so it is
 *     declared as hidden (IsVisible = false).
 *   - 12 visible properties matching the 12 JS columns in order:
 *       Wg (Text, 80), Grade (Text, 100), AvailHrs (DoubleNumber, 100),
 *       ChgRate (GbpValue, 95), TotalPlanHrs (DoubleNumber, 95),
 *       TotalFec (GbpValue, 105), TotalPctPlanned (Number, 85),
 *       AssuredPlanHrs (DoubleNumber, 95), AssuredFec (GbpValue, 105),
 *       AssuredPctPlanned (Number, 85), OhRate (GbpValue, 95), TotalCont (GbpValue, 110).
 *   - AllowAdd = false (showAddButton: false in JS prototype) — Required attrs retained on
 *     CRUD-modal fields because the add-row button (cs-add-row-btn) still triggers the modal.
 *   - AllowEdit = true (onEdit callback present in JS DataGridComponent callbacks).
 *   - AllowDelete = false (no delete callback in JS DataGridComponent callbacks).
 *
 * PRESERVED:
 *   - Property names match ContributionSummaryDto field names exactly (PascalCase, no Lk_ prefix).
 *   - Column widths taken directly from JS columns[].width values.
 *   - Display names taken from JS columns[].header values.
 *   - Required validation messages match JS crudFields[].requiredMessage values.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify whether ProfitCentre / FpsYear need to be carried on this item
 *     for CRUD POST/PUT payloads, or whether those are resolved server-side from request context.
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid row item for the ContributionSummary data grid (frmTimeSellerPC).
    /// Properties derived from the JS DataGridComponent columns array in contribution_summary.js.
    /// Also used as the modal partial model for _AddEditContributionSummary.cshtml.
    /// </summary>
    public class ContributionSummaryItem
    {
        // TRANSFORMENGINE: Id — integer PK; NOT a visible JS grid column; used as KeyProperty for
        //   edit/delete row operations. Hidden in the grid (IsVisible = false).
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int Id { get; set; }

        // TRANSFORMENGINE: Wg — JS column { field: "wg", header: "WG", width: 80 }; editable text field.
        //   Required in CRUD modal (crudFields[0].requiredMessage = "Enter WG").
        [Required(ErrorMessage = "Enter WG")]
        [Display(Name = "WG")]
        [GridColumn(Width = 80, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Wg { get; set; }

        // TRANSFORMENGINE: Grade — JS column { field: "grade", header: "Grade", width: 100 }; editable text field.
        //   Required in CRUD modal (crudFields[1].requiredMessage = "Enter grade").
        [Required(ErrorMessage = "Enter grade")]
        [Display(Name = "Grade")]
        [GridColumn(Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Grade { get; set; }

        // TRANSFORMENGINE: AvailHrs — JS column { field: "availHrs", header: "Avail Hrs", width: 100,
        //   render: formatNumber }; double, right-aligned number.
        //   Required in CRUD modal (crudFields[2].requiredMessage = "Enter available hours").
        [Required(ErrorMessage = "Enter available hours")]
        [Display(Name = "Avail Hrs")]
        [GridColumn(Width = 100, Type = GridColumnType.DoubleNumber, IsFilterable = false)]
        public double AvailHrs { get; set; }

        // TRANSFORMENGINE: ChgRate — JS column { field: "chgRate", header: "Chg Rate", width: 95,
        //   render: formatCurrency }; decimal, right-aligned currency (£).
        //   Required in CRUD modal (crudFields[3].requiredMessage = "Enter change rate").
        [Required(ErrorMessage = "Enter change rate")]
        [Display(Name = "Chg Rate")]
        [GridColumn(Width = 95, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal ChgRate { get; set; }

        // TRANSFORMENGINE: TotalPlanHrs — JS column { field: "totalPlanHrs", header: "PlanHrs", width: 95,
        //   render: formatNumber }; double, right-aligned number (grouped under "Total Planned Time" header).
        //   Required in CRUD modal (crudFields[4].requiredMessage = "Enter total planned hours").
        [Required(ErrorMessage = "Enter total planned hours")]
        [Display(Name = "PlanHrs")]
        [GridColumn(Width = 95, Type = GridColumnType.DoubleNumber, IsFilterable = false)]
        public double TotalPlanHrs { get; set; }

        // TRANSFORMENGINE: TotalFec — JS column { field: "totalFec", header: "FEC", width: 105,
        //   render: formatCurrency }; decimal, right-aligned currency (£) (grouped under "Total Planned Time").
        //   Required in CRUD modal (crudFields[5].requiredMessage = "Enter total FEC").
        [Required(ErrorMessage = "Enter total FEC")]
        [Display(Name = "FEC")]
        [GridColumn(Width = 105, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TotalFec { get; set; }

        // TRANSFORMENGINE: TotalPctPlanned — JS column { field: "totalPctPlanned", header: "% Planned", width: 85,
        //   render: formatPercent }; int, right-aligned percentage (grouped under "Total Planned Time").
        //   Required in CRUD modal (crudFields[6].requiredMessage = "Enter total percentage planned"); max 100.
        [Required(ErrorMessage = "Enter total percentage planned")]
        [Range(0, 100, ErrorMessage = "Total % Planned must be between 0 and 100")]
        [Display(Name = "% Planned")]
        [GridColumn(Width = 85, Type = GridColumnType.Number, IsFilterable = false)]
        public int TotalPctPlanned { get; set; }

        // TRANSFORMENGINE: AssuredPlanHrs — JS column { field: "assuredPlanHrs", header: "PlanHrs", width: 95,
        //   render: formatNumber }; double, right-aligned number (grouped under "Assured Planned Time").
        //   Required in CRUD modal (crudFields[7].requiredMessage = "Enter assured planned hours").
        [Required(ErrorMessage = "Enter assured planned hours")]
        [Display(Name = "PlanHrs")]
        [GridColumn(Width = 95, Type = GridColumnType.DoubleNumber, IsFilterable = false)]
        public double AssuredPlanHrs { get; set; }

        // TRANSFORMENGINE: AssuredFec — JS column { field: "assuredFec", header: "FEC", width: 105,
        //   render: formatCurrency }; decimal, right-aligned currency (£) (grouped under "Assured Planned Time").
        //   Required in CRUD modal (crudFields[8].requiredMessage = "Enter assured FEC").
        [Required(ErrorMessage = "Enter assured FEC")]
        [Display(Name = "FEC")]
        [GridColumn(Width = 105, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal AssuredFec { get; set; }

        // TRANSFORMENGINE: AssuredPctPlanned — JS column { field: "assuredPctPlanned", header: "% Planned", width: 85,
        //   render: formatPercent }; int (grouped under "Assured Planned Time"); max 100.
        //   Required in CRUD modal (crudFields[9].requiredMessage = "Enter assured percentage planned").
        [Required(ErrorMessage = "Enter assured percentage planned")]
        [Range(0, 100, ErrorMessage = "Assured % Planned must be between 0 and 100")]
        [Display(Name = "% Planned")]
        [GridColumn(Width = 85, Type = GridColumnType.Number, IsFilterable = false)]
        public int AssuredPctPlanned { get; set; }

        // TRANSFORMENGINE: OhRate — JS column { field: "ohRate", header: "OH Rate", width: 95,
        //   render: formatCurrency }; decimal, right-aligned currency (grouped under 'Rate "Efficacy" Checker').
        //   Required in CRUD modal (crudFields[10].requiredMessage = "Enter OH rate").
        [Required(ErrorMessage = "Enter OH rate")]
        [Display(Name = "OH Rate")]
        [GridColumn(Width = 95, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal OhRate { get; set; }

        // TRANSFORMENGINE: TotalCont — JS column { field: "totalCont", header: "Total Cont", width: 110,
        //   render: formatCurrency }; decimal, right-aligned currency (grouped under 'Rate "Efficacy" Checker').
        //   Required in CRUD modal (crudFields[11].requiredMessage = "Enter total contribution").
        [Required(ErrorMessage = "Enter total contribution")]
        [Display(Name = "Total Cont")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TotalCont { get; set; }
    }
}
