using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class RadTrackProgItem
    {
        [Required(ErrorMessage = "Program is required")]
        [Display(Name = "Program")]
        [GridColumn(Order = 1, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Program { get; set; }

        [Display(Name = "Publication Prefix")]
        [MaxLength(5, ErrorMessage = "Publication Prefix must not exceed 5 characters")]
        [GridColumn(Order = 2, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Publicationprefix { get; set; }

        [Display(Name = "RadTrack")]
        [GridColumn(Order = 3, Width = 90, Type = GridColumnType.Checkbox, IsFilterable = false, IsVisible =false)]
        public bool Radtrackprog { get; set; }
    }
}
