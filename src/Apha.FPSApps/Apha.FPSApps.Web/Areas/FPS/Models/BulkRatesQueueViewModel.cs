using Apha.FPSApps.Application.Dtos.FPS.BulkRates;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class BulkRatesQueueViewModel
    {
        public List<BulkRatesQueueEntryDto> Entries { get; set; } = [];
        public string? JobNameFilter { get; set; }
        public int? FpsYearFilter { get; set; }
        public string? StatusFilter { get; set; }
        public string CurrentUserEmail { get; set; } = string.Empty;

        /// <summary>The active (blocking-status) request for JobNameFilter, if any. Only populated when JobNameFilter is set.</summary>
        public BulkRatesQueueEntryDto? ActiveRequest { get; set; }

        /// <summary>True when this page was reached via a rate-type-locked entry point — hides the "Job type" picker.</summary>
        public bool IsJobNameLocked { get; set; }

        /// <summary>
        /// Most recent request for JobNameFilter owned by the current user that is open for
        /// (re-)upload via the "Upload Updated Data" tracker button. Null when none exists —
        /// the caller must direct the user to create a request first.
        /// </summary>
        public Guid? UploadTargetId =>
            Entries
                .Where(e => e.JobName == JobNameFilter
                            && e.Status is "Initiated" or "Rejected"
                            && string.Equals(e.RequestedBy, CurrentUserEmail, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.RequestedAtUtc)
                .Select(e => (Guid?)e.JobExecutionId)
                .FirstOrDefault();
    }
}
