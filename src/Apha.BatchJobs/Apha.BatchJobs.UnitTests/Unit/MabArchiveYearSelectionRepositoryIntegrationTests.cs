using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed smoke test for MabArchiveYearSelectionService's SQL wiring.
/// GetProcessableYearsAsync validates the *entire* fps.tblyearmaster table (spec
/// requires exactly one Open year across the whole table), so this test cannot safely
/// seed its own Open/Planned rows without risking a false "multiple Open years" failure
/// against whatever real data already exists. Instead it asserts against the live
/// table's current state: it records the real Open year up front, seeds one extra
/// out-of-range Closed sandbox row, and asserts the sandbox row is excluded from the
/// result while the real Open year is still returned correctly.
/// </summary>
[Trait("Category", "Integration")]
public sealed class MabArchiveYearSelectionRepositoryIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private const int SandboxClosedYear = 9701;

    private readonly string _connectionString;
    private string? _skipReason;
    private int _liveOpenYearCount;
    private int _livePlannedYearCount;
    private int _liveOpenYear;

    public MabArchiveYearSelectionRepositoryIntegrationTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var context = CreateDbContext();
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                _skipReason = "Integration DB unavailable.";
                return;
            }

            _liveOpenYearCount = await context.Database
                .SqlQuery<int>($@"SELECT COUNT(*)::int AS ""Value"" FROM fps.tblyearmaster WHERE yearstatus ILIKE 'Open'")
                .SingleAsync();
            _livePlannedYearCount = await context.Database
                .SqlQuery<int>($@"SELECT COUNT(*)::int AS ""Value"" FROM fps.tblyearmaster WHERE yearstatus ILIKE 'Planned'")
                .SingleAsync();

            if (_liveOpenYearCount == 1)
            {
                _liveOpenYear = await context.Database
                    .SqlQuery<int>($@"SELECT fpsyear AS ""Value"" FROM fps.tblyearmaster WHERE yearstatus ILIKE 'Open'")
                    .SingleAsync();
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task GetProcessableYearsAsync_ExcludesSeededClosedSandboxYear_AndReturnsLiveOpenYear()
    {
        Skip.IfNot(string.IsNullOrWhiteSpace(_skipReason), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _liveOpenYearCount == 1 && _livePlannedYearCount <= 1,
            $"Live fps.tblyearmaster does not currently satisfy exactly-one-Open/zero-or-one-Planned (OpenCount={_liveOpenYearCount}, PlannedCount={_livePlannedYearCount}); cannot safely assert against live state.");

        await DeleteSandboxYearAsync();
        await SeedSandboxYearAsync(SandboxClosedYear, "Closed");

        try
        {
            var service = new MabArchiveYearSelectionRepository(
                CreateDbContextFactory(),
                NullLogger<MabArchiveYearSelectionRepository>.Instance);

            var result = await service.GetProcessableYearsAsync(CancellationToken.None);

            Assert.Equal(_liveOpenYear, result.OpenYear);
            Assert.NotEqual(SandboxClosedYear, result.OpenYear);
            Assert.NotEqual(SandboxClosedYear, result.PlannedYear);
        }
        finally
        {
            await DeleteSandboxYearAsync();
        }
    }

    private async Task SeedSandboxYearAsync(int fpsYear, string yearStatus)
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO fps.tblyearmaster (fpsyear, fpsyearcode, yearstatus, remarks, active, createdby)
            VALUES ({fpsYear}, {$"IT{fpsYear}"}, {yearStatus}, 'MabArchiveYearSelectionServiceIntegrationTests', true, 'IntegrationTest');");
    }

    private async Task DeleteSandboxYearAsync()
    {
        await using var context = CreateDbContext();
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            DELETE FROM fps.tblyearmaster WHERE fpsyear = {SandboxClosedYear};");
    }

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private IDbContextFactory<BatchJobsDbContext> CreateDbContextFactory()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new TestDbContextFactory(options);
    }

    private sealed class TestDbContextFactory : IDbContextFactory<BatchJobsDbContext>
    {
        private readonly DbContextOptions<BatchJobsDbContext> _options;

        public TestDbContextFactory(DbContextOptions<BatchJobsDbContext> options)
        {
            _options = options;
        }

        public BatchJobsDbContext CreateDbContext() => new(_options);
    }
}
