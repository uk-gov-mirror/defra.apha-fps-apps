using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid row item for the Snapshot Animal Data DataGrid.
    /// Property names match <c>AnimalSnapshotViewDto</c> for AutoMapper convention mapping
    /// registered in <c>FpsViewModelMapper</c>.
    /// </summary>
    public class AnimalSnapshotItem
    {
        [Display(Name = "Directorate")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Directorate { get; set; }

        [Display(Name = "Program")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }

        [Display(Name = "Contract")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Contract { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Project { get; set; }

        [Display(Name = "Project Status")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectStatus { get; set; }

        [Display(Name = "Species")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Species { get; set; }

        [Display(Name = "Security Level")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? SecurityLevel { get; set; }

        [Display(Name = "Animal Type")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? AnimalType { get; set; }

        [Display(Name = "Daily Rate")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? DailyRate { get; set; }

        [Display(Name = "Job Code")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? JobCode { get; set; }

        [Display(Name = "Number of Days")]
        [GridColumn(Width = 80, Type = GridColumnType.DecimalNumber)]
        public double NumberOfDays { get; set; }

        [Display(Name = "Number of Animals")]
        [GridColumn(Width = 100, Type = GridColumnType.DecimalNumber)]
        public double NumberOfAnimals { get; set; }

        [Display(Name = "Cost")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? Cost { get; set; }
    }
}
