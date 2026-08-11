using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class StaffJobItemViewModel
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? StaffID { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string JobCode { get; set; } = null!;


        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? WorkGroupGrade { get; set; }

        [Required(ErrorMessage = "Staff name is required")]
        [Display(Name = "Staff Name")]
        [StringLength(200, ErrorMessage = "Staff name cannot exceed 200 characters")]
        [GridColumn(Width = 169, Type = GridColumnType.Text, IsFilterable = true)]
        public string Name { get; set; } = string.Empty;
        
        
        [Display(Name = "Rate")]
        [Range(0, double.MaxValue, ErrorMessage = "Rate must be a positive value")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 63, Type = GridColumnType.GbpValue)]
        public decimal ChargeRate { get; set; }

       
        [Display(Name = "Hrs")]
        [Range(0, int.MaxValue, ErrorMessage = "Hours must be 0 or greater")]
        [GridColumn(Width = 69, Type = GridColumnType.Number, IsFilterable = false)]
        public double PlannedHours { get; set; } 
        
        
        [Display(Name = "Days")]
        [Range(0, double.MaxValue, ErrorMessage = "Days must be 0 or greater")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [GridColumn(Width = 81, Type = GridColumnType.DecimalNumber)]
        public decimal Days { get; set; }
        
        
        [Display(Name = "Staff Cost")]        
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 104, Type = GridColumnType.GbpValue)]
        [CurrencyRange]
        public decimal StaffCost { get; set; }

        [GridColumn(IsVisible = false)]
        public List<SelectListItem> StaffList { get; set; } = new List<SelectListItem>();
    }
}
