using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class PublicationTypeItem
    {
        [Required(ErrorMessage = "Type code is required")]
        [Display(Name = "Type ")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string Type { get; set; } = null!;

        [Display(Name = "PublicationType ")]
        [GridColumn(Order = 2, Width = 280, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Description { get; set; }
    }
}
