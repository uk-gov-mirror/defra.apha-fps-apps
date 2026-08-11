using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class StagingMonthlyOutputItem
    {
        [GridColumn(IsVisible = false)]
        public int Id { get; set; }

        [Display(Name = "WG")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Test Code")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TestCode { get; set; }

        [Display(Name = "Buyer")]
        [GridColumn(Order = 3, Width = 140, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Buyer { get; set; }

        [Display(Name = "Period")]
        [GridColumn(Order = 4, Width = 80, Type = GridColumnType.Number)]
        public double? Month { get; set; }

        [Display(Name = "Volume")]
        [GridColumn(Order = 5, Width = 90, Type = GridColumnType.Number)]
        public double? Volume { get; set; }

        [Display(Name = "Pass")]
        [GridColumn(Order = 6, Width = 70, Type = GridColumnType.Checkbox)]
        public bool Passed { get; set; }

        [Display(Name = "Comments")]
        [GridColumn(Order = 7, Width = 260, Type = GridColumnType.Text, CssClass = "grid-column-truncate-tooltip")]
        public string? FailureComments { get; set; }

        [GridColumn(IsVisible = false)]
        public string? Filename { get; set; }

        [GridColumn(IsVisible = false)]
        public string? ImportedBy { get; set; }

        [GridColumn(IsVisible = false)]
        public DateTime? ImportedDate { get; set; }
    }
}
