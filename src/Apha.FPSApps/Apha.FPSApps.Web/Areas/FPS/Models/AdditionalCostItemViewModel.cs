using Apha.FPSApps.Web.Validation;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class AdditionalCostItemViewModel
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? JobCode { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        [StringLength(20, ErrorMessage = "Description cannot exceed 20 characters")]
        [GridColumn(Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account is required")]
        [Display(Name = "Account")]
        [GridColumn(Width = 130, Type = GridColumnType.Text, IsFilterable = true)]
        public string Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "Item cost is required")]
        [Display(Name = "Total Cost")]
        [CurrencyRange]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal ItemCost { get; set; }

        [Display(Name = "Freq/Month")]
        [StringLength(5, ErrorMessage = "Freq cannot exceed 5 characters")]
        [GridColumn(Width = 90, Type = GridColumnType.Text, IsFilterable = false)]
        public string? Freq { get; set; }

        [Display(Name = "Supplier")]
        [StringLength(50, ErrorMessage = "Supplier cannot exceed 50 characters")]
        [GridColumn(Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Supplier { get; set; }

        [GridColumn(IsVisible = false)]
        public List<SelectListItem> AccountList { get; set; } = new List<SelectListItem>();

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? OriginalDescription { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? OriginalAccount { get; set; }
    }
}
