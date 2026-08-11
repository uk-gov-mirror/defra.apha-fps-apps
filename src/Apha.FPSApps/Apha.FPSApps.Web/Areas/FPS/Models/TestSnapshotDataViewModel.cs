using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestSnapshotDataViewModel
    {
        public DataGridConfig<TestSnapshotItem> SnapShotTestDataGrid { get; set; } = new DataGridConfig<TestSnapshotItem>();
    }
}
