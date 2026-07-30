using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class ProjectSubContractItem
    {
        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 381, Type = GridColumnType.Text)]
        public string? Project { get; set; }

        [Required(ErrorMessage = "Month is required")]
        [Display(Name = "Month")]
        [GridColumn(Order = 2, Width = 99, Type = GridColumnType.Number, IsFilterable = true)]
        public double? Month { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Display(Name = "Amount")]
        [GridColumn(Order = 3, Width = 117, Type = GridColumnType.GbpValue)]
        public decimal? Amount { get; set; }

        [Display(Name = "AcctCode")]
        [GridColumn(Order = 4, Width = 295, Type = GridColumnType.Text, IsFilterable = true)]
        public string? AcctCode { get; set; }

        [Display(Name = "Test")]
        [GridColumn(Order = 5, Width = 73, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TestJob { get; set; }

        [Display(Name = "Counter")]
        [GridColumn(Order = 6, Width = 119, Type = GridColumnType.Number)]
        public int SubContCounter { get; set; }
    }
}
