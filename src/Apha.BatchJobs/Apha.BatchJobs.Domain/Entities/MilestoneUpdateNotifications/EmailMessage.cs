namespace Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

/// <summary>
/// Provider-agnostic outbound email (plan section 10.1) — mapped onto
/// Apha.Common's <c>EmailMessageModel</c> by <c>GraphBackedEmailService</c>.
/// </summary>
public sealed record EmailMessage(
    IReadOnlyList<string> To,
    string Subject,
    string HtmlBody);
