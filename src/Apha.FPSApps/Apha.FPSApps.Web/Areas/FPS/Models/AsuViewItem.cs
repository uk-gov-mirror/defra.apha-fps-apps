/*
 * TRANSFORMENGINE MIGRATION — AsuViewItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - Phase 10 stub replaced with fully-attributed DataGrid item class
 *   - [GridColumn] attributes added to all properties per fps_asuview.js DataGridComponent
 *     column definitions (initializeAsuGrid columns array)
 *   - Id: hidden PK (not a visible JS column — used only as row key for edit/delete)
 *   - AnimalType: hidden (not a JS grid column — it is the page-level filter, not a grid column)
 *   - Project: visible, Text, 200px — JS { field: 'project', header: 'Project', width: 200 }
 *   - AnimalDays: visible, DoubleNumber, 150px — JS { field: 'animalDays', header: 'Animal Days', width: 150 }
 *   - Cost: visible, GbpValue, 150px — JS { field: 'cost', header: 'Cost', width: 150,
 *     render: formatMoney() } (£ currency display)
 *   - [Required] on editable modal fields only (Project, AnimalDays, Cost) —
 *     matches fps_asuview.js asuValidationFields list
 *
 * PRESERVED:
 *   - All property names and types identical to AsuViewDto (AutoMapper convention mapping
 *     in FpsViewModelMapper.CreateMap<AsuViewItem, AsuViewDto>().ReverseMap() still valid)
 *   - Namespace Apha.FPSApps.Web.Areas.FPS.Models (unchanged from stub)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm Cost type (decimal vs double) against the DB column
 *     type in mabarchive once DataAccess layer is finalised
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid row item for the ASU Data View DataGrid.
    /// Properties derived from <c>initializeAsuGrid</c> column definitions in fps_asuview.js.
    /// AutoMapper convention maps this class to/from <c>AsuViewDto</c> via
    /// <c>FpsViewModelMapper.CreateMap&lt;AsuViewItem, AsuViewDto&gt;().ReverseMap()</c>.
    /// </summary>
    public class AsuViewItem
    {
        // TRANSFORMENGINE: Id — hidden PK. Not a visible JS column; used only as the DataGrid
        // KeyProperty for row-level edit/delete operations (row.id in fps_asuview.js).
        // IsVisible = false because 'id' does NOT appear in the JS columns array.
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int Id { get; set; }

        // TRANSFORMENGINE: AnimalType — hidden. Not a JS grid column; it is the page-level
        // filter value passed via the #asu-animal-type-value hidden input in fps_asuview.html.
        // Kept on the model so AutoMapper can map AsuViewDto.AnimalType without a ForMember override.
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? AnimalType { get; set; }

        // TRANSFORMENGINE: Project — JS column { field: 'project', header: 'Project', width: 200 }.
        // Editable field in the Add/Edit modal (modal-asu-project in fps_asuview.js).
        // [Required] matches asuValidationFields entry { message: 'Enter Project' }.
        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        [GridColumn(Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Project { get; set; }

        // TRANSFORMENGINE: AnimalDays — JS column { field: 'animalDays', header: 'Animal Days', width: 150 }.
        // Editable field in the Add/Edit modal (modal-asu-animaldays in fps_asuview.js).
        // [Required] matches asuValidationFields entry { message: 'Enter Animal Days' }.
        // DoubleNumber type: double in AsuViewDto matches JS Number(animalDays) in validateAsuModal().
        [Required(ErrorMessage = "Animal Days is required")]
        [Display(Name = "Animal Days")]
        [GridColumn(Width = 150, Type = GridColumnType.DoubleNumber, IsFilterable = true)]
        public double AnimalDays { get; set; }

        // TRANSFORMENGINE: Cost — JS column { field: 'cost', header: 'Cost', width: 150,
        // render: formatMoney(value) }. Rendered as £0.00 in the grid via formatMoney().
        // GbpValue type matches the £ currency display; decimal matches AsuViewDto.Cost.
        // Editable in the Add/Edit modal (modal-asu-cost in fps_asuview.js).
        // [Required] matches asuValidationFields entry { message: 'Enter Cost' }.
        [Required(ErrorMessage = "Cost is required")]
        [Display(Name = "Cost")]
        [GridColumn(Width = 150, Type = GridColumnType.GbpValue, IsFilterable = true)]
        public decimal Cost { get; set; }
    }
}
