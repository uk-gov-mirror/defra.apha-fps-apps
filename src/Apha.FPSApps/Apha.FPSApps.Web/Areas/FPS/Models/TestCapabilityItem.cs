using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid item for the FPS "Set Up Portfolio Components" page.
    /// Data is sourced from the PACT TestCapability service, filtered by PlanPortfolio.
    /// </summary>
    public class TestCapabilityItem
    {
        [Display(Name = "Work Group")]
        [Required(ErrorMessage = "Work Group is required.")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string WorkGroup { get; set; } = null!;

        [Display(Name = "Portfolio")]
        [Required(ErrorMessage = "Plan Portfolio is required.")]
        [GridColumn(Order = 2, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string PlanPortfolio { get; set; } = null!;

        [Display(Name = "Unit Cost")]
        [GridColumn(Order = 3, Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? UnitCost { get; set; }

        // ── Hidden ────────────────────────────────────────────────────────────

        [GridColumn(IsVisible = false)]
        public string TestCode { get; set; } = null!;

        [GridColumn(IsVisible = false)]
        public string? ItemDescription { get; set; }

        [GridColumn(IsVisible = false)]
        public double? PredOutturn { get; set; }

        [GridColumn(IsVisible = false)]
        public string? Sop { get; set; }

        [GridColumn(IsVisible = false)]
        public string? SmsCode { get; set; }

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }
    }
}
