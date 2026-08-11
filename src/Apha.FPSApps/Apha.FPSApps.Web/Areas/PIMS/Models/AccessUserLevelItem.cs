using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class AccessUserLevelItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SystemId { get; set; }

        [Required(ErrorMessage = "User is required")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? NtLogin { get; set; }

        [Display(Name = "User")]
        [GridColumn(Order = 1, Width = 220, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Access Level is required")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int AccessLevelId { get; set; }

        [Display(Name = "Access Level")]
        [GridColumn(Order = 2, Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? AccessLevelName { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool IsEditMode { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int? OriginalSystemId { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? OriginalNtLogin { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int? OriginalAccessLevelId { get; set; }

        [GridColumn(IsVisible = false)]
        public string CompositeKey => $"{NtLogin}|{AccessLevelId}|{SystemId}";
    }
}
