using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;

public class Csg7GroupItem
{
    [Required(ErrorMessage = "CSG7 Group is required.")]
    [Display(Name = "CSG7 Group")]
    [GridColumn(Order = 1, Width = 240, Type = GridColumnType.Text, IsFilterable = true)]
    public string Csg7Group { get; set; } = null!;

    [Display(Name = "Use Inflation?")]
    [GridColumn(Order = 2, Width = 150, Type = GridColumnType.Checkbox, IsFilterable = false)]
    public bool UseInflation { get; set; }
}
