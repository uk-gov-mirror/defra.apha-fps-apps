using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.UnitTests;

public sealed class EmailTemplateRendererTests
{
    private readonly EmailTemplateRenderer _renderer = new(
        Options.Create(new MilestoneNotificationsSettings { SupportContact = "support@example.com" }));

    [Fact]
    public void Subject_ShouldBeFixedConstant()
    {
        Assert.Equal("Milestone and Deliverable Update Request", _renderer.Subject);
    }

    [Fact]
    public void RenderManagerEmailBody_ShouldSubstituteManagerNameAndSupportContact()
    {
        var result = _renderer.RenderManagerEmailBody("Jane Smith", [], includeConfirmationInstruction: false);

        Assert.Contains("Dear Jane Smith,", result.HtmlBody);
        Assert.Contains("support@example.com", result.HtmlBody);
    }

    [Fact]
    public void RenderManagerEmailBody_ShouldHtmlEncodeManagerName()
    {
        var result = _renderer.RenderManagerEmailBody("Jane <script>alert(1)</script>", [], includeConfirmationInstruction: false);

        Assert.DoesNotContain("<script>", result.HtmlBody);
        Assert.Contains("&lt;script&gt;", result.HtmlBody);
    }

    [Fact]
    public void RenderManagerEmailBody_WhenProjectHasValidHttpsEditLink_ShouldIncludeItAsCleanAnchor()
    {
        var projects = new[]
        {
            new NotificationProjectLink(2026, "PROJ-A", "<a href=\"https://pims.example.com/edit/PROJ-A\">PROJ-A</a><br>")
        };

        var result = _renderer.RenderManagerEmailBody("Jane Smith", projects, includeConfirmationInstruction: false);

        Assert.Single(result.IncludedProjects);
        Assert.Empty(result.ExcludedProjects);
        Assert.Contains("<a href=\"https://pims.example.com/edit/PROJ-A\">PROJ-A</a>", result.HtmlBody);
        Assert.DoesNotContain("<br>", result.HtmlBody);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("<a href=\"http://insecure.example.com/edit/PROJ-A\">PROJ-A</a><br>")]
    [InlineData("not a link at all")]
    public void RenderManagerEmailBody_WhenEditLinkInvalid_ShouldExcludeProject_NotThrow(string? editLink)
    {
        var projects = new[] { new NotificationProjectLink(2026, "PROJ-A", editLink) };

        var result = _renderer.RenderManagerEmailBody("Jane Smith", projects, includeConfirmationInstruction: false);

        Assert.Empty(result.IncludedProjects);
        var excluded = Assert.Single(result.ExcludedProjects);
        Assert.Equal("PROJ-A", excluded.ParentProject);
    }

    [Fact]
    public void RenderManagerEmailBody_WhenMixOfValidAndInvalidLinks_ShouldPartitionCorrectly()
    {
        var projects = new[]
        {
            new NotificationProjectLink(2026, "PROJ-A", "<a href=\"https://pims.example.com/edit/PROJ-A\">PROJ-A</a><br>"),
            new NotificationProjectLink(2026, "PROJ-B", null),
        };

        var result = _renderer.RenderManagerEmailBody("Jane Smith", projects, includeConfirmationInstruction: false);

        Assert.Single(result.IncludedProjects);
        Assert.Equal("PROJ-A", result.IncludedProjects[0].ParentProject);
        Assert.Single(result.ExcludedProjects);
        Assert.Equal("PROJ-B", result.ExcludedProjects[0].ParentProject);
    }

    [Fact]
    public void RenderManagerEmailBody_WhenConfirmationInstructionRequested_ShouldIncludeWording()
    {
        var withInstruction = _renderer.RenderManagerEmailBody("Jane Smith", [], includeConfirmationInstruction: true);
        var withoutInstruction = _renderer.RenderManagerEmailBody("Jane Smith", [], includeConfirmationInstruction: false);

        Assert.Contains("confirm", withInstruction.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("confirm your milestone", withoutInstruction.HtmlBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RenderManagerEmailBody_ShouldIncludeDeputyAndSystemGeneratedWording()
    {
        var result = _renderer.RenderManagerEmailBody("Jane Smith", [], includeConfirmationInstruction: false);

        Assert.Contains("deputy", result.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("system-generated", result.HtmlBody, StringComparison.OrdinalIgnoreCase);
    }
}
