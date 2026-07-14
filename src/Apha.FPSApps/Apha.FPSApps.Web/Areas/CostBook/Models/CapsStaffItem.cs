using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;
public class CapsStaffItem
{
    [Required(ErrorMessage = "mNumber is required.")]
    [Display(Name = "mNumber")]
    [GridColumn(Order = 1, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
    public string MNumber { get; set; } = null!;

    [Required(ErrorMessage = "Name is required.")]
    [Display(Name = "Name")]
    [GridColumn(Order = 2, Width = 260, Type = GridColumnType.Text, IsFilterable = true)]
    public string Name { get; set; } = null!;

    [GridColumn(IsVisible = false)]
    public string? Dt2Number { get; set; }
}
