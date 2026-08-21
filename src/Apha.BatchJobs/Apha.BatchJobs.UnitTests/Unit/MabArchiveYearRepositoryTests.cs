using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Ports;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.MabArchive.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apha.BatchJobs.UnitTests;

public sealed class MabArchiveYearRepositoryTests
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=LOCAL_DB_PASSWORD;Timeout=30";

    [Fact]
    public void Constructor_WhenContextIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => new MabArchiveYearRepository(null!, NullLogger<MabArchiveYearRepository>.Instance, CreateSequentialLoaders(1, 24)));

        Assert.Equal("context", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        using var context = CreateContext();

        var ex = Assert.Throws<ArgumentNullException>(
            () => new MabArchiveYearRepository(context, null!, CreateSequentialLoaders(1, 24)));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoadersIsNull_ShouldThrowArgumentNullException()
    {
        using var context = CreateContext();
        var ex = Assert.Throws<ArgumentNullException>(
            () => new MabArchiveYearRepository(context, NullLogger<MabArchiveYearRepository>.Instance, null!));

        Assert.Equal("loaders", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoaderCountMismatch_ShouldThrowInvalidOperationException()
    {
        using var context = CreateContext();
        var loaders = CreateSequentialLoaders(1, 23);

        var ex = Assert.Throws<InvalidOperationException>(
            () => new MabArchiveYearRepository(context, NullLogger<MabArchiveYearRepository>.Instance, loaders));

        Assert.Contains("Expected 24 loaders but found 23", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_WhenDuplicateSequenceExists_ShouldThrowInvalidOperationException()
    {
        using var context = CreateContext();
        var loaders = CreateSequentialLoaders(1, 23)
            .Concat(new[] { CreateLoader(5, "Loader-5-Duplicate", (_, _) => Task.FromResult(0)) })
            .ToList();

        var ex = Assert.Throws<InvalidOperationException>(
            () => new MabArchiveYearRepository(context, NullLogger<MabArchiveYearRepository>.Instance, loaders));

        Assert.Contains("duplicate sequence values", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_WhenSequenceIsNotContiguous_ShouldThrowInvalidOperationException()
    {
        using var context = CreateContext();
        var loaders = CreateSequentialLoaders(1, 23)
            .Concat(new[] { CreateLoader(25, "Loader-25", (_, _) => Task.FromResult(0)) })
            .ToList();

        var ex = Assert.Throws<InvalidOperationException>(
            () => new MabArchiveYearRepository(context, NullLogger<MabArchiveYearRepository>.Instance, loaders));

        Assert.Contains("contiguous from 1 to 24", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadYearDataAsync_WhenLoadersProvidedUnordered_ShouldExecuteInSequenceOrderAndAggregateRows()
    {
        using var context = CreateContext();
        var executionOrder = new List<int>();

        var unorderedLoaders = Enumerable.Range(1, 24)
            .Reverse()
            .Select(i => CreateLoader(i, $"Loader-{i}", (_, _) =>
            {
                executionOrder.Add(i);
                return Task.FromResult(1);
            }))
            .ToList();

        var subject = new MabArchiveYearRepository(context, NullLogger<MabArchiveYearRepository>.Instance, unorderedLoaders);

        var totalRows = await subject.LoadYearDataAsync(2026, CancellationToken.None);

        Assert.Equal(24, totalRows);
        Assert.Equal(Enumerable.Range(1, 24), executionOrder);
    }

    [Fact]
    public async Task LoadYearDataAsync_WhenLoaderFails_ShouldStopAndRethrow()
    {
        using var context = CreateContext();
        var executionOrder = new List<int>();

        var loaders = Enumerable.Range(1, 24)
            .Select(i => CreateLoader(i, $"Loader-{i}", (_, _) =>
            {
                executionOrder.Add(i);

                if (i == 3)
                {
                    throw new InvalidOperationException("boom");
                }

                return Task.FromResult(1);
            }))
            .ToList();

        var subject = new MabArchiveYearRepository(context, NullLogger<MabArchiveYearRepository>.Instance, loaders);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => subject.LoadYearDataAsync(2026, CancellationToken.None));

        Assert.Equal("boom", ex.Message);
        Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
    }

    [Fact]
    public async Task DeleteYearDataAsync_WhenYearHasNoRows_ShouldReturnZero_AndRollback()
    {
        await using var context = CreatePostgresContext(GetConnectionString());
        await AssertCanConnectAsync(context);

        var subject = new MabArchiveYearRepository(
            context,
            NullLogger<MabArchiveYearRepository>.Instance,
            CreateSequentialLoaders(1, 24));

        await using var transaction = await context.Database.BeginTransactionAsync();
        var deletedRows = await subject.DeleteYearDataAsync(1900, CancellationToken.None);
        await transaction.RollbackAsync();

        Assert.Equal(0, deletedRows);
    }

    [Fact]
    public async Task DeleteYearDataAsync_WhenProviderDoesNotSupportExecuteDelete_ShouldRethrow()
    {
        await using var context = CreateContext();

        var subject = new MabArchiveYearRepository(
            context,
            NullLogger<MabArchiveYearRepository>.Instance,
            CreateSequentialLoaders(1, 24));

        await Assert.ThrowsAsync<InvalidOperationException>(() => subject.DeleteYearDataAsync(2026, CancellationToken.None));
    }

    [Fact]
    public async Task RefreshProjectsOnlyAsync_WhenLoader24ReturnsRows_ShouldReturnRows_AndRollback()
    {
        await using var context = CreatePostgresContext(GetConnectionString());
        await AssertCanConnectAsync(context);

        var invokedSequenceYears = new Dictionary<int, int>();

        var loaders = Enumerable.Range(1, 24)
            .Select(i => CreateLoader(i, $"Loader-{i}", (year, _) =>
            {
                if (i == 24)
                {
                    invokedSequenceYears[i] = year;
                    return Task.FromResult(i);
                }

                return Task.FromResult(0);
            }))
            .ToList();

        var subject = new MabArchiveYearRepository(
            context,
            NullLogger<MabArchiveYearRepository>.Instance,
            loaders);

        await using var transaction = await context.Database.BeginTransactionAsync();
        var rows = await subject.RefreshProjectsOnlyAsync(1900, CancellationToken.None);
        await transaction.RollbackAsync();

        Assert.Equal(24, rows);

        // Legacy sp_LoadFromFPS parity: only loader 24 (my_tlkpproject_all) runs for the
        // Planned-year path. g_tlkpproject (loader 2) and my_tlkpproject (loader 3) are
        // never touched here — they only refresh as part of the Open year's full load.
        Assert.Equal(new Dictionary<int, int> { [24] = 1900 }, invokedSequenceYears);
    }

    [Fact]
    public async Task RefreshProjectsOnlyAsync_WhenProviderDoesNotSupportExecuteDelete_ShouldRethrow()
    {
        await using var context = CreateContext();

        var subject = new MabArchiveYearRepository(
            context,
            NullLogger<MabArchiveYearRepository>.Instance,
            CreateSequentialLoaders(1, 24));

        await Assert.ThrowsAsync<InvalidOperationException>(() => subject.RefreshProjectsOnlyAsync(2026, CancellationToken.None));
    }

    [Fact]
    public async Task LoadYearDataAsync_WhenLoaderReturnsNegativeRows_ShouldAggregateNegativeValue()
    {
        await using var context = CreateContext();

        var loaders = Enumerable.Range(1, 24)
            .Select(i => CreateLoader(i, $"Loader-{i}", (_, _) => Task.FromResult(i == 2 ? -1 : 0)))
            .ToList();

        var subject = new MabArchiveYearRepository(context, NullLogger<MabArchiveYearRepository>.Instance, loaders);

        var totalRows = await subject.LoadYearDataAsync(2026, CancellationToken.None);

        Assert.Equal(-1, totalRows);
    }

    [Fact]
    public async Task LoadYearDataAsync_WhenLoaderIsSlow_ShouldCompleteAndAggregateRows()
    {
        await using var context = CreateContext();

        var loaders = Enumerable.Range(1, 24)
            .Select(i => CreateLoader(i, $"Loader-{i}", async (_, _) =>
            {
                if (i == 2)
                {
                    await Task.Delay(TimeSpan.FromSeconds(31));
                }

                return 1;
            }))
            .ToList();

        var subject = new MabArchiveYearRepository(context, NullLogger<MabArchiveYearRepository>.Instance, loaders);

        var totalRows = await subject.LoadYearDataAsync(2026, CancellationToken.None);

        Assert.Equal(24, totalRows);
    }

    private static BatchJobsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new BatchJobsDbContext(options);
    }

    private static BatchJobsDbContext CreatePostgresContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private static string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    private static async Task AssertCanConnectAsync(BatchJobsDbContext context)
    {
        var canConnect = await context.Database.CanConnectAsync();
        Assert.True(canConnect, "Integration DB unavailable for MyFpsYearlyDataServiceTests.");
    }

    private static List<IMabArchiveLoader> CreateSequentialLoaders(int start, int end)
    {
        return Enumerable.Range(start, end - start + 1)
            .Select(i => CreateLoader(i, $"Loader-{i}", (_, _) => Task.FromResult(0)))
            .Cast<IMabArchiveLoader>()
            .ToList();
    }

    private static TestLoader CreateLoader(int sequence, string name, Func<int, CancellationToken, Task<int>> loadFunc)
    {
        return new TestLoader(sequence, name, loadFunc);
    }

    private sealed class TestLoader : IMabArchiveLoader
    {
        private readonly Func<int, CancellationToken, Task<int>> _loadFunc;

        public TestLoader(int sequence, string name, Func<int, CancellationToken, Task<int>> loadFunc)
        {
            Sequence = sequence;
            Name = name;
            _loadFunc = loadFunc;
        }

        public int Sequence { get; }

        public string Name { get; }

        public Task<int> LoadAsync(int year, CancellationToken cancellationToken)
        {
            return _loadFunc(year, cancellationToken);
        }
    }
}
