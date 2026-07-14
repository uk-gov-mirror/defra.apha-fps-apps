using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Page ViewModel for the Project Audit Trail tabbed view.
    /// Holds filter state (ParentProject, FromDate, ToDate), project dropdown list,
    /// and one DataGridConfig per audit log tab.
    /// All grids are read-only (AllowAdd=false, AllowEdit=false, AllowDelete=false) per prototype.
    /// </summary>
    public class ProjectAuditTrailViewModel
    {
        public string? ParentProject { get; set; }

        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

        // Populated in controller via IProjectService.GetAllProjectsAsync()
        public List<SelectListItem> ProjectList { get; set; } = new();

        // NEVER leave as new() — built explicitly in controller Index()
        public DataGridConfig<ProjectLogItem> ProjectLogsGrid { get; set; } = new();

        // NEVER leave as new() — built explicitly in controller Index()
        public DataGridConfig<StaffJobLogItem> StaffJobLogsGrid { get; set; } = new();

        // NEVER leave as new() — built explicitly in controller Index()
        public DataGridConfig<TestRequirementLogItem> TestRequirementLogsGrid { get; set; } = new();

        // NEVER leave as new() — built explicitly in controller Index()
        public DataGridConfig<AnimalRequestLogItem> AnimalRequestLogsGrid { get; set; } = new();

        // NEVER leave as new() — built explicitly in controller Index()
        public DataGridConfig<AdditionalCostLogItem> AdditionalCostLogsGrid { get; set; } = new();
    }
}
