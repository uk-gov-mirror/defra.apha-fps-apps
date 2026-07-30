using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class MonthlyPactItem
    {
        [Display(Name = "Month")]
        [GridColumn(Width = 80, Type = GridColumnType.ReadOnly)]
        public double Monthno { get; set; }

        [Display(Name = "Month Name")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly)]
        public string? Periodname { get; set; }

        [Display(Name = "Proj Specific")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? Nonanimals { get; set; }

        [Display(Name = "Animals")]
        [GridColumn(Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? Animals { get; set; }

        [Display(Name = "TimeCosts")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? Timecosts { get; set; }

        [Display(Name = "Test Costs")]
        [GridColumn(Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? Transfercosts { get; set; }

        [Display(Name = "Total Cost")]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? Totalcost { get; set; }

        [Display(Name = "Total Hours")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly)]
        public double? Totalhours { get; set; }

        [Display(Name = "Invoices")]
        [GridColumn(Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? Invoices { get; set; }

        [Display(Name = "COIW")]
        [GridColumn(Width = 80, Type = GridColumnType.GbpValue)]
        public decimal? Coiw { get; set; }
    }
}
