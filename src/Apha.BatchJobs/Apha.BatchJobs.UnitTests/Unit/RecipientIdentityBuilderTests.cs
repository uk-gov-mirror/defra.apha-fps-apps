using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;

namespace Apha.BatchJobs.UnitTests;

public sealed class RecipientIdentityBuilderTests
{
    private readonly RecipientIdentityBuilder _builder = new();

    [Fact]
    public void BuildRecipientId_WhenNameAndEmailDifferOnlyByCasingAndWhitespace_ShouldProduceSameId()
    {
        var id1 = _builder.BuildRecipientId("M123", "Jane Smith", "jane.smith@example.com");
        var id2 = _builder.BuildRecipientId("m123", "  jane smith  ", "JANE.SMITH@EXAMPLE.COM");

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void BuildRecipientId_WhenSameMNumberButDifferentEmail_ShouldProduceDifferentIds()
    {
        var id1 = _builder.BuildRecipientId("M123", "Jane Smith", "jane.smith@example.com");
        var id2 = _builder.BuildRecipientId("M123", "Jane Smith", "jane.smith2@example.com");

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public void BuildRecipientId_WhenSameMNumberButDifferentName_ShouldProduceDifferentIds()
    {
        var id1 = _builder.BuildRecipientId("M123", "Jane Smith", "jane.smith@example.com");
        var id2 = _builder.BuildRecipientId("M123", "Jane Other", "jane.smith@example.com");

        Assert.NotEqual(id1, id2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildRecipientId_WhenMNumberMissing_ShouldStillProduceDeterministicId(string? mNumber)
    {
        var id1 = _builder.BuildRecipientId(mNumber, "Jane Smith", "jane.smith@example.com");
        var id2 = _builder.BuildRecipientId(null, "jane smith", "JANE.SMITH@example.com");

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void BuildRecipientId_WhenEmailMissing_ShouldNotCollideWithSameRecipientHavingEmail()
    {
        var withEmail = _builder.BuildRecipientId("M123", "Jane Smith", "jane.smith@example.com");
        var withoutEmail = _builder.BuildRecipientId("M123", "Jane Smith", null);

        Assert.NotEqual(withEmail, withoutEmail);
    }

    [Fact]
    public void BuildRecipientId_ShouldReturn64CharacterHexString()
    {
        var id = _builder.BuildRecipientId("M123", "Jane Smith", "jane.smith@example.com");

        Assert.Equal(64, id.Length);
        Assert.True(id.All(Uri.IsHexDigit));
    }

    [Theory]
    [InlineData("M123", "M123")]
    [InlineData("  m123  ", "M123")]
    public void BuildDurablePersonId_WhenMNumberPresent_ShouldReturnNormalizedValue(string input, string expected)
    {
        Assert.Equal(expected, _builder.BuildDurablePersonId(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildDurablePersonId_WhenMNumberMissing_ShouldReturnNull(string? mNumber)
    {
        Assert.Null(_builder.BuildDurablePersonId(mNumber));
    }
}
