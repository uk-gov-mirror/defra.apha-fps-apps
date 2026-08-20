using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;

namespace Apha.BatchJobs.UnitTests;

public sealed class EditLinkHrefExtractorTests
{
    [Fact]
    public void TryExtractHref_WhenLegacyAnchorHasHttpsHref_ShouldExtractIt()
    {
        var editLink = "<a href=\"https://pims.example.com/edit/PROJ-A\">PROJ-A</a><br>";

        var result = EditLinkHrefExtractor.TryExtractHref(editLink, out var href);

        Assert.True(result);
        Assert.Equal("https://pims.example.com/edit/PROJ-A", href);
    }

    [Fact]
    public void TryExtractHref_WhenHrefIsHttp_ShouldReject()
    {
        var editLink = "<a href=\"http://pims.example.com/edit/PROJ-A\">PROJ-A</a><br>";

        var result = EditLinkHrefExtractor.TryExtractHref(editLink, out var href);

        Assert.False(result);
        Assert.Equal(string.Empty, href);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryExtractHref_WhenEditLinkMissing_ShouldReject(string? editLink)
    {
        var result = EditLinkHrefExtractor.TryExtractHref(editLink, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryExtractHref_WhenNoHrefAttributePresent_ShouldReject()
    {
        var result = EditLinkHrefExtractor.TryExtractHref("<a>PROJ-A</a><br>", out _);

        Assert.False(result);
    }

    [Fact]
    public void TryExtractHref_WhenHrefIsNotAbsoluteUri_ShouldReject()
    {
        var result = EditLinkHrefExtractor.TryExtractHref("<a href=\"/relative/path\">PROJ-A</a><br>", out _);

        Assert.False(result);
    }

    [Fact]
    public void TryExtractHref_WhenHrefIsHtmlEncoded_ShouldDecodeBeforeValidating()
    {
        var editLink = "<a href=\"https://pims.example.com/edit?a=1&amp;b=2\">PROJ-A</a><br>";

        var result = EditLinkHrefExtractor.TryExtractHref(editLink, out var href);

        Assert.True(result);
        Assert.Equal("https://pims.example.com/edit?a=1&b=2", href);
    }

    [Fact]
    public void TryExtractHref_WhenGarbageInput_ShouldRejectWithoutThrowing()
    {
        var result = EditLinkHrefExtractor.TryExtractHref("not html at all", out var href);

        Assert.False(result);
        Assert.Equal(string.Empty, href);
    }
}
