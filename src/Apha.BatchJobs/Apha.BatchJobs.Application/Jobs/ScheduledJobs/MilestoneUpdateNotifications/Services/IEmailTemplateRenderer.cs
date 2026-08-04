using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;

/// <summary>
/// Renders the manager notification email from spec section 13's suggested template.
/// Pure — takes already-grouped/classified data, does no I/O.
/// </summary>
public interface IEmailTemplateRenderer
{
    /// <summary>
    /// Fixed subject constant — not configurable, for legacy logic parity.
    /// </summary>
    string Subject { get; }

    /// <summary>
    /// Renders one manager's email body. Projects whose EditLink cannot be resolved to
    /// a safe HTTPS href are dropped from <see cref="EmailTemplateRenderResult.IncludedProjects"/>
    /// and surfaced instead in <see cref="EmailTemplateRenderResult.ExcludedProjects"/>
    /// (plan section 9.2, section 10.3).
    /// </summary>
    /// <param name="managerName">Recipient display name, substituted into {{ManagerName}}.</param>
    /// <param name="projects">Deduplicated, code-ordered project links for this recipient group.</param>
    /// <param name="includeConfirmationInstruction">
    /// Whether the calendar month is one of the configured mandatory-confirmation months
    /// (spec section 12) — the caller decides this, the renderer stays month-agnostic.
    /// </param>
    EmailTemplateRenderResult RenderManagerEmailBody(
        string managerName,
        IReadOnlyList<NotificationProjectLink> projects,
        bool includeConfirmationInstruction);
}
