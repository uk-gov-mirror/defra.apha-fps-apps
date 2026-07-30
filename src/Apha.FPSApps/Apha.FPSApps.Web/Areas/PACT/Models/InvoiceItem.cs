using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class InvoiceItem
    {
        [Display(Name = "InvCntr")]
        [GridColumn(Order = 8, Width = 100, Type = GridColumnType.ReadOnly, IsVisible = true)]
        public int InvoiceCounter { get; set; }

        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 289, Type = GridColumnType.Text, IsFilterable = false)]
        public string ProjectParent { get; set; } = null!;

        [Required(ErrorMessage = "Month is required")]        
        [GridColumn(Order = 2, Width = 75, Type = GridColumnType.Number, IsFilterable = false)]
        public int? Month { get; set; }

        [Required(ErrorMessage = "Amount is required")]        
        [GridColumn(Order = 3, Width = 89, Type = GridColumnType.GbpValue)]
        public decimal? Amount { get; set; }
        
        [GridColumn(Order = 4, Width = 129, Type = GridColumnType.GbpValue)]
        public decimal? CostOfWork { get; set; }

        [Display(Name = "WIP")]
        [GridColumn(Order = 5, Width = 86, Type = GridColumnType.GbpValue)]
        public decimal? Wip { get; set; }
        
        [GridColumn(Order = 6, Width = 109, Type = GridColumnType.GbpValue)]
        public decimal? ProfitLoss { get; set; }

        [StringLength(100)]
        [GridColumn(Order = 7, Width = 177, Type = GridColumnType.Text, IsFilterable = false)]
        public string? Detail { get; set; }
    }
}
