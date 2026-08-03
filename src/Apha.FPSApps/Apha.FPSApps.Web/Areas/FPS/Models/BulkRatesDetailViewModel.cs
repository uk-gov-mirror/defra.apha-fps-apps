using System.ComponentModel.DataAnnotations;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;
using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class BulkRatesDetailViewModel
    {
        public BulkRatesRequestDetailDto Request { get; set; } = new();
        public string CurrentUserEmail { get; set; } = string.Empty;

        /// <summary>Populated from GetValidationResultsAsync on page load; null if no file uploaded yet.</summary>
        public BulkRatesUploadResultDto? UploadResult { get; set; }

        // ── Staging grids — built explicitly in BulkRatesController, never left as new() ──

        public DataGridConfig<FecStagingGridItem> FecStagingGrid { get; set; } = new();
        public DataGridConfig<AgrupStagingGridItem> AgrupStagingGrid { get; set; } = new();
        public DataGridConfig<StaffStagingGridItem> StaffStagingGrid { get; set; } = new();
        public DataGridConfig<AnimalStagingGridItem> AnimalStagingGrid { get; set; } = new();

        // ── Derived permission flags used by the view ──────────────────────

        public bool IsInitiator =>
            string.Equals(Request.Entry.RequestedBy, CurrentUserEmail, StringComparison.OrdinalIgnoreCase);

        public bool CanUpload =>
            IsInitiator && Request.Entry.Status is "Initiated" or "Rejected";

        public bool CanRelease =>
            IsInitiator
            && Request.Entry.Status == "Initiated"
            && !string.IsNullOrEmpty(UploadResult?.Filename)
            && UploadResult.RowCounts.Total > 0
            && !UploadResult.ValidationErrors.Any(e => string.Equals(e.Severity, "Error", StringComparison.OrdinalIgnoreCase));

        // TEMPORARILY DISABLED the maker-checker (!IsInitiator) restriction so a single
        // admin can self-approve during testing. Restore before release.
        public bool CanApprove =>
            Request.Entry.Status == "ReleasedForApproval";

        // TEMPORARILY DISABLED the maker-checker (!IsInitiator) restriction so a single
        // admin can self-reject during testing. Restore before release.
        public bool CanReject =>
            Request.Entry.Status == "ReleasedForApproval";

        public bool CanCancel =>
            IsInitiator && Request.Entry.Status is "Initiated" or "Rejected" or "Failed";
    }

    /// <summary>
    /// Implemented by staging grid items that can carry a per-row validation summary
    /// (matched from BulkRatesUploadResultDto.ValidationErrors by business key in
    /// BulkRatesController.BuildStagingGridConfig) so it can be set without a generic
    /// TItem constraint on every grid item type.
    /// </summary>
    public interface IHasValidationSummary
    {
        string? ValidationSummary { get; set; }
    }

    /// <summary>DataGrid row model for the "FEC Data (Staging)" grid. Property names must match BulkRatesStagingFecRowDto.</summary>
    public class FecStagingGridItem : IHasValidationSummary
    {
        [Display(Name = "Status")]
        [GridColumn(Order = 1, Width = 110, Type = GridColumnType.Text, IsFilterable = true)]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Test Code")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = string.Empty;

        [Display(Name = "Unit Price VLA")]
        [GridColumn(Order = 3, Width = 130, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? UnitPriceVla { get; set; }

        [Display(Name = "Defra Unit Price")]
        [GridColumn(Order = 4, Width = 140, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? DefraUnitPrice { get; set; }

        [Display(Name = "FEC New")]
        [GridColumn(Order = 5, Width = 110, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? FecNewRate { get; set; }

        [Display(Name = "Item Description")]
        [GridColumn(Order = 6, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ItemDescription { get; set; }

        [Display(Name = "Short Description")]
        [GridColumn(Order = 7, Width = 160, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ShortDescription { get; set; }

        [Display(Name = "Owner")]
        [GridColumn(Order = 8, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Owner { get; set; }

        [Display(Name = "Comments")]
        [GridColumn(Order = 9, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Comments { get; set; }

        [Display(Name = "Validation")]
        [GridColumn(Order = 10, Width = 260, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ValidationSummary { get; set; }
    }

    /// <summary>DataGrid row model for the "Agrup Details" grid. Property names must match BulkRatesStagingAgrupRowDto.</summary>
    public class AgrupStagingGridItem : IHasValidationSummary
    {
        [Display(Name = "Status")]
        [GridColumn(Order = 1, Width = 110, Type = GridColumnType.Text, IsFilterable = true)]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Test Code")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = string.Empty;

        [Display(Name = "Buyer")]
        [GridColumn(Order = 3, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string Buyer { get; set; } = string.Empty;

        [Display(Name = "Agrup")]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? Agrup { get; set; }

        [Display(Name = "Agrup New")]
        [GridColumn(Order = 5, Width = 110, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? AgrupNew { get; set; }

        [Display(Name = "No Required")]
        [GridColumn(Order = 6, Width = 110, Type = GridColumnType.DoubleNumber, IsFilterable = false)]
        public double? NoRequired { get; set; }

        [Display(Name = "Date Created")]
        [GridColumn(Order = 7, Width = 130, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Active")]
        [GridColumn(Order = 8, Width = 90, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool Active { get; set; }

        [Display(Name = "Comments")]
        [GridColumn(Order = 9, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Comments { get; set; }

        [Display(Name = "Validation")]
        [GridColumn(Order = 10, Width = 260, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ValidationSummary { get; set; }
    }

    /// <summary>DataGrid row model for the "Staff Data (Staging)" grid. Property names must match BulkRatesStagingStaffRowDto.</summary>
    public class StaffStagingGridItem
    {
        [Display(Name = "Status")]
        [GridColumn(Order = 1, Width = 110, Type = GridColumnType.Text, IsFilterable = true)]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "PcGrade")]
        [GridColumn(Order = 2, Width = 110, Type = GridColumnType.Text, IsFilterable = true)]
        public string PcGrade { get; set; } = string.Empty;

        [Display(Name = "Pay Rate")]
        [GridColumn(Order = 3, Width = 110, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? PayRate { get; set; }

        [Display(Name = "Pay Rate New")]
        [GridColumn(Order = 4, Width = 120, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? PayRateNew { get; set; }

        [Display(Name = "NPR")]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? Npr { get; set; }

        [Display(Name = "NPR New")]
        [GridColumn(Order = 6, Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? NprNew { get; set; }

        [Display(Name = "OHR")]
        [GridColumn(Order = 7, Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? Ohr { get; set; }

        [Display(Name = "OHR New")]
        [GridColumn(Order = 8, Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? OhrNew { get; set; }
    }

    /// <summary>DataGrid row model for the "Animal Data (Staging)" grid. Property names must match BulkRatesStagingAnimalRowDto.</summary>
    public class AnimalStagingGridItem
    {
        [Display(Name = "Status")]
        [GridColumn(Order = 1, Width = 110, Type = GridColumnType.Text, IsFilterable = true)]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Animal Type")]
        [GridColumn(Order = 2, Width = 130, Type = GridColumnType.Text, IsFilterable = true)]
        public string AnimalType { get; set; } = string.Empty;

        [Display(Name = "Species")]
        [GridColumn(Order = 3, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Species { get; set; }

        [Display(Name = "Security Level")]
        [GridColumn(Order = 4, Width = 130, Type = GridColumnType.Text, IsFilterable = true)]
        public string? SecurityLevel { get; set; }

        [Display(Name = "Daily Rate")]
        [GridColumn(Order = 5, Width = 110, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? DailyRate { get; set; }

        [Display(Name = "Daily Rate New")]
        [GridColumn(Order = 6, Width = 120, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? DailyRateNew { get; set; }

        [Display(Name = "Defra Daily Rate")]
        [GridColumn(Order = 7, Width = 140, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? DefraDailyRate { get; set; }

        [Display(Name = "Defra Daily Rate New")]
        [GridColumn(Order = 8, Width = 150, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? DefraDailyRateNew { get; set; }

        [Display(Name = "Plan By Week")]
        [GridColumn(Order = 9, Width = 110, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public bool? PlanByWeek { get; set; }
    }
}
