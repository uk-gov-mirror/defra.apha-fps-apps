using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectSnapshotDataViewModel
    {
        public DataGridConfig<ProjectSnapshotItem> SnapShotProjectDataGrid { get; set; } = new DataGridConfig<ProjectSnapshotItem>();
    }
}
