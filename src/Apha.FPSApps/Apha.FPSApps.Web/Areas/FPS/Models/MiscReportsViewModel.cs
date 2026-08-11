using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class MiscReportsViewModel
    {
        public DataGridConfig<Dictionary<string, string?>> Grid { get; set; } = new DataGridConfig<Dictionary<string, string?>>();

        public List<SelectListItem> ProfitCentreOptions { get; set; } = new();

        public string? SelectedProfitCentre { get; set; }

        public string? SelectedReport { get; set; }

        public int FpsYear { get; set; }
    }
}
