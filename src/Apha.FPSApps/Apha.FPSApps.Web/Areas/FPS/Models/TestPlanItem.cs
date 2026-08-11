using Apha.FPSApps.Web.Validation;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestPlanItem
    {
        [Display(Name = "Test")]
        [Required(ErrorMessage = "Test Code is required.")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [Display(Name = "Description")]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ItemDescription { get; set; }

        [Display(Name = "RecPrice")]
        [DisplayFormat(DataFormatString = "{0:F4}", ApplyFormatInEditMode = true)]
        [GridColumn(Order = 3, Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? RecUnitPrice { get; set; }

        [Display(Name = "No")]
        [Range(0, double.MaxValue, ErrorMessage = "No Required must be 0 or greater.")]
        [GridColumn(Order = 4, Width = 110, Type = GridColumnType.DecimalNumber)]
        public double NoRequired { get; set; }

        [Display(Name = "AgrPrice")]
        [CurrencyRange]
        [DisplayFormat(DataFormatString = "{0:F4}", ApplyFormatInEditMode = true)]
        [GridColumn(Order = 5, Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "Fee")]
        [DisplayFormat(DataFormatString = "{0:F4}", ApplyFormatInEditMode = false)]
        [GridColumn(Order = 6, Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? TestCost => (UnitPrice ?? 0) * (decimal)NoRequired;  

        [GridColumn(IsVisible = false)]
        public bool IsEdit { get; set; }

        [Required(ErrorMessage = "Buyer is required.")]
        [GridColumn(IsVisible = false)]
        public string Buyer { get; set; } = null!;

        [GridColumn(IsVisible = false)]
        public string? ProjectBuyerCode { get; set; }

        [GridColumn(IsVisible = false)]
        public short? Active { get; set; }

        [GridColumn(IsVisible = false)]
        public List<SelectListItem> TestCodeOptions { get; set; } = new();
    }
}
