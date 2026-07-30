using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestReqBreakdownViewModel
    {
        public DataGridConfig<TestReqBreakdownItem> Grid { get; set; } = new DataGridConfig<TestReqBreakdownItem>();
    }
}
