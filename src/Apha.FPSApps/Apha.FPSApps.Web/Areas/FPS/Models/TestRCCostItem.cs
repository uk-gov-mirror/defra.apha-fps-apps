using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestRCCostItem
    {
        // Part of composite PK — visible, editable
        [Required(ErrorMessage = "Profit Centre is required.")]
        [Display(Name = "ProfitCentre")]
        [GridColumn(Order = 1, Width = 140, Type = GridColumnType.Text, IsFilterable = true)]
        public string ProfitCentre { get; set; } = null!;

        // Maps to DTO Price (NOT NULL DEFAULT 0)
        [Required(ErrorMessage = "Price is required.")]
        [Display(Name = "Price")]
        [GridColumn(Order = 2, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal Price { get; set; }

        [GridColumn(IsVisible = false)]
        public string TestCode { get; set; } = null!;

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }
    }
}
