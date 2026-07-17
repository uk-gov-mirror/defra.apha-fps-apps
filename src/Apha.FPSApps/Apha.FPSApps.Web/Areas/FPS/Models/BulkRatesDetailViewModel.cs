using Apha.FPSApps.Application.Dtos.FPS.BulkRates;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class BulkRatesDetailViewModel
    {
        public BulkRatesRequestDetailDto Request { get; set; } = new();
        public string CurrentUserEmail { get; set; } = string.Empty;

        /// <summary>Populated from GetValidationResultsAsync on page load; null if no file uploaded yet.</summary>
        public BulkRatesUploadResultDto? UploadResult { get; set; }

        /// <summary>Populated from GetStagingDataAsync on page load; null on load failure. Empty (not null) for non-FEC requests.</summary>
        public BulkRatesStagingDataDto? StagingData { get; set; }

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

        public bool CanReject =>
            !IsInitiator && Request.Entry.Status == "ReleasedForApproval";

        public bool CanCancel =>
            IsInitiator && Request.Entry.Status is "Initiated" or "Rejected" or "Failed";
    }
}
