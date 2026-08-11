using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class WgStaffPlanViewItem
    {
        [Display(Name = "WorkGroup")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "GradeCode")]
        [GridColumn(Order = 2, Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? GradeCode { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Order = 3, Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Order = 4, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Manager { get; set; }

        [Display(Name = "Program")]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }

        [Display(Name = "Jobcode")]
        [GridColumn(Order = 6, Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? JobCode { get; set; }

        [Display(Name = "ProjectStatus")]
        [GridColumn(Order = 7, Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectStatus { get; set; }

        [Display(Name = "Hrs")]
        [GridColumn(Order = 8, Width = 90, Type = GridColumnType.ReadOnly)]
        public double? PlannedHours { get; set; }

        [Display(Name = "Fee")]
        [GridColumn(Order = 9, Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? Fee { get; set; }
    }
}
