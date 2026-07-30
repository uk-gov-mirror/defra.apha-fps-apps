using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class WgTestCapabilitiesWithDescriptionItem
    {
        [Display(Name = "Work Group")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = false)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Test Code")]
        [GridColumn(Order = 2, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TestCode { get; set; }

        [Display(Name = "Item Description")]
        [GridColumn(Order = 3, Width = 300, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ItemDescription { get; set; }
    }
}