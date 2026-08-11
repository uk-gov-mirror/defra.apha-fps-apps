using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class WgStaffPlanViewModel
    {
        public string? SelectedResourceCentre { get; set; }
        public List<SelectListItem> ResourceCentreList { get; set; } = new();
        public string? SelectedWorkGroup { get; set; }
        public List<SelectListItem> WorkGroupList { get; set; } = new();
        public DataGridConfig<WgStaffPlanViewItem> Grid { get; set; } = new();
    }
}
