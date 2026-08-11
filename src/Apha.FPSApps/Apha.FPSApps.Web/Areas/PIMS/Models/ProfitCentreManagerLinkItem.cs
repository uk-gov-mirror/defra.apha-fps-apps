using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ProfitCentreManagerLinkItem
    {
        [Required(ErrorMessage = "Resource Centre is required")]
        [Display(Name = "Resource Centre")]
        [GridColumn(Order = 1, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ProfitCentre { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true, IsVisible = false)]
        public string? Manager { get; set; }
    }
}
