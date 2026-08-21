using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.MilestoneUpdateNotifications.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apha.BatchJobs.UnitTests;

public sealed class MilestoneNotificationReadRepositoryTests
{
    [Fact]
    public void Constructor_WhenContextIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new MilestoneNotificationReadRepository(null!, NullLogger<MilestoneNotificationReadRepository>.Instance));

        Assert.Equal("context", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        using var context = CreateInMemoryDbContext();

        var ex = Assert.Throws<ArgumentNullException>(() =>
            new MilestoneNotificationReadRepository(context, null!));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public async Task GetNotificationCandidatesAsync_WhenViewHasNoRows_ShouldReturnEmptyList()
    {
        await using var context = CreateInMemoryDbContext();
        var repository = new MilestoneNotificationReadRepository(context, NullLogger<MilestoneNotificationReadRepository>.Instance);

        var candidates = await repository.GetNotificationCandidatesAsync(CancellationToken.None);

        Assert.Empty(candidates);
    }

    private static BatchJobsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new BatchJobsDbContext(options);
    }
}
