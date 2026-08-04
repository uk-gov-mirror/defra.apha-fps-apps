using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ExceptionalCostSnapshotViewModel
    {
        public DataGridConfig<ExceptionalCostSnapshotItem> ExceptionalCostSnapshotGrid { get; set; } = new DataGridConfig<ExceptionalCostSnapshotItem>();
    }
}
