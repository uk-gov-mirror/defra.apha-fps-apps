using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class YearEndInitiationViewModel
    {
        public int PlannedYear { get; set; }
        public bool CanInitiate { get; set; }
        public bool CanApprove { get; set; }
        public required DataGridConfig<YearEndConfigValueItem> ConfigValuesGrid { get; set; }
        public required DataGridConfig<YearEndMonthWorkingItem> MonthHoursGrid { get; set; }
        public required DataGridConfig<YearEndHistoryItem> HistoryGrid { get; set; }
    }

    public class YearEndConfigValueItem
    {
        [Display(Name = "Fps Year")]
        [GridColumn(Order = 1, Width = 60, Type = GridColumnType.Text)]
        public int FpsYear { get; set; }

        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.Text)]
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Value")]
        [GridColumn(Order = 3, Width = 200, Type = GridColumnType.Text)]
        public string? Setting { get; set; }

        [Display(Name = "Exists For Planned Year")]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.Text)]
        public string ExistsForPlannedYear { get; set; } = string.Empty;
    }

    public class YearEndMonthWorkingItem
    {
        [Display(Name = "Fps Year")]
        [GridColumn(Order = 1, Width = 60, Type = GridColumnType.Text)]
        public int FpsYear { get; set; }

        [Display(Name = "Year")]
        [GridColumn(Order = 2, Width = 60, Type = GridColumnType.Text)]
        public short Year { get; set; }

        [Display(Name = "Month Name")]
        [GridColumn(Order = 3, Width = 120, Type = GridColumnType.Text)]
        public string MonthName { get; set; } = string.Empty;

        [Display(Name = "Month")]
        [GridColumn(Order = 4, Width = 60, Type = GridColumnType.Text)]
        public short Month { get; set; }

        [Display(Name = "Fmonth")]
        [GridColumn(Order = 5, Width = 60, Type = GridColumnType.Text)]
        public short? Fmonth { get; set; }

        [Display(Name = "Days")]
        [GridColumn(Order = 6, Width = 80, Type = GridColumnType.Text)]
        public decimal? Days { get; set; }

        [Display(Name = "CVL Hours")]
        [GridColumn(Order = 7, Width = 100, Type = GridColumnType.Text)]
        public decimal? CvlHours { get; set; }

        [Display(Name = "VID Hours")]
        [GridColumn(Order = 8, Width = 100, Type = GridColumnType.Text)]
        public decimal? VidHours { get; set; }

        [Display(Name = "Exists For Planned Year")]
        [GridColumn(Order = 9, Width = 100, Type = GridColumnType.Text)]
        public string ExistsForPlannedYear { get; set; } = string.Empty;
    }

    public class YearEndEditConfigValueModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Value { get; set; }
        public bool IsYesNo { get; set; }
        public int FpsYear { get; set; }
    }

    public class YearEndEditMonthHourModel
    {
        public string MonthName { get; set; } = string.Empty;
        public short Year { get; set; }
        public short Month { get; set; }
        public short? Fmonth { get; set; }
        public int FpsYear { get; set; }
        public decimal? Days { get; set; }
        public decimal? CvlHours { get; set; }
        public decimal? VidHours { get; set; }
    }

    public class YearEndHistoryItem
    {
        [Display(Name = "Job Name")]
        [GridColumn(Order = 1, Width = 180, Type = GridColumnType.Text)]
        public string JobName { get; set; } = null!;

        [Display(Name = "Requested By")]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.Text)]
        public string RequestedBy { get; set; } = null!;

        [Display(Name = "Start Date/Time")]
        [GridColumn(Order = 3, Width = 180, Type = GridColumnType.Text)]
        public DateTime StartDateTime { get; set; }

        [Display(Name = "End Date/Time")]
        [GridColumn(Order = 4, Width = 180, Type = GridColumnType.Text)]
        public DateTime? EndDateTime { get; set; }

        [Display(Name = "Status")]
        [GridColumn(Order = 5, Width = 120, Type = GridColumnType.Text)]
        public string Status { get; set; } = null!;

        [Display(Name = "Error Message")]
        [GridColumn(Order = 6, Width = 300, Type = GridColumnType.Text)]
        public string? ErrorMessage { get; set; }
    }
}
