/*
 * TRANSFORMENGINE MIGRATION — AsuViewViewModel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New ViewModel created for the ASU View page (no prior equivalent in the MVC layer)
 *   - AsuViewGrid: DataGridConfig<AsuViewItem> — explicitly built in AsuViewController.Index(),
 *     never left as new() default
 *   - AnimalTypeList: List<SelectListItem> for the Animal Type dropdown — justified by the
 *     explicit <select>-equivalent custom dropdown control (#animal-type-dropdown) outside
 *     the grid container in fps_asuview.html
 *   - TotalAnimalDays / TotalCost: summary fields for the summary row rendered by
 *     updateAsuSummary() in fps_asuview.js; populated by AsuViewController.GetTotals()
 *
 * PRESERVED:
 *   - Property names match AsuViewDto exactly (AllowAnonymous field names unchanged)
 *   - Dropdown list named AnimalTypeList — bound scalar property is AnimalType
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm TotalCost type (decimal vs double) once DataAccess
 *     type for the cost column is finalised
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the ASU Data View page (fps_asuview.html).
    /// Carries the Animal Type filter dropdown, the ASU Usage grid config, and
    /// the Total Animal Days / Total Cost summary values.
    /// </summary>
    public class AsuViewViewModel
    {
        // TRANSFORMENGINE: AnimalType — the currently-selected animal type filter value.
        // Bound to the custom animal-type dropdown (#animal-type-dropdown) in the Razor view.
        // Named to match AsuViewDto.AnimalType exactly (no alias).
        public string? AnimalType { get; set; }

        // TRANSFORMENGINE: AnimalTypeList — SelectListItem list for the Animal Type filter dropdown.
        // Justified by the explicit custom dropdown control (#animal-type-dropdown) outside
        // the grid container in fps_asuview.html (not inferred from backend API params).
        // Named [FieldName]List where FieldName = AnimalType (bound scalar property above).
        public List<SelectListItem> AnimalTypeList { get; set; } = new();

        // TRANSFORMENGINE: AsuViewGrid — DataGridConfig<AsuViewItem> explicitly built in
        // AsuViewController.Index(). Never left as new() — see DANGEROUS DEFAULTS note in skill.
        // AllowAdd/AllowEdit/AllowDelete all false (JS showAddButton: false; IAsuViewService
        // exposes no create/update/delete methods).
        public DataGridConfig<AsuViewItem> AsuViewGrid { get; set; } = new();

        // TRANSFORMENGINE: TotalAnimalDays — corresponds to #asuTotalDays in fps_asuview.html.
        // Computed from the sum of AnimalDays for the selected animal type, mirroring
        // updateAsuSummary() totalDays in fps_asuview.js.
        public double TotalAnimalDays { get; set; }

        // TRANSFORMENGINE: TotalCost — corresponds to #asuTotalCost in fps_asuview.html.
        // Computed from the sum of Cost for the selected animal type, mirroring
        // updateAsuSummary() totalCost in fps_asuview.js. Decimal matches AsuViewDto.Cost type.
        public decimal TotalCost { get; set; }
    }
}
