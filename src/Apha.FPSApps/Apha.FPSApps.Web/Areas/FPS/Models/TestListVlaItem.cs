using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestListVlaItem
    {
        [Required(ErrorMessage = "Item Code is required.")]
        [Display(Name = "ItemCode")]
        [GridColumn(Order = 1, Width = 95, Type = GridColumnType.Text, IsFilterable = true)]
        public string ItemCode { get; set; } = null!;

        // HTML modal vla-form-description — aria-required="true" → [Required]
        [Required(ErrorMessage = "Description is required.")]
        [Display(Name = "Description")]
        [GridColumn(Order = 2, Width = 310, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ItemDescription { get; set; }

        // Maps to DTO ShortDescription — not required in modal
        [Display(Name = "Short Desc")]
        [GridColumn(Order = 3, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ShortDescription { get; set; }

        // Maps to DTO TestManager — not required in modal
        [Display(Name = "Manager")]
        [GridColumn(Order = 4, Width = 90, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TestManager { get; set; }

        // Maps to DTO JobStatus — not required in modal
        [Display(Name = "Status")]
        [GridColumn(Order = 5, Width = 75, Type = GridColumnType.Text, IsFilterable = true)]
        public string? JobStatus { get; set; }

        // Maps to DTO UnitPriceVla — aria-required="true" in modal → [Required]
        [Required(ErrorMessage = "Unit Price (Std) is required.")]
        [Display(Name = "UnitPrice(Std)")]
        [GridColumn(Order = 6, Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? UnitPriceVla { get; set; }

        // Maps to DTO DefraUnitPrice (NOT NULL DEFAULT 0 in DDL) — aria-required="true" in modal
        [Required(ErrorMessage = "Default Unit Price is required.")]
        [Display(Name = "DefaultUnitPrice")]
        [GridColumn(Order = 7, Width = 130, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal DefraUnitPrice { get; set; }

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }

        [GridColumn(IsVisible = false)]
        public decimal? PriceAhvg { get; set; }

        [GridColumn(IsVisible = false)]
        public string? Owner { get; set; }

        [GridColumn(IsVisible = false)]
        public string? ChargeMethod { get; set; }
    }
}
