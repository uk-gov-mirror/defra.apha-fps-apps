using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// Grid item for Job Codes belonging to a project (Panel 1 in Project Cascade).
    /// </summary>
    public class CascadeJobCodeItem
    {
        [Display(Name = "Job Code")]
        [Required]
        [StringLength(50)]
        [GridColumn(Order = 1, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string JobCodeId { get; set; } = null!;

        [Display(Name = "Job Code Name")]
        [StringLength(255)]
        [GridColumn(Order = 2, Width = 400, Type = GridColumnType.Text, IsFilterable = true)]
        public string? JobCodeName { get; set; }

        [Display(Name = "Work Group")]
        [StringLength(50)]
        [GridColumn(Order = 3, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? JobCodeWorkGroup { get; set; }

        [Display(Name = "Parent Project")]
        [GridColumn(IsVisible = false)]
        public string? ParentProject { get; set; }
    }
}
