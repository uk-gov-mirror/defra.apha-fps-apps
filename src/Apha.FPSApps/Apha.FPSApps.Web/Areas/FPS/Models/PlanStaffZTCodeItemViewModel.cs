using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{

    public class PlanStaffZTCodeItemViewModel
    {
        [Display(Name = "Staff ID")]
        [GridColumn(IsVisible = false)]
        public string StaffID { get; set; } = null!;

        [Required(ErrorMessage = "ZT Code is required")]
        [Display(Name = "ZT Code")]
        [GridColumn(Order = 1, Width = 160, IsFilterable = false)]
        public string JobCode { get; set; } = null!;

        /// <summary>
        /// Retains the original JobCode value before edit so the API can locate the record by its composite key.
        /// </summary>
        [GridColumn(IsVisible = false)]
        public string? OriginalJobCode { get; set; }

        [Display(Name = "ZT Description")]
        [GridColumn(Order = 2, Width = 200, IsFilterable = false)]
        public string? ZtDescription { get; set; }

        [Required(ErrorMessage = "Hours are required")]
        [Display(Name = "Hrs")]
        [Range(0, int.MaxValue, ErrorMessage = "Hours must be 0 or greater")]
        [GridColumn(Order = 3, Width = 100, IsFilterable = false)]
        public double PlannedHours { get; set; }

        [GridColumn(IsVisible = false)]
        public List<SelectListItem> ZtCodeList { get; set; } = new List<SelectListItem>();
    }


    public class PlanStaffZTCodePageViewModel
    {   
        // Staff header (from qrytblStaff / StaffView)
        public string StaffId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string WorkGroupGrade { get; set; } = string.Empty;

        // Time summary fields
        public double HrsPaid { get; set; }
        public double Leave { get; set; }
        public double SickSpecial { get; set; }
        public double HrsAvail { get; set; }
        public double PlannedAdminZT { get; set; }   // sum of planned ZT hours = Admin
        public double FreeForChargeableWork => HrsAvail - PlannedAdminZT;  // Remainder

        /// <summary>
        /// True when the user arrived from the SetUpStaffResources page.
        /// Controls which Back link is rendered.
        /// </summary>
        public bool ReturnToSsr { get; set; }

        // Grid configuration
        public DataGridConfig<PlanStaffZTCodeItemViewModel> GridConfig { get; set; } = new();

        public List<PlanStaffZTCodeItemViewModel> Items { get; set; } = new();
        public List<SelectListItem> ZtCodeList { get; set; } = new();
    }
}
