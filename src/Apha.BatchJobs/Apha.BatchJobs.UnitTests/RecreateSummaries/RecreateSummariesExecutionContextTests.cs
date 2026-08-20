using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class RecreateSummariesExecutionContextTests
{
    [Fact]
    public void Constructor_WhenDbContextIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var connection = new NpgsqlConnection();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => new RecreateSummariesExecutionContext(dbContext: null!, connection, fpsYear: 2026));

        // Assert
        Assert.Equal("dbContext", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenConnectionIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var dbContext = CreateDbContext();

        // Act
        var exception = Assert.Throws<ArgumentNullException>(
            () => new RecreateSummariesExecutionContext(dbContext, connection: null!, fpsYear: 2026));

        // Assert
        Assert.Equal("connection", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenArgumentsAreValid_ShouldExposeProperties()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        using var connection = new NpgsqlConnection();
        // Act
        var context = new RecreateSummariesExecutionContext(dbContext, connection, fpsYear: 2026);

        // Assert
        Assert.Same(dbContext, context.DbContext);
        Assert.Same(connection, context.Connection);
        Assert.Equal(2026, context.FpsYear);
    }

    private static BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new BatchJobsDbContext(options);
    }
}
