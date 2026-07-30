using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ResourceMgmtReplanGridItem
    {
        /// <summary>Composite row key: "{ParentProject}|{WgGrade}".</summary>
        [GridColumn(IsVisible = false)]
        public string? StaffRowKey { get; set; }

        [GridColumn(IsVisible = false)]
        public string? WgGrade { get; set; }

        [Display(Name = "WG")]
        [GridColumn(Width = 50, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Grade")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? GradeCode { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "Hours")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? PlannedHours { get; set; }

        [Display(Name = "Programme")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }

        [GridColumn(IsVisible = true)]
        [Display(Name = "Project")]
        public string? ParentProject { get; set; }

    }
}
