using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class MonthlyTimeViewModel
    {
        public DataGridConfig<MonthlyTimeLiveItem> LiveGrid { get; set; } = new();
        public DataGridConfig<StagingMonthlyTimeItem> StagingGrid { get; set; } = new();
        public List<SelectListItem> WorkGroupOptions { get; set; } = new();
        public List<SelectListItem> StaffOptions { get; set; } = new();
        public List<SelectListItem> TimeCodeOptions { get; set; } = new();
        public List<SelectListItem> ProjectOptions { get; set; } = new();
        public List<SelectListItem> MonthOptions { get; set; } = new();
        public decimal LiveTotalHours { get; set; }
        public decimal StagingTotalHours { get; set; }
        public bool NameUpdating { get; set; }
    }
}
