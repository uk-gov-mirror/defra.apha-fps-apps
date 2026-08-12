using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class TestRequirementItem
    {
        // ── Key / hidden ──────────────────────────────────────────────────────

        [Display(Name = "Test Code")]
        [Required(ErrorMessage = "Test Code is required.")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        // ── Visible grid columns (order matches prototype) ────────────────────

        [Display(Name = "Project")]
        [GridColumn(Order = 2, Width = 180, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ProjectBuyerCode { get; set; }

        [Display(Name = "Buyer")]
        [Required(ErrorMessage = "Buyer is required.")]
        [GridColumn(Order = 3, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string Buyer { get; set; } = null!;

        [Display(Name = "Unit Price")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Unit Price must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 4, Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "No")]
        [Range(0, double.MaxValue, ErrorMessage = "No must be 0 or greater.")]
        [GridColumn(Order = 5, Width = 70, Type = GridColumnType.DecimalNumber)]
        public double? NoRequired { get; set; }

        [Display(Name = "Active")]
        [GridColumn(Order = 6, Width = 70, Type = GridColumnType.Number)]
        public short? Active { get; set; }

        [Display(Name = "Defra Project?")]
        [GridColumn(Order = 7, Width = 100, Type = GridColumnType.Checkbox)]
        public short IsDefraProject { get; set; }

        [Display(Name = "RecUnitPrice")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Rec Unit Price must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 8, Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? RecUnitPrice { get; set; }

        // ── Hidden (not shown in grid or form, kept for mapping) ─────────────

        [GridColumn(IsVisible = false)]
        public string? TestBuyerCode { get; set; }
    }
}
