using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// Grid item for Monthly Time records attributed to a selected TimeCodeValid (Panel 3 in Project Cascade).
    /// </summary>
    public class CascadeMonthlyTimeItem
    {
        [Display(Name = "PACT Staff ID")]
        [Required]
        [StringLength(50)]
        [GridColumn(Order = 1, Width = 180, Type = GridColumnType.Text, IsFilterable = true)]
        public string PactStaffId { get; set; } = null!;

        [Display(Name = "Month")]
        [GridColumn(Order = 2, Width = 100, Type = GridColumnType.Text, IsFilterable = false)]
        public double Month { get; set; }

        [Display(Name = "Hours")]
        [GridColumn(Order = 3, Width = 100, Type = GridColumnType.Text, IsFilterable = false)]
        public double? Hours { get; set; }

        [Display(Name = "Work Group")]
        [StringLength(50)]
        [GridColumn(Order = 4, Width = 180, Type = GridColumnType.Text, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Time Code")]
        [GridColumn(IsVisible = false)]
        public string TimeCode { get; set; } = null!;

        [Display(Name = "Parent Project")]
        [GridColumn(IsVisible = false)]
        public string ParentProject { get; set; } = null!;
    }
}
