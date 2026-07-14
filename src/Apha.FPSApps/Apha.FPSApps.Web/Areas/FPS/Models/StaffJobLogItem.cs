using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Read-only DataGrid row model for the Staff Plan Changes audit log tab.
    /// Derives from JS initializeStaffPlanChangesTable() columns array (8 visible columns).
    /// Property names match StaffJobLogDto exactly where applicable; Name and UserEmail are
    /// display-only fields not present in the DTO (see DEFERRED notes).
    /// </summary>
    public class StaffJobLogItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SequenceNo { get; set; }

        [Display(Name = "StaffID")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string StaffId { get; set; } = null!;

        // JS column field=name, header=Name, width=240
        [Display(Name = "Name")]
        [GridColumn(Width = 240, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "Jobcode")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string JobCode { get; set; } = null!;

        [Display(Name = "Plannedhours")]
        [GridColumn(Width = 140, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double PlannedHours { get; set; }

        [Display(Name = "Date_Time")]
        [GridColumn(Width = 180, Type = GridColumnType.DateTime, IsFilterable = false)]
        public DateTime? DateTime { get; set; }

        [Display(Name = "User_ID")]
        [GridColumn(Width = 170, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? UserId { get; set; }

        // JS column field=userEmail, header=User_Email, width=240
        [Display(Name = "User_Email")]
        [GridColumn(Width = 240, Type = GridColumnType.ReadOnly, IsFilterable = false, IsVisible = false)]
        public string? UserEmail { get; set; }

        [Display(Name = "Insert_Delete")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? InsertDelete { get; set; }
    }
}
