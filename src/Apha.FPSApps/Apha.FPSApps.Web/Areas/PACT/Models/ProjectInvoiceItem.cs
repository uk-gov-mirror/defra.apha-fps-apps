using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ProjectInvoiceItem
    {
        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 289, Type = GridColumnType.Text)]
        public string ProjectParent { get; set; } = null!;

        [Required(ErrorMessage = "Month is required")]
        [GridColumn(Order = 2, Width = 75, Type = GridColumnType.Number, IsFilterable = true)]
        public int? Month { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Amount must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 3, Width = 89, Type = GridColumnType.GbpValue)]
        public decimal? Amount { get; set; }

        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Cost of Work must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 4, Width = 129, Type = GridColumnType.GbpValue)]
        public decimal? CostOfWork { get; set; }

        [Display(Name = "WIP")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "WIP must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 5, Width = 86, Type = GridColumnType.GbpValue)]
        public decimal? Wip { get; set; }

        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Profit/Loss must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 6, Width = 109, Type = GridColumnType.GbpValue)]
        public decimal? ProfitLoss { get; set; }

        [GridColumn(Order = 7, Width = 177, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Detail { get; set; }

        [Display(Name = "Invoice Counter")]
        [GridColumn(Order = 8, Width = 159, Type = GridColumnType.Number)]
        public int InvoiceCounter { get; set; }
    }
}