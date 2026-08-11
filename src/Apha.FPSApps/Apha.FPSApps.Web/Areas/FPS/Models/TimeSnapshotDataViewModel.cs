using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TimeSnapshotDataViewModel
    {
        public DataGridConfig<TimeSnapshotItem> SnapShotTimeDataGrid { get; set; } = new DataGridConfig<TimeSnapshotItem>();
    }
}
