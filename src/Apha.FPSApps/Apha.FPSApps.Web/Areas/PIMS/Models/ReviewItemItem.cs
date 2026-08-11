using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ReviewItemItem
    {
        [Display(Name = "Item Id")]
        [GridColumn(Order = 1, Width = 80, Type = GridColumnType.Number, IsFilterable = true)]
        public int Itemid { get; set; }

        [Required(ErrorMessage = "Item value is required")]
        [Display(Name = "Item")]
        [GridColumn(Order = 2, Width = 280, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Item { get; set; }
    }
}
