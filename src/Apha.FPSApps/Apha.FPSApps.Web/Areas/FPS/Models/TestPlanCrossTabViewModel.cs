using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestPlanCrossTabViewModel
    {
        public DataGridConfig<Dictionary<string, string?>> Grid { get; set; } = new();
    }
}