using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid item for the Jobs-for-Staff grid (frmResourceDetail2 — read-only).
    /// </summary>
    public class ResourceStaffJobItem
    {
        [GridColumn(IsVisible = false)]
        public int? StaffId { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Project { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Width = 220, Type = GridColumnType.ReadOnly)]
        public string? Description { get; set; }

        [Display(Name = "Hour")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? Hour { get; set; }

        [Display(Name = "Status")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly)]
        public string? Status { get; set; }
    }
}
