using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class StaffPlanDetailsViewModel
    {
        public DataGridConfig<StaffPlanDetailsViewItem> Grid { get; set; } = new DataGridConfig<StaffPlanDetailsViewItem>();

        public List<SelectListItem> ProfitCentreOptions { get; set; } = new();

        public string? SelectedProfitCentre { get; set; }
    }
}
