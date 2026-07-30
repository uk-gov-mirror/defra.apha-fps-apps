using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the Grade Maintenance DataGrid form (frmMaintGrade).
    /// No page-level dropdowns — HTML prototype contains no &lt;select&gt; outside the grid container.
    /// Grid config is built explicitly in GradeMaintenanceController.Index() — never left as new().
    /// </summary>
    public class GradeMaintenanceViewModel
    {
        // Leaving as new() would render an empty grid with default Add button regardless of profile
        public DataGridConfig<GradeItem> GradeGrid { get; set; } = new DataGridConfig<GradeItem>();
    }

    /// <summary>
    /// DataGrid row model and Add/Edit modal partial model for Grade Maintenance.
    /// Properties derived from the JS DataGridComponent columns array in fps_grade_maintenance.js.
    /// Property names must match GradeDto exactly for FpsViewModelMapper AutoMapper profiles.
    /// </summary>
    public class GradeItem
    {
        // Visible in grid (JS column present); used as KeyProperty in DataGridConfig.
        // ReadOnly type in grid display; editable in Add modal, read-only on Edit modal (Phase 12).
        // [Required] matches JS modal validation: requiredFields = [gradeValidationFields[0]] (GradeCode only).
        [Required(ErrorMessage = "GradeCode is required")]
        [Display(Name = "GradeCode")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string GradeCode { get; set; } = null!;

        // Not required — JS modal validation does NOT include description in requiredFields array.
        // Maps to Grade.DescLong via backend EntityMapper (DescLong → Description rename).
        [Display(Name = "Description")]
        [GridColumn(Width = 280, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Description { get; set; }

        // Currency field — HTML prototype shows £ prefix; GbpValue type renders currency format.
        // Not required — JS modal validation only checks numeric format if a value is provided.
        [Display(Name = "AvSalary")]
        [GridColumn(Width = 170, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? AvSalary { get; set; }

        // Hidden field; FPS financial year partition managed server-side via HasQueryFilter on FpsDbContext.
        // Included to support full AutoMapper round-trip with GradeDto.FpsYear.
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int? FpsYear { get; set; }
    }
}
