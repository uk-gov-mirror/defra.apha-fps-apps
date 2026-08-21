using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.MabArchive.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.UnitTests;

public sealed class FpsTotalsRepositoryTests
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=LOCAL_DB_PASSWORD;Timeout=30";

    [Fact]
    public void Constructor_WhenSettingsIsNull_ShouldUseDefaults()
    {
        using var context = CreateDbContext(GetConnectionString());

        var service = new FpsTotalsRepository(
            context,
            NullLogger<FpsTotalsRepository>.Instance,
            settings: null!);

        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WhenContextIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new FpsTotalsRepository(
                null!,
                NullLogger<FpsTotalsRepository>.Instance,
                Options.Create(new MabArchiveSettings())));

        Assert.Equal("context", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        using var context = CreateInMemoryDbContext();
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new FpsTotalsRepository(
                context,
                null!,
                Options.Create(new MabArchiveSettings())));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public async Task RebuildSourceTotalsAsync_WhenNoSourceRowsAndStrictIsolationDisabled_ShouldReturnZero()
    {
        await using var context = CreateDbContext(GetConnectionString());
        await AssertCanConnectAsync(context);

        var service = new FpsTotalsRepository(
            context,
            NullLogger<FpsTotalsRepository>.Instance,
            Options.Create(new MabArchiveSettings { StrictYearIsolation = false }));

        var rows = await service.RebuildSourceTotalsAsync(1900, CancellationToken.None);

        Assert.Equal(0, rows);
    }

    [Fact]
    public async Task RebuildSourceTotalsAsync_WhenNoSourceRowsAndStrictIsolationEnabled_ShouldReturnZero()
    {
        await using var context = CreateDbContext(GetConnectionString());
        await AssertCanConnectAsync(context);

        var service = new FpsTotalsRepository(
            context,
            NullLogger<FpsTotalsRepository>.Instance,
            Options.Create(new MabArchiveSettings { StrictYearIsolation = true }));

        var rows = await service.RebuildSourceTotalsAsync(1900, CancellationToken.None);

        Assert.Equal(0, rows);
    }

    [Fact]
    public async Task RebuildSourceTotalsAsync_WhenExecutedWithinTransaction_ShouldComplete_AndRollback()
    {
        await using var context = CreateDbContext(GetConnectionString());
        await AssertCanConnectAsync(context);

        var targetYear = DateTime.UtcNow.Year;

        var service = new FpsTotalsRepository(
            context,
            NullLogger<FpsTotalsRepository>.Instance,
            Options.Create(new MabArchiveSettings { StrictYearIsolation = false }));

        await using var transaction = await context.Database.BeginTransactionAsync();
        var rows = await service.RebuildSourceTotalsAsync(targetYear, CancellationToken.None);
        await transaction.RollbackAsync();

        Assert.True(rows >= 0, $"Expected non-negative row count for year {targetYear}, but got {rows}.");
    }

    [Fact]
    public async Task RebuildSourceTotalsAsync_WhenProviderDoesNotSupportExecuteDelete_ShouldRethrow()
    {
        await using var context = CreateInMemoryDbContext();

        var service = new FpsTotalsRepository(
            context,
            NullLogger<FpsTotalsRepository>.Instance,
            Options.Create(new MabArchiveSettings { StrictYearIsolation = false }));

        await Assert.ThrowsAnyAsync<Exception>(() => service.RebuildSourceTotalsAsync(2026, CancellationToken.None));
    }

    private static string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    private static BatchJobsDbContext CreateDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private static BatchJobsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new BatchJobsDbContext(options);
    }

    private static async Task AssertCanConnectAsync(BatchJobsDbContext context)
    {
        var canConnect = await context.Database.CanConnectAsync();
        Assert.True(canConnect, "Integration DB unavailable for ReloadFpsTotalsServiceTests.");
    }
}
