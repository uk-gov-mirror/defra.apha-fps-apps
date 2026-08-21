namespace Apha.BatchJobs.Infrastructure.Email;

/// <summary>
/// Email delivery settings — environment routing and external address configuration.
/// Bound from the "MilestoneNotifications" section so appsettings structure is unchanged.
/// </summary>
public sealed class EmailDeliverySettings
{
    /// <summary>
    /// When true, non-production environments redirect every email to
    /// <see cref="NonProdRedirectRecipients"/> instead of the real recipient list,
    /// per spec section 22 ("real users must not receive test emails").
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
}
