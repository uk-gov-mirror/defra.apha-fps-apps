using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class AccessUserItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SystemId { get; set; }

        [Required(ErrorMessage = "NTLogin is required")]
        [Display(Name = "NTLogin")]
        [GridColumn(Order = 1, Width = 170, Type = GridColumnType.Text, IsFilterable = true)]
        public string? NtLogin { get; set; }

        [Required(ErrorMessage = "UserName is required")]
        [Display(Name = "UserName")]
        [GridColumn(Order = 2, Width = 240, Type = GridColumnType.Text, IsFilterable = true)]
        public string? UserName { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Dt2Login { get; set; }

        [Required(ErrorMessage = "UserEmail is required")]
        [Display(Name = "Email")]
        [GridColumn(Order = 3, Width = 250, Type = GridColumnType.Text, IsFilterable = true)]
        public string? UserEmail { get; set; }

        [GridColumn(IsVisible = false)]
        public string CompositeKey => $"{NtLogin}|{SystemId}";
    }
}

