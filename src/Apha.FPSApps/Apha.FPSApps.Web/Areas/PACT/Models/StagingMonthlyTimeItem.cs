using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class StagingMonthlyTimeItem
    {
        [GridColumn(IsVisible = false)]
        public int Id { get; set; }

        [Display(Name = "Work Group")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "ID")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? PactStaffId { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Order = 3, Width = 180, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "Time Code")]
        [GridColumn(Order = 4, Width = 120, Type = GridColumnType.Text, IsFilterable = true, CssClass = "grid-column-truncate-tooltip")]
        public string? TimeCode { get; set; }

        [Display(Name = "Parent Project")]
        [GridColumn(Order = 5, Width = 140, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ParentProject { get; set; }

        [Display(Name = "Period")]
        [GridColumn(Order = 6, Width = 80, Type = GridColumnType.Number, CssClass = "monthly-time-staging-period-right-align")]
        public double? Month { get; set; }

        [Display(Name = "Hours")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Hours must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 7, Width = 90, Type = GridColumnType.Number, CssClass = "monthly-time-staging-hours-right-align")]
        public decimal? Hours { get; set; }

        [Display(Name = "Pass")]
        [GridColumn(Order = 8, Width = 70, Type = GridColumnType.Checkbox)]
        public bool Passed { get; set; }

        [Display(Name = "Pact ID")]
        [GridColumn(Order = 9, Width = 100, Type = GridColumnType.Text)]
        public string? PactId { get; set; }

        [Display(Name = "Comments")]
        [GridColumn(Order = 10, Width = 260, Type = GridColumnType.Text, CssClass = "aabbcc grid-column-truncate-tooltip")]
        public string? FailureComments { get; set; }

        [GridColumn(IsVisible = false)]
        public string? Filename { get; set; }

        [GridColumn(IsVisible = false)]
        public string? ImportedBy { get; set; }

        [GridColumn(IsVisible = false)]
        public DateTime? ImportedDate { get; set; }

        [GridColumn(IsVisible = false)]
        public bool NameUpdating { get; set; }
    }
}
