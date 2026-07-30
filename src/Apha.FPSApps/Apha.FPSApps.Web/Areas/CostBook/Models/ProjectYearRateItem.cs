using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

public class ProjectYearRateItem : IValidatableObject
{
    [GridColumn(IsVisible = false)]
    public string Project { get; set; } = null!;

    [Display(Name = "Year")]
    [GridColumn(Order = 1, Width = 80, Type = GridColumnType.Text)]
    public int YearValue { get; set; }

    [Display(Name = "Time")]
    [GridColumn(Order = 2, Width = 110, Type = GridColumnType.Number)]
    public double? MarkupTime { get; set; }

    [Display(Name = "Tests")]
    [GridColumn(Order = 3, Width = 110, Type = GridColumnType.Number)]
    public double? MarkupTests { get; set; }

    [Display(Name = "Animal")]
    [GridColumn(Order = 4, Width = 120, Type = GridColumnType.Number)]
    public double? MarkupAnimals { get; set; }

    [Display(Name = "Additional")]
    [GridColumn(Order = 5, Width = 130, Type = GridColumnType.Number)]
    public double? MarkupAdditional { get; set; }

    [Display(Name = "Time")]
    [GridColumn(Order = 6, Width = 110, Type = GridColumnType.Number)]
    public double? ProfitTime { get; set; }

    [Display(Name = "Tests")]
    [GridColumn(Order = 7, Width = 110, Type = GridColumnType.Number)]
    public double? ProfitTests { get; set; }

    [Display(Name = "Animal")]
    [GridColumn(Order = 8, Width = 120, Type = GridColumnType.Number)]
    public double? ProfitAnimals { get; set; }

    [Display(Name = "Additional")]
    [GridColumn(Order = 9, Width = 130, Type = GridColumnType.Number)]
    public double? ProfitAdditional { get; set; }

    [GridColumn(IsVisible = false)]
    public string? Programme { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        bool isComm = string.Equals(Programme, "comm", StringComparison.OrdinalIgnoreCase);

        if (!isComm) yield break;

        if (MarkupTime == null)
            yield return new ValidationResult("Markup Time is required.", [nameof(MarkupTime)]);

        if (MarkupTests == null)
            yield return new ValidationResult("Markup Tests is required.", [nameof(MarkupTests)]);

        if (MarkupAnimals == null)
            yield return new ValidationResult("Markup Animals is required.", [nameof(MarkupAnimals)]);

        if (MarkupAdditional == null)
            yield return new ValidationResult("Markup Additional is required.", [nameof(MarkupAdditional)]);

        if (ProfitTime == null)
            yield return new ValidationResult("Profit Time is required.", [nameof(ProfitTime)]);

        if (ProfitTests == null)
            yield return new ValidationResult("Profit Tests is required.", [nameof(ProfitTests)]);

        if (ProfitAnimals == null)
            yield return new ValidationResult("Profit Animals is required.", [nameof(ProfitAnimals)]);

        if (ProfitAdditional == null)
            yield return new ValidationResult("Profit Additional is required.", [nameof(ProfitAdditional)]);
    }
}
