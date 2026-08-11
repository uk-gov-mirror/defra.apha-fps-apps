using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class MonthlyOutputViewModel
    {
        public DataGridConfig<MonthlyOutputLiveItem> LiveGrid { get; set; } = new();
        public DataGridConfig<StagingMonthlyOutputItem> StagingGrid { get; set; } = new();
        public List<SelectListItem> WorkGroupOptions { get; set; } = new();
        public List<SelectListItem> TestCodeOptions { get; set; } = new();
        public List<SelectListItem> BuyerOptions { get; set; } = new();
        public List<SelectListItem> MonthOptions { get; set; } = new();
        public decimal LiveTotalVolume { get; set; }
        public decimal StagingTotalVolume { get; set; }
    }
}
