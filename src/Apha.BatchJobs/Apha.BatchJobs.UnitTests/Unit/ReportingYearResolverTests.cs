using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.MilestoneUpdateNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apha.BatchJobs.UnitTests;

public sealed class ReportingYearResolverTests
{
    [Fact]
    public void Constructor_WhenContextIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new ReportingYearResolver(null!, NullLogger<ReportingYearResolver>.Instance));

        Assert.Equal("context", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        using var context = CreateInMemoryDbContext();

        var ex = Assert.Throws<ArgumentNullException>(() =>
            new ReportingYearResolver(context, null!));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public async Task ResolveAsync_WhenVLatestMonthYearHasNoRows_ShouldThrowInvalidOperationException()
    {
        await using var context = CreateInMemoryDbContext();
        var resolver = new ReportingYearResolver(context, NullLogger<ReportingYearResolver>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(CancellationToken.None));
    }

    private static BatchJobsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new BatchJobsDbContext(options);
    }
}
