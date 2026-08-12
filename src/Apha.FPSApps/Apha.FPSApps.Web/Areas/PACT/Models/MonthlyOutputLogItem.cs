using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class MonthlyOutputLogItem
    {
        [Display(Name = "ID")]
        [GridColumn(Order = 1, Width = 70, Type = GridColumnType.Number)]
        public int SequenceNo { get; set; }

        [Display(Name = "Test Code")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [Display(Name = "Buyer")]
        [GridColumn(Order = 3, Width = 140, Type = GridColumnType.Text, IsFilterable = true)]
        public string Buyer { get; set; } = null!;

        [Display(Name = "Month")]
        [GridColumn(Order = 4, Width = 70, Type = GridColumnType.Number)]
        public double Month { get; set; }

        [Display(Name = "WG")]
        [GridColumn(Order = 5, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string WorkGroup { get; set; } = null!;

        [Display(Name = "Vol")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Volume must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 6, Width = 70, Type = GridColumnType.Number)]
        public double? Volume { get; set; }

        [Display(Name = "Date Imported")]
        [GridColumn(Order = 7, Width = 150, Type = GridColumnType.DateTime)]
        public DateTime? DateTime { get; set; }

        [Display(Name = "MAB User SP No.")]
        [GridColumn(Order = 8, Width = 150, Type = GridColumnType.Text)]
        public string? UserId { get; set; }

        [Display(Name = "Action")]
        [GridColumn(Order = 9, Width = 80, Type = GridColumnType.Text)]
        public string? InsertDelete { get; set; }

        [GridColumn(IsVisible = false)]
        public int? FpsYear { get; set; }
    }
}
