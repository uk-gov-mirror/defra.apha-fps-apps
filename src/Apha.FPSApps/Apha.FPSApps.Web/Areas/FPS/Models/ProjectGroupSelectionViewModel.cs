using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for Project Group Selection - a read-only navigation interface
    /// that lists projects filtered by selected project group.
    /// </summary>
    public class ProjectGroupSelectionViewModel
    {
        /// <summary>
        /// Currently selected project group name (filter value)
        /// </summary>
        public string SelectedProjectGroup { get; set; } = string.Empty;

        /// <summary>
        /// Currently selected / searched project name
        /// </summary>
        public string ProjectSearch { get; set; } = string.Empty;

        /// <summary>
        /// List of all project groups for the dropdown
        /// </summary>
        public List<SelectListItem> ProjectGroupList { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// DataGrid configuration for the projects table
        /// </summary>
        public DataGridConfig<ProjectGroupSelectionProjectItem>? ProjectsGrid { get; set; }
    }

    /// <summary>
    /// Simplified project item for the Project Group Selection list
    /// </summary>
    public class ProjectGroupSelectionProjectItem
    {
        /// <summary>
        /// Project group name
        /// </summary>
        [Display(Name = "Project Group")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = false)]
        public string ProjectGroup { get; set; } = string.Empty;

        /// <summary>
        /// Project code
        /// </summary>
        [Display(Name = "Project Code")]
        [GridColumn(Order = 2, Width = 150, Type = GridColumnType.Text, IsFilterable = false)]
        public string ParentProject { get; set; } = string.Empty;
    }
}
