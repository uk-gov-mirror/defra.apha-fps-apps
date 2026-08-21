using System.Net;
using System.Text.RegularExpressions;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Rendering;

/// <summary>
/// Extracts and validates the <c>href</c> from the legacy-generated EditLink HTML fragment
/// (a full <c>&lt;a href="..."&gt;...&lt;/a&gt;&lt;br&gt;</c> anchor, not a bare URL —
/// plan section 9.2). Reusing EditLink safely means pulling out just the href and
/// building fresh anchor markup around it — never splicing the legacy fragment's HTML
/// directly into a new template. HTTPS-only, per spec section 22's unconditional
/// "all links must use HTTPS" requirement — anything else is treated as invalid rather
/// than silently downgraded.
/// </summary>
internal static class EditLinkHrefExtractor
{
    private static readonly Regex HrefPattern = new(
        "href\\s*=\\s*\"([^\"]*)\"",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool TryExtractHref(string? editLinkHtml, out string href)
    {
        href = string.Empty;

        if (string.IsNullOrWhiteSpace(editLinkHtml))
            return false;

        var match = HrefPattern.Match(editLinkHtml);
        if (!match.Success)
            return false;

        var candidate = WebUtility.HtmlDecode(match.Groups[1].Value).Trim();

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttps)
            return false;

        href = uri.ToString();
        return true;
    }
}
