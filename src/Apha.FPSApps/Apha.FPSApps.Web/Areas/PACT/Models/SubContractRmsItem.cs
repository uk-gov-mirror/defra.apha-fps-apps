using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class SubContractRmsItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SubContCounter { get; set; }

        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 180, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Project { get; set; }

        [Display(Name = "Account Code")]
        [GridColumn(Order = 2, Width = 130, Type = GridColumnType.Text, IsFilterable = true)]
        public string? AcctCode { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Display(Name = "Amount")]
        [GridColumn(Order = 3, Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? Amount { get; set; }

        [Required(ErrorMessage = "Month is required")]
        [Display(Name = "Month")]
        [GridColumn(Order = 4, Width = 85, Type = GridColumnType.Number)]
        public double? Month { get; set; }

        [Display(Name = "Test Job")]
        [GridColumn(Order = 5, Width = 110, Type = GridColumnType.Text)]
        public string? TestJob { get; set; }

        [StringLength(200)]
        [Display(Name = "Description")]
        [GridColumn(Order = 6, Width = 180, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Description { get; set; }

        [StringLength(100)]
        [Display(Name = "Supplier")]
        [GridColumn(Order = 7, Width = 140, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Supplier { get; set; }

        [Display(Name = "Supplier Number")]
        [GridColumn(Order = 8, Width = 140, Type = GridColumnType.Number)]
        public int? SupplierNumber { get; set; }

        [Display(Name = "Daily Rate")]
        [GridColumn(Order = 9, Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? DailyRate { get; set; }

        [Display(Name = "Animal Days")]
        [GridColumn(Order = 10, Width = 110, Type = GridColumnType.Number)]
        public int? AnimalDays { get; set; }
    }
}
