using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// View model for displaying time code validity in the Portfolio Time Codes page.
    /// Based on the prototype file: source/ui/PACT/portfolio_time_codes.html
    /// This is a separate view-specific model, not reusing TimeCodeViewModel.
    /// 
    /// Business Rule:
    /// - EITHER JobCode has a value (and Portfolio + TestCode are null)
    /// - OR Portfolio + TestCode have values (and JobCode is null)
    /// </summary>
    public class ValidTimeCodeViewModel : IValidatableObject
    {
        [Display(Name = "Work Group")]
        [Required(ErrorMessage = "Work Group is required")]
        [StringLength(50)]
        [GridColumn(Order = 1, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string WorkGroup { get; set; } = null!;

        [Display(Name = "Active")]
        [Required(ErrorMessage = "Active is required")]
        [GridColumn(Order = 2, Width = 80, Type = GridColumnType.Checkbox, IsFilterable = true)]
        public bool Active { get; set; }

        [Display(Name = "Time Code")]
        [Required(ErrorMessage = "Time Code is required")]
        [StringLength(50)]
        [GridColumn(Order = 3, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string TimeCode { get; set; } = null!;

        [Display(Name = "Project")]
        [Required(ErrorMessage = "Project is required")]
        [StringLength(50)]
        [GridColumn(Order = 4, Width = 160, Type = GridColumnType.Text)]
        public string? Project { get; set; }

        [Display(Name = "Job Code")]
        [StringLength(50)]
        [GridColumn(Order = 5, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string? JobCode { get; set; }

        [Display(Name = "Test Code")]
        [StringLength(50)]
        [GridColumn(Order = 6, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TestCode { get; set; }

        [Display(Name = "Portfolio")]
        [StringLength(50)]
        [GridColumn(Order = 7, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Portfolio { get; set; }

        [Display(Name = "Parent Project")]
        [Required(ErrorMessage = "Parent Project is required")]
        [StringLength(50)]
        [GridColumn(IsVisible = false)]
        public string ParentProject { get; set; } = null!;

        /// <summary>
        /// Holds the WorkGroup value as it existed before editing.
        /// Required to locate and replace the old composite key record
        /// (ParentProject + OriginalWorkGroup + TimeCode) when WorkGroup changes.
        /// Not rendered in the grid.
        /// </summary>
        [GridColumn(IsVisible = false)]
        public string? OriginalWorkGroup { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {  
            var hasJobCode = !string.IsNullOrWhiteSpace(JobCode);
            var hasPortfolio = !string.IsNullOrWhiteSpace(Portfolio);
            var hasTestCode = !string.IsNullOrWhiteSpace(TestCode);

            // Business Rule: Either JobCode OR (Portfolio + TestCode), not both
            if (hasJobCode && (hasPortfolio || hasTestCode))
            {
                yield return new ValidationResult(
                    "Cannot specify JobCode together with Portfolio or TestCode. Please use either JobCode OR Portfolio/TestCode.",
                    new[] { nameof(JobCode), nameof(Portfolio), nameof(TestCode) });
            }

            // If Portfolio or TestCode is provided without JobCode, both should typically be provided
            // (optional: you can adjust this based on your exact requirement)
            if (!hasJobCode && (hasPortfolio || hasTestCode))
            {
                // This is valid - Portfolio and TestCode can be used without JobCode
                // You could add additional rules here if needed
            }
        }
    }
}
