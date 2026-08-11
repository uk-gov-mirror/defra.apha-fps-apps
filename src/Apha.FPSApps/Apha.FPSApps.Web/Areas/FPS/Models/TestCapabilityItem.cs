using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Validation;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid item for the FPS "Set Up Portfolio Components" page.
    /// Data is sourced from the PACT TestCapability service, filtered by PlanPortfolio.
    /// </summary>
    public class TestCapabilityItem
    {
        [Display(Name = "Test Code")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [Display(Name = "Description")]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ItemDescription { get; set; }

        [Display(Name = "Work Group")]
        [Required(ErrorMessage = "Work Group is required.")]
        [GridColumn(Order = 3, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string WorkGroup { get; set; } = null!;

        // Holds the WorkGroup value the record was loaded with, so the update can
        // locate the original row (WorkGroup is part of the composite key) even when
        // the user changes the WorkGroup in the edit modal.
        [GridColumn(IsVisible = false)]
        public string? OriginalWorkGroup { get; set; }

        [Display(Name = "Portfolio")]
        [Required(ErrorMessage = "Plan Portfolio is required.")]
        [GridColumn(IsVisible = false)]
        public string PlanPortfolio { get; set; } = null!;

        [Display(Name = "Unit Cost")]
        [GridColumn(Order = 4, Width = 110, Type = GridColumnType.GbpValue)]
        [CurrencyRange]
        public decimal? UnitCost { get; set; }

        // ── Hidden ────────────────────────────────────────────────────────────

        [Display(Name = "SOP")]
        [GridColumn(Order = 5, Width = 110, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Sop { get; set; }

        [Display(Name = "SMS Code")]
        [GridColumn(Order = 6, Width = 110, Type = GridColumnType.Text, IsFilterable = true)]
        public string? SmsCode { get; set; }

        [GridColumn(IsVisible = false)]
        public double? PredOutturn { get; set; }

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }
    }
}
