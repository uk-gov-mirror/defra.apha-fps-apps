using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class SubContractRmsFailedItem
    {
        [GridColumn(IsVisible = false)]
        public int Id { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text)]
        public string? Project { get; set; }

        [Display(Name = "Test Job")]
        [GridColumn(Order = 2, Width = 110, Type = GridColumnType.Text)]
        public string? TestJob { get; set; }

        [Display(Name = "Month")]
        [RegularExpression(@"^(?:[1-9]|1[0-2])$", ErrorMessage = "Month must be between 1 and 12.")]
        [GridColumn(Order = 3, Width = 80, Type = GridColumnType.Text)]
        public string? Month { get; set; }

        [Display(Name = "Amount")]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.Text)]
        public string? Amount { get; set; }

        [Display(Name = "Work Group")]
        [GridColumn(Order = 5, Width = 120, Type = GridColumnType.Text)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Account Code")]
        [GridColumn(Order = 6, Width = 120, Type = GridColumnType.Text)]
        public string? AcctCode { get; set; }

        [Display(Name = "Supplier")]
        [GridColumn(Order = 7, Width = 120, Type = GridColumnType.Text)]
        public string? Supplier { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Order = 8, Width = 150, Type = GridColumnType.Text)]
        public string? Description { get; set; }

        [Display(Name = "Supplier Number")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Supplier Number must be a valid whole number.")]
        [GridColumn(Order = 9, Width = 120, Type = GridColumnType.Text)]
        public string? SupplierNumber { get; set; }

        [Display(Name = "Daily Rate")]
        [GridColumn(Order = 10, Width = 100, Type = GridColumnType.Text)]
        public string? DailyRate { get; set; }

        [Display(Name = "Animal Days")]
        [GridColumn(Order = 11, Width = 110, Type = GridColumnType.Text)]
        public string? AnimalDays { get; set; }

        [Display(Name = "Validation Failure")]
        [GridColumn(Order = 12, Width = 250, Type = GridColumnType.Text, CssClass = "grid-column-truncate-tooltip")]
        public string? ValidationFailure { get; set; }
    }
}
