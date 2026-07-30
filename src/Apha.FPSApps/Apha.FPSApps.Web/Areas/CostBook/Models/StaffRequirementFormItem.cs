using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

public class StaffRequirementFormItem
{
    public int SrIdentity { get; set; }

    [Display(Name = "WG Grade")]
    [Required(ErrorMessage = "WG Grade is required.")]
    public string WgGrade { get; set; } = null!;

    [Display(Name = "Name")]
    public string? Name { get; set; }

    [Display(Name = "Rate")]
    public double? Chargerate { get; set; }

    [Display(Name = "Hrs")]
    public double? Nohours { get; set; }

    [Display(Name = "Days")]
    public double? Nodays { get; set; }

    [Display(Name = "Cost")]
    public double? StaffCost { get; set; }

    public double? Payrate { get; set; }

    public double? Npr { get; set; }

    public double? Ohr { get; set; }
}
