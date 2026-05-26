using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// View model for the Project Cascade page (frmCascadeProject).
    /// Mirrors the three-panel layout: JobCodes → TimeCodeValid → MonthlyTime.
    /// </summary>
    public class ProjectCascadeViewModel
    {
        /// <summary>Currently selected project code.</summary>
        public string? SelectedProjectCode { get; set; }

        /// <summary>Currently selected project title.</summary>
        public string? SelectedProjectTitle { get; set; }

        /// <summary>Panel 1 — Job codes belonging to the selected project.</summary>
        public DataGridConfig<CascadeJobCodeItem> JobCodeGrid { get; set; } = new DataGridConfig<CascadeJobCodeItem>();

        /// <summary>Panel 2 — TimeCodeValid options for the selected job code.</summary>
        public DataGridConfig<CascadeTimeCodeItem> TimeCodeGrid { get; set; } = new DataGridConfig<CascadeTimeCodeItem>();

        /// <summary>Panel 3 — Monthly time records for the selected TimeCodeValid row.</summary>
        public DataGridConfig<CascadeMonthlyTimeItem> MonthlyTimeGrid { get; set; } = new DataGridConfig<CascadeMonthlyTimeItem>();

        /// <summary>All PACT projects — populates the "Select a Project" dropdown.</summary>
        public List<PactProjectViewModel> Projects { get; set; } = new List<PactProjectViewModel>();
    }
}
