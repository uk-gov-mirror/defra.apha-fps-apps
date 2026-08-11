using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class TestPurchaseRequirementItem
    {
        [Display(Name = "Test Code")]
        [Required(ErrorMessage = "Test Code is required.")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [Display(Name = "No Required")]
        [Range(0, double.MaxValue, ErrorMessage = "No Required must be 0 or greater.")]
        [GridColumn(Order = 2, Width = 110, Type = GridColumnType.DecimalNumber)]
        public double? NoRequired { get; set; }

        [Display(Name = "Unit Price")]
        [Required(ErrorMessage = "Unit Price is required.")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Unit Price must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 3, Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? UnitPrice { get; set; }

        // ── Hidden (not shown in grid, kept for form and mapping) ─────────

        [GridColumn(IsVisible = false)]
        public bool IsEdit { get; set; }

        [Required(ErrorMessage = "Buyer is required.")]
        [GridColumn(IsVisible = false)]
        public string Buyer { get; set; } = null!;

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
