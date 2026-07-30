using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class MonthlyPactDataItem
    {
        [Display(Name = "Month")]
        [GridColumn(Width = 60, Type = GridColumnType.ReadOnly)]
        public double MonthNo { get; set; }

        [Display(Name = "Month Name")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly)]
        public string? PeriodName { get; set; }

        [Display(Name = "Proj Specific")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? NonAnimals { get; set; }

        [Display(Name = "Animals")]
        [GridColumn(Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? Animals { get; set; }

        [Display(Name = "TimeCosts")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? TimeCosts { get; set; }

        [Display(Name = "Test Costs")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? TransferCosts { get; set; }

        [Display(Name = "Total Cost")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? TotalCost { get; set; }

        [Display(Name = "Total Hours")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly)]
        public double? TotalHours { get; set; }

        [Display(Name = "Invoices")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? Invoices { get; set; }

        [Display(Name = "COIW")]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? Coiw { get; set; }
    }
}
