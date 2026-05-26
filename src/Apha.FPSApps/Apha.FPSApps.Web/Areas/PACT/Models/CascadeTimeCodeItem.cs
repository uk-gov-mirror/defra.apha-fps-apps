using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// Grid item for TimeCodeValid options for a selected Job Code (Panel 2 in Project Cascade).
    /// </summary>
    public class CascadeTimeCodeItem
    {
        [Display(Name = "Work Group")]
        [Required]
        [StringLength(50)]
        [GridColumn(Order = 1, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string WorkGroup { get; set; } = null!;

        [Display(Name = "Active")]
        [GridColumn(Order = 2, Width = 80, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Active { get; set; }

        [Display(Name = "Time Code")]
        [Required]
        [StringLength(50)]
        [GridColumn(Order = 3, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string TimeCode { get; set; } = null!;

        [Display(Name = "Job Code")]
        [StringLength(50)]
        [GridColumn(Order = 4, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? JobCode { get; set; }

        [Display(Name = "Parent Project")]
        [GridColumn(IsVisible = false)]
        public string ParentProject { get; set; } = null!;
    }
}
