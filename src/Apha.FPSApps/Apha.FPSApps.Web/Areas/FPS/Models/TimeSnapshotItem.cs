using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid row item for the Snapshot Time DataGrid.
    /// Property names match <c>ProgramPlanCostViewDto</c> for AutoMapper convention mapping.
    /// </summary>
    public class TimeSnapshotItem
    {
        [Display(Name = "Version")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Version { get; set; }

        [Display(Name = "Directorate")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Directorate { get; set; }

        [Display(Name = "Program")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }

        [Display(Name = "Customer")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Customer { get; set; }

        [Display(Name = "Contract")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Contract { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Project { get; set; }

        [Display(Name = "Status")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Status { get; set; }

        [Display(Name = "Resource Centre")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ResourceCentre { get; set; }

        [Display(Name = "Work Group")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Grade Code")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? GradeCode { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "Hours")]
        [GridColumn(Width = 90, Type = GridColumnType.DecimalNumber)]
        public double Hours { get; set; }

        [Display(Name = "Hours Cost")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? HoursCost { get; set; }
    }
}
