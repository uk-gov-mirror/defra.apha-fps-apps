using System.ComponentModel.DataAnnotations;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;
using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class BulkRatesQueueViewModel
    {
        /// <summary>Grid config built explicitly in BulkRatesController — never left as new().</summary>
        public DataGridConfig<BulkRatesQueueGridItem> Grid { get; set; } = new();
        public string? JobNameFilter { get; set; }
        public int? FpsYearFilter { get; set; }
        public string? StatusFilter { get; set; }
        public string CurrentUserEmail { get; set; } = string.Empty;

        /// <summary>The active (blocking-status) request for JobNameFilter, if any. Only populated when JobNameFilter is set.</summary>
        public BulkRatesQueueEntryDto? ActiveRequest { get; set; }

        /// <summary>True when this page was reached via a rate-type-locked entry point — hides the "Job type" picker.</summary>
        public bool IsJobNameLocked { get; set; }

        /// <summary>
        /// True when the app-wide selected FPS year's status matches what JobNameFilter requires
        /// (Open for FEC Test Rates, Planned for Staff/Animal Rates) — false blocks "New Request".
        /// Always true when JobNameFilter is empty/unrecognised (no single job type to gate against).
        /// </summary>
        public bool CanCreateForYear { get; set; } = true;

        /// <summary>The yearstatus JobNameFilter requires (e.g. "Open"), for the blocked-state message.</summary>
        public string? RequiredYearStatus { get; set; }

        /// <summary>The app-wide selected FPS year's actual current status, for the blocked-state message.</summary>
        public string? CurrentYearStatus { get; set; }

        /// <summary>
        /// The current user's own open (Initiated/Rejected) request for JobNameFilter, if it is
        /// also the active blocking request — i.e. available for (re-)upload via the "Upload
        /// Updated Data" tracker button. Derived from ActiveRequest rather than the (now paged)
        /// grid data, since at most one request can be in a blocking status per job type at a time.
        /// </summary>
        public Guid? UploadTargetId =>
            ActiveRequest != null
            && ActiveRequest.Status is "Initiated" or "Rejected"
            && string.Equals(ActiveRequest.RequestedBy, CurrentUserEmail, StringComparison.OrdinalIgnoreCase)
                ? ActiveRequest.JobExecutionId
                : null;
    }

    /// <summary>
    /// DataGrid row model for the Bulk Rates request queue.
    /// Property names must match BulkRatesQueueEntryDto for FpsViewModelMapper's AutoMapper profile.
    /// </summary>
    public class BulkRatesQueueGridItem
    {
        [Display(Name = "Request ID")]
        [GridColumn(Order = 1, Width = 320, Type = GridColumnType.Text, IsFilterable = false)]
        public Guid JobExecutionId { get; set; }

        [Display(Name = "Job Type")]
        [GridColumn(Order = 2, Width = 160, Type = GridColumnType.Text, IsFilterable = false)]
        public string JobName { get; set; } = string.Empty;

        [Display(Name = "Status")]
        [GridColumn(Order = 3, Width = 160, Type = GridColumnType.Badge, IsFilterable = false, CssClassSourceProperty = nameof(StatusBadgeModifier))]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// GOV.UK tag colour modifier for Status (e.g. "govuk-tag--yellow") — a data carrier for
        /// the Badge column type only, never rendered as its own column. See BulkRatesStatusDisplay.
        /// GetColumnsDefination includes every property by default (no [GridColumn] = still a
        /// visible column using the raw property name) — IsVisible = false is required here, not
        /// optional, or this becomes its own "StatusBadgeModifier" column in the grid.
        /// </summary>
        [GridColumn(IsVisible = false)]
        public string StatusBadgeModifier { get; set; } = string.Empty;

        [Display(Name = "Requested By")]
        [GridColumn(Order = 4, Width = 220, Type = GridColumnType.Text, IsFilterable = false)]
        public string RequestedBy { get; set; } = string.Empty;

        [Display(Name = "Requested At (UTC)")]
        [GridColumn(Order = 5, Width = 170, Type = GridColumnType.DateTime, IsFilterable = false)]
        public DateTime RequestedAtUtc { get; set; }
    }
}
