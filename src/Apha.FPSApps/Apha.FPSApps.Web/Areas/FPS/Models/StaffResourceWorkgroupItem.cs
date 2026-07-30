using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class StaffResourceWorkgroupItem
    {
        [Display(Name = "Workgroup")]
        [GridColumn(Order = 1, Width = 160, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string WorkGroupName { get; set; } = string.Empty;
    }
}
