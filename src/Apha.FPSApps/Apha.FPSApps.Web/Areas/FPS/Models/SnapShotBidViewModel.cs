using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class SnapShotBidViewModel
    {
        public DataGridConfig<GenericBidItem> SnapShotBidGrid { get; set; } = new DataGridConfig<GenericBidItem>();
    }
}
