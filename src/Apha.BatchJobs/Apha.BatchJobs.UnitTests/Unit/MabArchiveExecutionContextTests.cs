using Apha.BatchJobs.Domain.Entities.MabArchive;

namespace Apha.BatchJobs.UnitTests;

public sealed class MabArchiveExecutionContextTests
{
    [Fact]
    public void Constructor_WithOpenYearOnly_ShouldExposeNullPlannedYear()
    {
        var context = new MabArchiveExecutionContext(2026, null);

        Assert.Equal(2026, context.OpenYear);
        Assert.Null(context.PlannedYear);
    }

    [Fact]
    public void Constructor_WithOpenAndPlannedYear_ShouldExposeBoth()
    {
        var context = new MabArchiveExecutionContext(2026, 2027);

        Assert.Equal(2026, context.OpenYear);
        Assert.Equal(2027, context.PlannedYear);
    }

    [Fact]
    public void Equality_WhenSameValues_ShouldBeEqual()
    {
        var first = new MabArchiveExecutionContext(2026, 2027);
        var second = new MabArchiveExecutionContext(2026, 2027);

        Assert.Equal(first, second);
    }
}
