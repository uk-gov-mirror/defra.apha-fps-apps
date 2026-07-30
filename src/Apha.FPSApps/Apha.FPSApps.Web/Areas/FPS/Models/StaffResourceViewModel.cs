using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class StaffResourceViewModel
    {
        /// <summary>Currently selected profit centre code.</summary>
        public string? SelectedProfitCentre { get; set; }

        /// <summary>Profit centre dropdown options.</summary>
        public List<SelectListItem> ProfitCentreList { get; set; } = new();

        /// <summary>Currently selected workgroup name.</summary>
        public string? SelectedWorkgroup { get; set; }

        /// <summary>Workgroup grid — filtered by selected profit centre.</summary>
        public DataGridConfig<StaffResourceWorkgroupItem> WorkgroupGrid { get; set; } = new DataGridConfig<StaffResourceWorkgroupItem>();

        /// <summary>Staff utilisation grid — populated after workgroup selection.</summary>
        public DataGridConfig<StaffResourceStaffItem> StaffGrid { get; set; } = new DataGridConfig<StaffResourceStaffItem>();
    }
}
