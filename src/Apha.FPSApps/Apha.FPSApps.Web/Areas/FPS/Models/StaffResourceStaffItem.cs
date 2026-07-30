using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Represents one row in the Staff Resource Utilisation grid.
    /// Columns match the prototype resource_utilization_view staff grid.
    /// Data is populated per-workgroup selection; the grid is rendered empty on initial load.
    /// </summary>
    public class StaffResourceStaffItem
    {
        // ── Baseline columns (colspan 6 in grouped header) ──────────────────

        [Display(Name = "WGGrade")]
        [GridColumn(Order = 1, Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WgGrade { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Order = 2, Width = 160, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "Tot H")]
        [GridColumn(Order = 3, Width = 70, Type = GridColumnType.DecimalNumber)]
        public double? TotalH { get; set; }

        [Display(Name = "ZTW")]
        [GridColumn(Order = 4, Width = 65, Type = GridColumnType.DecimalNumber)]
        public double? Ztw { get; set; }

        [Display(Name = "Avail")]
        [GridColumn(Order = 5, Width = 70, Type = GridColumnType.DecimalNumber)]
        public double? Avail { get; set; }

        [Display(Name = "Left")]
        [GridColumn(Order = 6, Width = 70, Type = GridColumnType.DecimalNumber)]
        public double? Left { get; set; }

        // ── Approved columns (colspan 2 in grouped header) ──────────────────

        [Display(Name = "Plan")]
        [GridColumn(Order = 7, Width = 80, Type = GridColumnType.DecimalNumber)]
        public double? ApprovedPlan { get; set; }

        [Display(Name = "Util")]
        [GridColumn(Order = 8, Width = 80, Type = GridColumnType.Percentage)]
        public double? ApprovedUtil { get; set; }

        // ── Not Approved columns (colspan 2 in grouped header) ──────────────

        [Display(Name = "Plan")]
        [GridColumn(Order = 9, Width = 80, Type = GridColumnType.DecimalNumber)]
        public double? NotApprovedPlan { get; set; }

        [Display(Name = "Util")]
        [GridColumn(Order = 10, Width = 80, Type = GridColumnType.Percentage)]
        public double? NotApprovedUtil { get; set; }

        // ── Total columns (colspan 2 in grouped header) ──────────────────────

        [Display(Name = "Plan")]
        [GridColumn(Order = 11, Width = 80, Type = GridColumnType.DecimalNumber)]
        public double? TotalPlan { get; set; }

        [Display(Name = "Util")]
        [GridColumn(Order = 12, Width = 80, Type = GridColumnType.Percentage)]
        public double? TotalUtil { get; set; }
    }
}
