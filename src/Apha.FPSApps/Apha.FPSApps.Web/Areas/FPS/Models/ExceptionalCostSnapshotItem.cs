using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid row item for the Exceptional Cost Snapshot DataGrid.
    /// Property names match <c>ProjectExceptionalCostViewDto</c> for AutoMapper convention mapping
    /// registered in <c>FpsViewModelMapper</c>.
    /// </summary>
    public class ExceptionalCostSnapshotItem
    {
        [Display(Name = "Directorate")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Directorate { get; set; }

        [Display(Name = "Program")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Programme { get; set; }

        [Display(Name = "Contract")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ContractNumber { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Project { get; set; }

        [Display(Name = "Account")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? AccountCat { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Description { get; set; }

        [Display(Name = "Item Cost")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? ItemCost { get; set; }
    }
}
