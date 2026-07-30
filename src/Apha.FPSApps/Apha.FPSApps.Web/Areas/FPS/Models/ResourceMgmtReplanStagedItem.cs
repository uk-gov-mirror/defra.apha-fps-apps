using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Represents a single staged re-plan row in the staged panel (frmRM_RePlan — Section 4).
    /// </summary>
    public class ResourceMgmtReplanStagedItem
    {
        [Display(Name = "Staff ID")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly)]
        public string? StaffId { get; set; }

        [Display(Name = "Job Code")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly)]
        public string? JobCode { get; set; }

        [Display(Name = "Plan Hrs")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double PlannedHours { get; set; }
    }
}
