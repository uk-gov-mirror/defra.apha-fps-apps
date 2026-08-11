using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class OtherReportGroupItem
    {
        [Display(Name = "Group ID")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Number, IsFilterable = true, IsVisible = true)]
        public int GroupId { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "Description is required.")]
        [GridColumn(Order = 2, Width = 300, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Description { get; set; }
    }
}
