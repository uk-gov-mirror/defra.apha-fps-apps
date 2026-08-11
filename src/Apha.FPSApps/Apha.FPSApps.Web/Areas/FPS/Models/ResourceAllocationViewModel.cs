using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the Stage 2 Check Resource Allocation page (frmResourceAllocation).
    /// </summary>
    public class ResourceAllocationViewModel
    {
        /// <summary>Resource Centre / WorkGroup dropdown list.</summary>
        public List<SelectListItem> ResourceCentres { get; set; } = new();

        /// <summary>Currently selected Resource Centre code.</summary>
        public string SelectedResourceCentre { get; set; } = string.Empty;

        /// <summary>List of work groups for the selected resource centre.</summary>
        public List<SelectListItem> WorkGroupList { get; set; } = new();

        /// <summary>Currently selected Work Group.</summary>
        public string SelectedWorkGroup { get; set; } = string.Empty;

        /// <summary>Staff-of-grade allocation DataGrid.</summary>
        public DataGridConfig<ResourceStaffAllocationItem> StaffAllocationGrid { get; set; } = new();

        /// <summary>Jobs-for-staff DataGrid.</summary>
        public DataGridConfig<ResourceStaffJobItem> StaffJobsGrid { get; set; } = new();

        /// <summary>Name of the currently selected staff member (shown in the Person Selected panel).</summary>
        public string SelectedPersonName { get; set; } = string.Empty;
    }
}
