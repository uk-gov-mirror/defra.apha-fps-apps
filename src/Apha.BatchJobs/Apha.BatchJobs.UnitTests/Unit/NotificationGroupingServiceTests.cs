using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Apha.BatchJobs.Infrastructure.Repositories.MilestoneUpdateNotifications;

namespace Apha.BatchJobs.UnitTests;

public sealed class NotificationGroupingServiceTests
{
    private readonly NotificationGroupingService _service = new(new RecipientIdentityBuilder());

    [Fact]
    public void GroupCandidates_WhenNullCandidates_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.GroupCandidates(null!));
    }

    [Fact]
    public void GroupCandidates_WhenOneManagerHasMultipleProjects_ShouldProduceOneGroup()
    {
        var candidates = new[]
        {
            new MilestoneNotificationCandidate(2026, "PROJ-A", "Jane Smith", "M123", "jane@example.com", false, "<a href=\"/a\">A</a>"),
            new MilestoneNotificationCandidate(2026, "PROJ-B", "Jane Smith", "M123", "jane@example.com", false, "<a href=\"/b\">B</a>"),
            new MilestoneNotificationCandidate(2026, "PROJ-C", "Jane Smith", "M123", "jane@example.com", false, "<a href=\"/c\">C</a>"),
        };

        var groups = _service.GroupCandidates(candidates);

        var group = Assert.Single(groups);
        Assert.Equal(3, group.Projects.Count);
    }

    [Fact]
    public void GroupCandidates_WhenDuplicateProjectCodeForSameRecipient_ShouldCollapseToOneLink()
    {
        var candidates = new[]
        {
            new MilestoneNotificationCandidate(2026, "PROJ-A", "Jane Smith", "M123", "jane@example.com", false, "<a href=\"/a\">A</a>"),
            new MilestoneNotificationCandidate(2026, "PROJ-A", "Jane Smith", "M123", "jane@example.com", false, "<a href=\"/a\">A</a>"),
        };

        var groups = _service.GroupCandidates(candidates);

        var group = Assert.Single(groups);
        var project = Assert.Single(group.Projects);
        Assert.Equal("PROJ-A", project.ParentProject);
    }

    [Fact]
    public void GroupCandidates_ShouldOrderProjectLinksByProjectCodeAscending()
    {
        var candidates = new[]
        {
            new MilestoneNotificationCandidate(2026, "PROJ-C", "Jane Smith", "M123", "jane@example.com", false, null),
            new MilestoneNotificationCandidate(2026, "PROJ-A", "Jane Smith", "M123", "jane@example.com", false, null),
            new MilestoneNotificationCandidate(2026, "PROJ-B", "Jane Smith", "M123", "jane@example.com", false, null),
        };

        var groups = _service.GroupCandidates(candidates);

        var group = Assert.Single(groups);
        Assert.Equal(["PROJ-A", "PROJ-B", "PROJ-C"], group.Projects.Select(p => p.ParentProject));
    }

    [Fact]
    public void GroupCandidates_WhenTwoDistinctRecipients_ShouldProduceTwoGroups()
    {
        var candidates = new[]
        {
            new MilestoneNotificationCandidate(2026, "PROJ-A", "Jane Smith", "M123", "jane@example.com", false, null),
            new MilestoneNotificationCandidate(2026, "PROJ-B", "John Doe", "M456", "john@example.com", false, null),
        };

        var groups = _service.GroupCandidates(candidates);

        Assert.Equal(2, groups.Count);
    }

    [Fact]
    public void GroupCandidates_WhenRecipientIsDisabled_ShouldStillProduceGroup()
    {
        var candidates = new[]
        {
            new MilestoneNotificationCandidate(2026, "PROJ-A", "Jane Smith", "M123", "jane@example.com", true, null),
        };

        var groups = _service.GroupCandidates(candidates);

        var group = Assert.Single(groups);
        Assert.True(group.IsDisabled);
    }

    [Fact]
    public void GroupCandidates_WhenEmailMissing_ShouldStillProduceGroup()
    {
        var candidates = new[]
        {
            new MilestoneNotificationCandidate(2026, "PROJ-A", "Jane Smith", "M123", null, false, null),
        };

        var groups = _service.GroupCandidates(candidates);

        var group = Assert.Single(groups);
        Assert.Null(group.Email);
    }

    [Fact]
    public void GroupCandidates_ShouldPopulateDurablePersonIdFromMNumber()
    {
        var candidates = new[]
        {
            new MilestoneNotificationCandidate(2026, "PROJ-A", "Jane Smith", "  m123 ", "jane@example.com", false, null),
        };

        var groups = _service.GroupCandidates(candidates);

        var group = Assert.Single(groups);
        Assert.Equal("M123", group.DurablePersonId);
    }
}
