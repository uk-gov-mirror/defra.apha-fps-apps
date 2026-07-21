namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;

/// <summary>
/// Computes the two distinct identity concepts described in plan section 9.1:
/// a grouping/duplicate-prevention key that is always the MNumber+Name+Email
/// composite (never MNumber alone), and a separate durable person identifier
/// used only for reporting/consolidation, never for grouping.
/// </summary>
public interface IRecipientIdentityBuilder
{
    /// <summary>
    /// Deterministic SHA-256 hash of normalized MNumber + ProjectManager + Email
    /// (fixed sentinels substituted for any missing component). This is what
    /// fps.notification_delivery.recipientid stores.
    /// </summary>
    string BuildRecipientId(string? mNumber, string? projectManager, string? email);

    /// <summary>
    /// Normalized MNumber alone, or null when absent. Never used for grouping.
    /// </summary>
    string? BuildDurablePersonId(string? mNumber);
}
