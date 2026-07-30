using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the Resource Management Re-plan page (frmRM_RePlan).
    /// </summary>
    public class ResourceMgmtReplanViewModel
    {
        /// <summary>Resource Centre dropdown list.</summary>
        public List<SelectListItem> ResourceCentres { get; set; } = new();

        /// <summary>Currently selected Resource Centre code.</summary>
        public string SelectedResourceCentre { get; set; } = string.Empty;

        /// <summary>Re-plan staff grid (Section 2).</summary>
        public DataGridConfig<ResourceMgmtReplanGridItem> RePlanGrid { get; set; } = new();

        /// <summary>All-time staff jobs grid (Section 3).</summary>
        public DataGridConfig<ResourceMgmtReplanAllTimeItem> AllTimeGrid { get; set; } = new();
    }
}
