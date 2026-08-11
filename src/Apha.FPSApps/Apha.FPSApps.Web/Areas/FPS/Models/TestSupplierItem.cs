using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Validation;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid model for the FPS Test Supplier view.
    /// Read-only columns sourced from the FPS TestSupplier API (custom project-join view).
    /// CRUD operations use the PACT TestRequirement API.
    /// </summary>
    public class TestSupplierItem
    {
        [Display(Name = "Test Code")]
        [Required(ErrorMessage = "Test Code is required.")]
        [GridColumn(IsVisible = false)]  // ✅ Hidden from grid
        public string TestCode { get; set; } = null!;

        [Display(Name = "Project")]  // ✅ Renamed from "Buyer" to "Project"
        [Required(ErrorMessage = "Project is required.")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string Buyer { get; set; } = null!;

        [Display(Name = "Project Manager")]
        [GridColumn(Order = 2, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ProjectManager { get; set; }

        [Display(Name = "No Tests")]  // ✅ Renamed from "No. Required" to "No Tests"
        [GridColumn(Order = 3, Width = 90, Type = GridColumnType.DecimalNumber)]
        public int? NoRequired { get; set; }

        [Display(Name = "Test Price")]  // ✅ Renamed from "Unit Price" to "Test Price"
        [GridColumn(Order = 4, Width = 110, Type = GridColumnType.GbpValue)]
        [CurrencyRange]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "Test Cost")]
        [GridColumn(Order = 5, Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? TestCost { get; set; }

        [Display(Name = "Project Status")]
        [GridColumn(Order = 6, Width = 130, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ProjectStatus { get; set; }

        // ── Hidden — used by Add/Edit modal only ──────────────────────────────

        [GridColumn(IsVisible = false)]
        public string? ProjectBuyerCode { get; set; }

        [GridColumn(IsVisible = false)]
        public string? TestBuyerCode { get; set; }

        [GridColumn(IsVisible = false)]
        public short? Active { get; set; }

        [GridColumn(IsVisible = false)]
        public short IsDefraProject { get; set; }

        [GridColumn(IsVisible = false)]
        public decimal? RecUnitPrice { get; set; }
    }
}