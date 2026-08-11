using Apha.FPSApps.Web.Validation;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProgramViewModel
    {
        [Required(ErrorMessage = "Program number is required")]
        [StringLength(10, ErrorMessage = "Program number cannot exceed 10 characters")]
        [Display(Name = "Program")]
        [GridColumn(IsFilterable = true)]
        public required string ProgramNo { get; set; }
        
       
        [StringLength(80, ErrorMessage = "Program name cannot exceed 80 characters")]
        [Display(Name = "Program Name")]
        [GridColumn(IsFilterable = true)]
        public required string ProgramName { get; set; }


        [GridColumn(Type = GridColumnType.GbpValue)]
        [Display(Name = "Target")]
        [CurrencyRange]
        public decimal? Target { get; set; }

        
        [StringLength(50, ErrorMessage = "Manager name cannot exceed 50 characters")]
        [Display(Name = "Manager")]
        [GridColumn(IsFilterable = true)]
        public string? Manager { get; set; }
       
      
        [StringLength(50, ErrorMessage = "Directorate cannot exceed 50 characters")]
        [Display(Name = "Directorate")]
        public required string Directorate { get; set; }
        
        [GridColumn(IsVisible = false)]
        public List<SelectListItem> DirectorateOptions { get; set; }
        [GridColumn(IsVisible = false)]
        public List<SelectListItem>  ManagerList { get; set; }

        public ProgramViewModel()
        {
            ProgramNo = string.Empty;
            ProgramName = string.Empty;
            Directorate = string.Empty;
            DirectorateOptions = new List<SelectListItem>();
            ManagerList = new List<SelectListItem>();
        }
    }
}
