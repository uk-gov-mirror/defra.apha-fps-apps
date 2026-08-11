using Apha.FPSApps.Web.Validation;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class AnimalMaintenanceViewModel
    {
        [Required(ErrorMessage = "Animal type is required")]
        [StringLength(50, ErrorMessage = "Animal type cannot exceed 50 characters")]
        [Display(Name = "Animal Type")]
        [GridColumn(IsFilterable = true)]
        public required string AnimalType { get; set; }

        [StringLength(50, ErrorMessage = "Species cannot exceed 50 characters")]
        [Display(Name = "Species")]
        [GridColumn(IsFilterable = true)]
        public string? Species { get; set; }

        [StringLength(50, ErrorMessage = "Security level cannot exceed 50 characters")]
        [Display(Name = "Security Level")]
        [GridColumn(IsFilterable = true)]
        public string? SecurityLevel { get; set; }

        [Display(Name = "Daily Rate")]
        [GridColumn(Type = GridColumnType.GbpValue)]
        [CurrencyRange]
        public decimal? DailyRate { get; set; }

        [Display(Name = "Defra Daily Rate")]
        [GridColumn(Type = GridColumnType.GbpValue)]
        [CurrencyRange]
        public decimal? DefraDailyRate { get; set; }

        [Display(Name = "Plan Full Weeks")]
        public bool PlanByWeek { get; set; }

        public AnimalMaintenanceViewModel()
        {
            AnimalType = string.Empty;
        }
    }
}
