using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class SettingItem
    {
        [Display(Name = "Setting ID")]
        [GridColumn(Order = 1, Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Id { get; set; }

        
        [Display(Name = "Value")]
        [GridColumn(Order = 2, Width = 150, Type = GridColumnType.Text, IsFilterable = false)]
        public string? SettingValue { get; set; }

        
        [Display(Name = "Notes")]
        [GridColumn(Order = 3, Width = 280, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? Notes { get; set; }

        
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Testsetting { get; set; }

        
        [Display(Name = "User Updateable")]
        [GridColumn(Order = 4, Width = 120, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Userupdateable { get; set; }
    }
}
