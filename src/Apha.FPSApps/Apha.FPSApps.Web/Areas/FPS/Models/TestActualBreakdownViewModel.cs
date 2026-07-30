using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestActualBreakdownViewModel
    {
        public DataGridConfig<TestActualBreakdownItem> Grid { get; set; } = new DataGridConfig<TestActualBreakdownItem>();
    }
}