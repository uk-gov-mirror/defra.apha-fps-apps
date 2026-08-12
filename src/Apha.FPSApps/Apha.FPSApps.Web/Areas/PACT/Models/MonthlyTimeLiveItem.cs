using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class MonthlyTimeLiveItem
    {
        [GridColumn(IsVisible = false)]
        public string CompositeKey { get; set; } = string.Empty;

        [Display(Name = "Work Group")]
        [GridColumn(Order = 1, Width = 200, Type = GridColumnType.Text, CssClass = "monthly-time-live-col-workgroup")]
        public string? WorkGroup { get; set; }

        [Display(Name = "PACT Id")]
        [GridColumn(Order = 7, Width = 200, Type = GridColumnType.Text, CssClass = "monthly-time-live-col-pactstaffid")]
        public string PactStaffId { get; set; } = string.Empty;

        [Display(Name = "PACT Staff Id")]
        [GridColumn(Order = 2, Width = 250, Type = GridColumnType.Text, CssClass = "monthly-time-live-col-name")]
        public string? Name { get; set; } 

        [Display(Name = "Time Code")]
        [GridColumn(Order = 3, Width = 230, Type = GridColumnType.Text, CssClass = "monthly-time-live-col-timecode")]
        public string TimeCode { get; set; } = string.Empty;

        [Display(Name = "Parent Project")]
        [GridColumn(Order = 4, Width = 250, Type = GridColumnType.Text, CssClass = "monthly-time-live-col-parentproject")]
        public string ParentProject { get; set; } = string.Empty;

        [Display(Name = "Period")]
        [GridColumn(Order = 5, Width = 90, Type = GridColumnType.Number, CssClass = "monthly-time-live-col-period")]
        public double Month { get; set; }

        [Display(Name = "Hours")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Hours must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 6, Width = 90, Type = GridColumnType.Number, CssClass = "monthly-time-live-col-hours")]
        public decimal? Hours { get; set; }

        [GridColumn(IsVisible = false)]
        public int? FpsYear { get; set; }
    }
}
