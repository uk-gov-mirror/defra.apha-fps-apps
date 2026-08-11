using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid row item for the Snapshot Tests DataGrid.
    /// Property names match <c>TestFeePlanViewDto</c> for AutoMapper convention mapping.
    /// </summary>
    public class TestSnapshotItem
    {
        [Display(Name = "Version")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Version { get; set; }

        [Display(Name = "Directorate")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Directorate { get; set; }

        [Display(Name = "Customer")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Customer { get; set; }

        [Display(Name = "Program")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }

        [Display(Name = "Contract")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Contract { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Project { get; set; }

        [Display(Name = "Status")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Status { get; set; }

        [Display(Name = "Test Code")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "No. Tests")]
        [GridColumn(Width = 90, Type = GridColumnType.DecimalNumber)]
        public double? NoTests { get; set; }

        [Display(Name = "Test Fee")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public double? TestFee { get; set; }

        [Display(Name = "Owner")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Owner { get; set; }
    }
}
