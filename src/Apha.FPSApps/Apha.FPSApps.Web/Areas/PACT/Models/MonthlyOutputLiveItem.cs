using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class MonthlyOutputLiveItem
    {
        [GridColumn(IsVisible = false)]
        public string CompositeKey { get; set; } = string.Empty;

        [Display(Name = "Work Group")]
        [GridColumn(Order = 1, Width = 140, Type = GridColumnType.Text)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Test Code")]
        [GridColumn(Order = 2, Width = 140, Type = GridColumnType.Text)]
        public string? TestCode { get; set; }

        [Display(Name = "Buyer")]
        [GridColumn(Order = 3, Width = 140, Type = GridColumnType.Text)]
        public string? Buyer { get; set; }

        [Display(Name = "Period")]
        [GridColumn(Order = 4, Width = 90, Type = GridColumnType.Number)]
        public double Month { get; set; }

        [Display(Name = "Volume")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Volume must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 5, Width = 90, Type = GridColumnType.Number)]
        public decimal? Volume { get; set; }

        [GridColumn(IsVisible = false)]
        public int? FpsYear { get; set; }
    }
}
