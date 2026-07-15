using Apha.FPSApps.Application.Dtos.FPS.BulkRates;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class BulkRatesDetailViewModel
    {
        public BulkRatesRequestDetailDto Request { get; set; } = new();
        public string CurrentUserEmail { get; set; } = string.Empty;

        /// <summary>Populated from GetValidationResultsAsync on page load; null if no file uploaded yet.</summary>
        public BulkRatesUploadResultDto? UploadResult { get; set; }

        // ── Derived permission flags used by the view ──────────────────────

        public bool IsInitiator =>
            string.Equals(Request.Entry.RequestedBy, CurrentUserEmail, StringComparison.OrdinalIgnoreCase);

        public bool CanUpload =>
            IsInitiator && Request.Entry.Status is "Initiated" or "Rejected";

        public bool CanRelease =>
            IsInitiator
            && Request.Entry.Status == "Initiated"
            && UploadResult != null
            && !UploadResult.ValidationErrors.Any(e => e.Severity == "Error");

        public bool CanApprove =>
            !IsInitiator && Request.Entry.Status == "PendingApproval";

        public bool CanReject =>
            !IsInitiator && Request.Entry.Status == "PendingApproval";

        public bool CanCancel =>
            IsInitiator && Request.Entry.Status is "Initiated" or "Rejected";
    }
}
