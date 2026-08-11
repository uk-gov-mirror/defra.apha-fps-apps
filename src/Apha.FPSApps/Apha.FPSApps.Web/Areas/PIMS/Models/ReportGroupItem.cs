using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ReportGroupItem
    {
        [Display(Name = "Group ID")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Number, IsFilterable = true, IsVisible = false)]
        public int GroupId { get; set; }

        [GridColumn(Order = 2, Width = 100, Type = GridColumnType.Number, IsFilterable = true, IsVisible = false)]
        public int Reportid { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Order = 3, Width = 300, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Description { get; set; }
    }
}