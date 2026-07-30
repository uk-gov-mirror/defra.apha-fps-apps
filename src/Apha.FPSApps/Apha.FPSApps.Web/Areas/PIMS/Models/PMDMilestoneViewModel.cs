using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class PMDMilestoneViewModel
    {
        public string Parentproject { get; set; } = string.Empty;
        public List<SelectListItem> ProjectOptions { get; set; } = [];
        public DataGridConfig<MilestoneItem> MilestonesGrid { get; set; } = new();

        public bool ShowConfirmationSection { get; set; }
        public string ConfirmationLabelText { get; set; } = string.Empty;
    }
}
