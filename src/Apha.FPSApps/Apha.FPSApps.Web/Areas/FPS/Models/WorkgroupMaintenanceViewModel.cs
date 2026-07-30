using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the WorkGroup Maintenance page (frmMaintWorkGroup2).
    /// Holds the DataGrid configuration for the workgroup list grid.
    /// No page-level filter dropdowns — all lookup data is served via AJAX from the Add/Edit modal.
    /// </summary>
    public class WorkgroupMaintenanceViewModel
    {
        // TRANSFORMENGINE: DataGridConfig built explicitly in WorkgroupMaintenanceController.Index() — never left as new()
        public DataGridConfig<WorkgroupMaintenanceItem> WorkgroupGrid { get; set; } = new DataGridConfig<WorkgroupMaintenanceItem>();
    }
}
