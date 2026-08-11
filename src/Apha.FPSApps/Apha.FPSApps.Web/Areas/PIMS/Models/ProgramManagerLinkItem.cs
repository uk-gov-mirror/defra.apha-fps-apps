using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ProgramManagerLinkItem
    {
        [Required(ErrorMessage = "Programme is required")]
        [Display(Name = "Programme")]
        [GridColumn(Order = 1, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Program { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true,IsVisible =false)]
        public string? Manager { get; set; }
    }
}
