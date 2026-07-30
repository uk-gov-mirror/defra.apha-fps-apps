using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestRequirementItem
    {
        // Maps to DTO Buyer (project buyer code) — part of composite PK
        [Required(ErrorMessage = "Project (Buyer) is required.")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string Buyer { get; set; } = null!;

        // Maps to DTO NoRequired (double? in DTO — using decimal? for grid rendering)
        [Required(ErrorMessage = "No Tests is required.")]
        [Display(Name = "No Tests")]
        [GridColumn(Order = 2, Width = 90, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double? NoRequired { get; set; }

        // Maps to DTO UnitPrice (agreed/agency price per test)
        [Required(ErrorMessage = "AgPrice is required.")]
        [Display(Name = "Agr Price")]
        [GridColumn(Order = 3, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? UnitPrice { get; set; }

        [GridColumn(IsVisible = false)]
        public string TestCode { get; set; } = null!;

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }

        [GridColumn(IsVisible = false)]
        public string? ProjectBuyerCode { get; set; }

        [GridColumn(IsVisible = false)]
        public string? TestBuyerCode { get; set; }

        [GridColumn(IsVisible = false)]
        public short? Active { get; set; }

        [GridColumn(IsVisible = false)]
        public decimal? RecUnitPrice { get; set; }
    }
}
