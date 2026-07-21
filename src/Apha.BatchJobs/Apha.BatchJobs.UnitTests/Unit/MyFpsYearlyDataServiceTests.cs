using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apha.BatchJobs.UnitTests;

public sealed class MyFpsYearlyDataServiceTests
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=LOCAL_DB_PASSWORD;Timeout=30";

    [Fact]
    public void Constructor_WhenContextIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(
            () => new MyFpsYearlyDataService(null!, NullLogger<MyFpsYearlyDataService>.Instance, CreateSequentialLoaders(1, 24)));

        Assert.Equal("context", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        using var context = CreateContext();

        var ex = Assert.Throws<ArgumentNullException>(
            () => new MyFpsYearlyDataService(context, null!, CreateSequentialLoaders(1, 24)));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoadersIsNull_ShouldThrowArgumentNullException()
    {
        using var context = CreateContext();

        var ex = Assert.Throws<ArgumentNullException>(
            () => new MyFpsYearlyDataService(context, NullLogger<MyFpsYearlyDataService>.Instance, null!));

        Assert.Equal("loaders", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoaderCountMismatch_ShouldThrowInvalidOperationException()
    {
        using var context = CreateContext();
        var loaders = CreateSequentialLoaders(1, 23);

        var ex = Assert.Throws<InvalidOperationException>(
            () => new MyFpsYearlyDataService(context, NullLogger<MyFpsYearlyDataService>.Instance, loaders));

        Assert.Contains("Expected 24 loaders but found 23", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_WhenDuplicateSequenceExists_ShouldThrowInvalidOperationException()
    {
        using var context = CreateContext();
        var loaders = CreateSequentialLoaders(1, 23)
            .Concat(new[] { CreateLoader(5, "Loader-5-Duplicate", (_, _, _) => Task.FromResult(0)) })
            .ToList();

        var ex = Assert.Throws<InvalidOperationException>(
            () => new MyFpsYearlyDataService(context, NullLogger<MyFpsYearlyDataService>.Instance, loaders));

        Assert.Contains("duplicate sequence values", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_WhenSequenceIsNotContiguous_ShouldThrowInvalidOperationException()
    {
        using var context = CreateContext();
        var loaders = CreateSequentialLoaders(1, 23)
            .Concat(new[] { CreateLoader(25, "Loader-25", (_, _, _) => Task.FromResult(0)) })
            .ToList();

        var ex = Assert.Throws<InvalidOperationException>(
            () => new MyFpsYearlyDataService(context, NullLogger<MyFpsYearlyDataService>.Instance, loaders));

        Assert.Contains("contiguous from 1 to 24", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoadYearDataAsync_WhenLoadersProvidedUnordered_ShouldExecuteInSequenceOrderAndAggregateRows()
    {
        using var context = CreateContext();
        var executionOrder = new List<int>();

        var unorderedLoaders = Enumerable.Range(1, 24)
            .Reverse()
            .Select(i => CreateLoader(i, $"Loader-{i}", (_, _, _) =>
            {
                executionOrder.Add(i);
                return Task.FromResult(1);
            }))
            .ToList();

        var subject = new MyFpsYearlyDataService(context, NullLogger<MyFpsYearlyDataService>.Instance, unorderedLoaders);

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
            .Select(i => CreateLoader(i, $"Loader-{i}", (_, _, _) =>
            {
                executionOrder.Add(i);

                if (i == 3)
                {
                    throw new InvalidOperationException("boom");
                }

                return Task.FromResult(1);
            }))
            .ToList();

        var subject = new MyFpsYearlyDataService(context, NullLogger<MyFpsYearlyDataService>.Instance, loaders);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => subject.LoadYearDataAsync(2026, CancellationToken.None));

        Assert.Equal("boom", ex.Message);
        Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
    }

    [Fact]
    public async Task DeleteYearDataAsync_WhenYearHasNoRows_ShouldReturnZero_AndRollback()
    {
        await using var context = CreatePostgresContext(GetConnectionString());
        await AssertCanConnectAsync(context);

        var subject = new MyFpsYearlyDataService(
            context,
            NullLogger<MyFpsYearlyDataService>.Instance,
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

        var subject = new MyFpsYearlyDataService(
            context,
            NullLogger<MyFpsYearlyDataService>.Instance,
            CreateSequentialLoaders(1, 24));

        await Assert.ThrowsAsync<InvalidOperationException>(() => subject.DeleteYearDataAsync(2026, CancellationToken.None));
    }

    [Fact]
    public async Task RefreshProjectsOnlyAsync_WhenLoaders2And3And24ReturnRows_ShouldAggregateRows_AndRollback()
    {
        await using var context = CreatePostgresContext(GetConnectionString());
        await AssertCanConnectAsync(context);

        var invokedSequenceYears = new Dictionary<int, int>();

        var loaders = Enumerable.Range(1, 24)
            .Select(i => CreateLoader(i, $"Loader-{i}", (_, year, _) =>
            {
                if (i is 2 or 3 or 24)
                {
                    invokedSequenceYears[i] = year;
                    return Task.FromResult(i);
                }

                return Task.FromResult(0);
            }))
            .ToList();

        var subject = new MyFpsYearlyDataService(
            context,
            NullLogger<MyFpsYearlyDataService>.Instance,
            loaders);

        await using var transaction = await context.Database.BeginTransactionAsync();
        var rows = await subject.RefreshProjectsOnlyAsync(1900, CancellationToken.None);
        await transaction.RollbackAsync();

        Assert.Equal(2 + 3 + 24, rows);
        Assert.Equal(new Dictionary<int, int> { [2] = 1900, [3] = 1900, [24] = 1900 }, invokedSequenceYears);

        // Only loaders 2, 3, and 24 (project master/lookup/cross-reference) run; nothing else does.
        Assert.Equal(3, invokedSequenceYears.Count);
    }

    [Fact]
    public async Task RefreshProjectsOnlyAsync_WhenProviderDoesNotSupportExecuteDelete_ShouldRethrow()
    {
        await using var context = CreateContext();

        var subject = new MyFpsYearlyDataService(
            context,
            NullLogger<MyFpsYearlyDataService>.Instance,
            CreateSequentialLoaders(1, 24));

        await Assert.ThrowsAsync<InvalidOperationException>(() => subject.RefreshProjectsOnlyAsync(2026, CancellationToken.None));
    }

    [Fact]
    public async Task LoadYearDataAsync_WhenLoaderReturnsNegativeRows_ShouldAggregateNegativeValue()
    {
        await using var context = CreateContext();

        var loaders = Enumerable.Range(1, 24)
            .Select(i => CreateLoader(i, $"Loader-{i}", (_, _, _) => Task.FromResult(i == 2 ? -1 : 0)))
            .ToList();

        var subject = new MyFpsYearlyDataService(context, NullLogger<MyFpsYearlyDataService>.Instance, loaders);

        var totalRows = await subject.LoadYearDataAsync(2026, CancellationToken.None);

        Assert.Equal(-1, totalRows);
    }

    [Fact]
    public async Task LoadYearDataAsync_WhenLoaderIsSlow_ShouldCompleteAndAggregateRows()
    {
        await using var context = CreateContext();

        var loaders = Enumerable.Range(1, 24)
            .Select(i => CreateLoader(i, $"Loader-{i}", async (_, _, _) =>
            {
                if (i == 2)
                {
                    await Task.Delay(TimeSpan.FromSeconds(31));
                }

                return 1;
            }))
            .ToList();

        var subject = new MyFpsYearlyDataService(context, NullLogger<MyFpsYearlyDataService>.Instance, loaders);

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
            .Select(i => CreateLoader(i, $"Loader-{i}", (_, _, _) => Task.FromResult(0)))
            .Cast<IMabArchiveLoader>()
            .ToList();
    }

    private static TestLoader CreateLoader(int sequence, string name, Func<BatchJobsDbContext, int, CancellationToken, Task<int>> loadFunc)
    {
        return new TestLoader(sequence, name, loadFunc);
    }

    private sealed class TestLoader : IMabArchiveLoader
    {
        private readonly Func<BatchJobsDbContext, int, CancellationToken, Task<int>> _loadFunc;

        public TestLoader(int sequence, string name, Func<BatchJobsDbContext, int, CancellationToken, Task<int>> loadFunc)
        {
            Sequence = sequence;
            Name = name;
            _loadFunc = loadFunc;
        }

        public int Sequence { get; }

        public string Name { get; }

        public Task<int> LoadAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
        {
            return _loadFunc(context, year, cancellationToken);
        }
    }
}