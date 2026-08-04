using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid row item for the Snapshot Bid DataGrid.
    /// Property names match <c>GenericBidViewDto</c> for AutoMapper convention mapping
    /// registered in <c>FpsViewModelMapper</c>.
    /// </summary>
    public class GenericBidItem
    {
        [Display(Name = "Profit Centre")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProfitCentre { get; set; }

        [Display(Name = "Work Group")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroupName { get; set; }

        [Display(Name = "Account")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Account { get; set; }

        [Display(Name = "Generic Bid")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal GenBid { get; set; }

        [Display(Name = "Account Type")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? AccountType { get; set; }
    }
}
