
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models;
public class AccountCategoryItem
{
    [Display(Name = "Account Short Name")]
    [GridColumn(Order = 1, Width = 220, Type = GridColumnType.Text, IsFilterable = true)]
    public string AccShortName { get; set; } = null!;
   
    [Display(Name = "Description")]
    [GridColumn(Order = 2, Width = 260, Type = GridColumnType.ReadOnly, IsFilterable = true)]
    public string? AccountDescription { get; set; }

    [Display(Name = "CSG7 Group")]
    [GridColumn(Order = 3, Width = 180, Type = GridColumnType.Dropdown, IsFilterable = true)]
    public string? Csg7Group { get; set; }

    [GridColumn(IsVisible = false)]
    public int FpsYear { get; set; }
}
