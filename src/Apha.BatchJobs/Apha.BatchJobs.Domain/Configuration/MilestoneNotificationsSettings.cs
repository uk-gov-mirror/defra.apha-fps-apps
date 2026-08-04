namespace Apha.BatchJobs.Domain.Configuration;

/// <summary>
/// Configuration settings for the MilestoneUpdateNotifications scheduled job's email
/// delivery (plan section 10). Extended by later build steps as they need more of the
/// spec section 11 config block (ApplicationBaseUrl, CapsMailbox, MandatoryConfirmationMonths, etc.).
/// </summary>
public class MilestoneNotificationsSettings
{
    /// <summary>
    /// Approved support contact substituted into {{SupportContact}} in the manager
    /// email template (plan section 10.2).
    /// </summary>
    public string? SupportContact { get; set; }

    /// <summary>
    /// When true (the safe default), non-production environments redirect every
    /// manager email to <see cref="NonProdRedirectRecipients"/> instead of the real
    /// recipient list, per spec section 22 ("real users must not receive test emails").
    /// Has no effect when the resolved environment is Production.
    /// </summary>
    public bool NonProdRedirectEnabled { get; set; } = true;

    /// <summary>
    /// Recipients every non-production email is redirected to when
    /// <see cref="NonProdRedirectEnabled"/> is true. Must be non-empty whenever
    /// redirect is active — an empty list fails the send rather than risk delivering
    /// to a real recipient.
    /// </summary>
    public List<string> NonProdRedirectRecipients { get; set; } = [];

    /// <summary>
    /// Base URL for the FPS application — used by MilestoneLinkBuilder as a fallback
    /// path when the view's EditLink value is unavailable (plan section 9.2).
    /// Must begin with "https://" — validated at job start (plan section 22).
    /// Inert when the EditLink path is in use.
    /// </summary>
    public string? ApplicationBaseUrl { get; set; }

    /// <summary>
    /// CAPS team mailbox address — recipient for the run-summary email (plan section 11.4, §13).
    /// Required for a live send; placeholder until stakeholder confirms the address.
    /// </summary>
    public string? CapsMailbox { get; set; }

    /// <summary>
    /// Calendar month numbers (1–12) for which the email body must include the
    /// confirmation instruction paragraph (plan section 6.2, spec section 12).
    /// Empty list = confirmation instruction never included.
    /// </summary>
    public List<int> MandatoryConfirmationMonths { get; set; } = [];

    /// <summary>
    /// When true, allows a <c>monthOverride</c> parameter to be supplied even when
    /// <c>ASPNETCORE_ENVIRONMENT</c> is "Production". Must be an explicit, deliberate
    /// opt-in — not set by default. Guards against accidental production reruns with
    /// a misleading audit month (plan section 6.2).
    /// </summary>
    public bool AllowMonthOverrideInProduction { get; set; } = false;
}
