using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ProjectManagerItem
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Manager")]
        [GridColumn(Order = 1, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Projectmanager { get; set; }

        [Display(Name = "Manager’s Email")]
        [GridColumn(Order = 2, Width = 220, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Email { get; set; }

        [Display(Name = "MNumber")]
        [GridColumn(Order = 3, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Mnumber { get; set; }

        [Display(Name = "LoginEmail")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [GridColumn(Order = 4, Width = 220, Type = GridColumnType.Text, IsFilterable = true)]
        public string? LoginEmail { get; set; }

        [Display(Name = "Disable")]
        [GridColumn(Order = 5, Width = 80, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Disable { get; set; }
    }
}
