using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class SubContractItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SubContCounter { get; set; }

        [Required(ErrorMessage = "Project is required")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 289, Type = GridColumnType.Text, IsFilterable = false)]
        public string? Project { get; set; }

        [Required(ErrorMessage = "Month is required")]
        [Display(Name = "Month")]
        [GridColumn(Order = 2, Width = 75, Type = GridColumnType.Number, IsFilterable = false)]
        public double? Month { get; set; }

        [Display(Name = "Acct Code")]
        [GridColumn(Order = 3, Width = 129, Type = GridColumnType.Text, IsFilterable=true)]
        public string? AcctCode { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Display(Name = "Amount")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Amount must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 4, Width = 89, Type = GridColumnType.GbpValue)]
        public decimal? Amount { get; set; }

        [Display(Name = "Work Group")]
        [GridColumn(Order = 5, Width = 109, Type = GridColumnType.Text,IsFilterable =true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Test Job")]
        [GridColumn(Order = 6, Width = 86, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TestJob { get; set; }

        [Display(Name = "SCtCntr")]
        [GridColumn(Order = 7, Width = 109, Type = GridColumnType.Number)]
        public int Counter { get; set; }

        [StringLength(200)]
        [Display(Name = "Description")]
        [GridColumn(Order = 8, Width = 177, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Description { get; set; }

        [StringLength(100)]
        [Display(Name = "Supplier")]
        [GridColumn(Order = 9, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Supplier { get; set; }

        [Display(Name = "Supplier Number")]
        [GridColumn(Order = 10, Width = 150, Type = GridColumnType.Number, IsFilterable=true)]
        public int? SupplierNumber { get; set; }
    }
}
