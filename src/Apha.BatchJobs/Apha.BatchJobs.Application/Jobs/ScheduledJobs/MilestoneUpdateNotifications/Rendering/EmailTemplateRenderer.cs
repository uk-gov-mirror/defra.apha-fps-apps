using System.Net;
using System.Text;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Rendering;

/// <summary>
/// Implementation of <see cref="IEmailTemplateRenderer"/>, following spec section 13's
/// suggested template. EditLink is never spliced into the template as raw HTML — each
/// project's href is extracted and validated first (plan section 9.2), and projects that
/// fail that check are reported back via <see cref="EmailTemplateRenderResult.ExcludedProjects"/>
/// rather than silently dropped.
/// </summary>
public sealed class EmailTemplateRenderer : IEmailTemplateRenderer
{
    /// <inheritdoc />
    public string Subject => "Milestone and Deliverable Update Request";

    private readonly MilestoneNotificationsSettings _settings;

    public EmailTemplateRenderer(IOptions<MilestoneNotificationsSettings> settings)
    {
        _settings = settings?.Value ?? new MilestoneNotificationsSettings();
    }

    /// <inheritdoc />
    public EmailTemplateRenderResult RenderManagerEmailBody(
        string managerName,
        IReadOnlyList<NotificationProjectLink> projects,
        bool includeConfirmationInstruction)
    {
        ArgumentNullException.ThrowIfNull(projects);

        var included = new List<NotificationProjectLink>();
        var excluded = new List<NotificationProjectLink>();
        var linksHtml = new StringBuilder();
        linksHtml.Append("<ul>");

        foreach (var project in projects)
        {
            if (EditLinkHrefExtractor.TryExtractHref(project.EditLink, out var href))
            {
                included.Add(project);
                linksHtml
                    .Append("<li><a href=\"")
                    .Append(WebUtility.HtmlEncode(href))
                    .Append("\">")
                    .Append(WebUtility.HtmlEncode(project.ParentProject))
                    .Append("</a></li>");
            }
            else
            {
                excluded.Add(project);
            }
        }

        linksHtml.Append("</ul>");

        var confirmationInstruction = includeConfirmationInstruction
            ? "<p>Please confirm your milestone or deliverable data due this month, even where no update is made.</p>"
            : string.Empty;

        var body = $"""
            <p>Dear {WebUtility.HtmlEncode(managerName)},</p>

            <p>
            Here are the links to edit the milestones for your projects.
            </p>

            {linksHtml}

            {confirmationInstruction}

            <p>
            If you are not the person named in this email, you are receiving it
            as a deputy for information only. You will not be able to edit these
            milestones or deliverables.
            </p>

            <p>
            This is a system-generated email. Please do not reply.
            For assistance, contact {WebUtility.HtmlEncode(_settings.SupportContact)}.
            </p>
            """;

        return new EmailTemplateRenderResult(body, included, excluded);
    }
}
