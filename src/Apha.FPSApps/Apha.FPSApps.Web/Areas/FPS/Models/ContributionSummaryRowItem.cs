using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// DataGrid row item for Income/Contribution from Time Sales (frmTimeSellerPC).
    /// Column order matches the Access form layout:
    ///   PC Grade | Work Group | WG Grade (displayed as "Grade") | Avail Hrs | Chrg Rate
    ///   ── Total Planned Time ──  PlanHrs | FEC | % Planned
    ///   ── Assured Planned Time ── App Hrs | App FEC | % Assured
    ///   ── Rate "Efficacy" Checker ── OHR | Total Cont
    /// </summary>
    public class ContributionSummaryRowItem
    {
        // ── Identity / ungrouped ────────────────────────────────────────────

        [Display(Name = "PC Grade")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text, IsFilterable = true, IsVisible = false)]
        public string? ProfitCentreGrade { get; set; }

        [Display(Name = "WG")]
        [GridColumn(Order = 2, Width = 130, Type = GridColumnType.Text, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [Display(Name = "Grade")]
        [GridColumn(Order = 3, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? WgGrade { get; set; }

        [Display(Name = "Avail Hrs")]
        [GridColumn(Order = 4, Width = 85, Type = GridColumnType.DoubleNumber)]
        public double? AvHrs { get; set; }

        [Display(Name = "Chrg Rate")]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? ChargeRate { get; set; }

        // ── Total Planned Time group ────────────────────────────────────────

        [Display(Name = "PlanHrs")]
        [GridColumn(Order = 6, Width = 85, Type = GridColumnType.DoubleNumber)]
        public double? Hrs { get; set; }

        [Display(Name = "FEC")]
        [GridColumn(Order = 7, Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? Fec { get; set; }

        [Display(Name = "% Planned")]
        [GridColumn(Order = 8, Width = 90, Type = GridColumnType.DoubleNumber)]
        public double? PctPlanned { get; set; }

        // ── Assured Planned Time group ──────────────────────────────────────

        [Display(Name = "App Hrs")]
        [GridColumn(Order = 9, Width = 85, Type = GridColumnType.DoubleNumber)]
        public double? AppHours { get; set; }

        [Display(Name = "App FEC")]
        [GridColumn(Order = 10, Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? AppFec { get; set; }

        [Display(Name = "% Assured")]
        [GridColumn(Order = 11, Width = 90, Type = GridColumnType.DoubleNumber)]
        public double? PctAssuredPlanned { get; set; }

        // ── Rate "Efficacy" Checker group ───────────────────────────────────

        [Display(Name = "OH Rate")]
        [GridColumn(Order = 12, Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? Ohr { get; set; }

        [Display(Name = "Total Cont")]
        [GridColumn(Order = 13, Width = 105, Type = GridColumnType.GbpValue)]
        public decimal? Contribution { get; set; }
    }
}
