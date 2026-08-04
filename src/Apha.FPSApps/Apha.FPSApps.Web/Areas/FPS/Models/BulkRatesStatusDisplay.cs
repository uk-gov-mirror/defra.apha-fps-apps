using System.Text.RegularExpressions;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Single source of truth for how a Bulk Rates request status is shown to the user —
    /// the friendly label and its GOV.UK tag colour modifier. Shared by the queue grid
    /// (Badge column type) and the request Detail page, so both render identically.
    /// </summary>
    public static class BulkRatesStatusDisplay
    {
        /// <summary>
        /// Approved is relabelled "Submitted" — the request has been approved but the worker
        /// hasn't picked it up yet, and "Approved" alone reads as a finished state rather than
        /// one still in flight. The underlying status value stays "Approved" everywhere else
        /// (state-machine checks, audit trail); only this display label changes.
        /// </summary>
        public static string FriendlyLabel(string status) => status switch
        {
            "Approved" => "Submitted",
            _ => Regex.Replace(status, "(?<!^)([A-Z])", " $1")
        };

        public static string GovUkTagModifierClass(string status) => status switch
        {
            "Initiated" => "govuk-tag--blue",
            "ReleasedForApproval" => "govuk-tag--yellow",
            "Approved" => "govuk-tag--turquoise", // shown as "Submitted" — queued, not yet running
            "Running" => "govuk-tag--purple",
            "Completed" => "",
            "Failed" => "govuk-tag--red",
            "Rejected" => "govuk-tag--orange",
            "Cancelled" => "govuk-tag--grey",
            _ => "govuk-tag--grey"
        };
    }
}
