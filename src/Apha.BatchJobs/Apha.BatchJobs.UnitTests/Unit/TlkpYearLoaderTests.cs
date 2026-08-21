using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.MabArchive.Loaders;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for TlkpYearLoader covering source-table contract, boundary validation,
/// and confirmed isolation from the legacy fps.tbldb_variables table.
/// Behavioral tests require a live PostgreSQL connection and use SkippableFact.
/// </summary>
[Trait("Category", "Integration")]
public sealed class TlkpYearLoaderTests : IAsyncLifetime
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";

    private readonly string _connectionString;
    private string? _skipReason;

    public TlkpYearLoaderTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var ctx = CreateDbContext();
            var canConnect = await ctx.Database.CanConnectAsync();
            if (!canConnect)
            {
                _skipReason = "Integration DB unavailable.";
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ─────────────────────────────────────────────────────────────
    // Metadata / contract (no DB connection required)
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void Loader_HasSequence16_AndName_tlkpyear()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase("tlkpyear-metadata")
            .Options;
        var loader = new TlkpYearLoader(new BatchJobsDbContext(options));

        Assert.Equal(16, loader.Sequence);
        Assert.Equal("tlkpyear", loader.Name);
    }

    [Fact]
    public void Loader_DoesNotReferenceObsoleteTblDbVariableSource()
    {
        // fps.tbldb_variables is an orphaned legacy config table with no active writer.
        // TlkpYearLoader must use fps.tblcurrentmonth exclusively in its executable code.
        // Comments may still mention the legacy table name for context (e.g. explaining
        // why it must not be used) — only code lines are checked here, not comments.
        var loaderSource = System.IO.File.ReadAllText(
            System.IO.Path.Combine(
                AppContext.BaseDirectory,
                $"../../../../Apha.BatchJobs.Infrastructure/Repositories/MabArchive/Loaders/TlkpYearLoader.cs"));

        var codeOnly = string.Join(
            '\n',
            loaderSource.Split('\n').Select(line =>
            {
                var commentIndex = line.IndexOf("//", StringComparison.Ordinal);
                return commentIndex >= 0 ? line[..commentIndex] : line;
            }));

        Assert.DoesNotContain("MaSrcTblDbVariable", codeOnly, StringComparison.Ordinal);
        Assert.DoesNotContain("DbVarName", codeOnly, StringComparison.Ordinal);
        Assert.DoesNotContain("DbVarValue", codeOnly, StringComparison.Ordinal);
        Assert.DoesNotContain("tbldb_variables", codeOnly, StringComparison.OrdinalIgnoreCase);
    }

    // ─────────────────────────────────────────────────────────────
    // Behavioral (PostgreSQL required)
    // ─────────────────────────────────────────────────────────────

    [SkippableFact]
    public async Task LoadAsync_WhenTblCurrentMonthHasOneRow_InsertsRowWithCorrectYearAndMonth()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var ctx = CreateDbContext();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        const int targetYear = 1899;

        // Capture the current value from the real source table.
        var expectedMonth = await ctx.MaSrcTblCurrentMonth
            .AsNoTracking()
            .Select(x => x.CurrentMonth)
            .SingleAsync();

        var loader = new TlkpYearLoader(ctx);
        var rowsAffected = await loader.LoadAsync(targetYear, CancellationToken.None);

        var inserted = await ctx.MaDstTlkpYear
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Year == targetYear);

        await tx.RollbackAsync();

        Assert.Equal(1, rowsAffected);
        Assert.NotNull(inserted);
        Assert.Equal(targetYear, inserted.Year);
        Assert.Equal(expectedMonth, inserted.LatestMonthReleased);
    }

    [SkippableFact]
    public async Task LoadAsync_WhenTblCurrentMonthIsEmpty_ThrowsInvalidOperationException()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var ctx = CreateDbContext();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        await ctx.Database.ExecuteSqlRawAsync("DELETE FROM fps.tblcurrentmonth");

        var loader = new TlkpYearLoader(ctx);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => loader.LoadAsync(1899, CancellationToken.None));

        await tx.RollbackAsync();

        Assert.Contains("fps.tblcurrentmonth", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("0", ex.Message, StringComparison.Ordinal);
    }

    [SkippableFact]
    public async Task LoadAsync_WhenTblCurrentMonthHasMultipleRows_ThrowsInvalidOperationException()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var ctx = CreateDbContext();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        // Insert a second row to violate the single-row contract.
        await ctx.Database.ExecuteSqlRawAsync("INSERT INTO fps.tblcurrentmonth (currentmonth) VALUES (99)");

        var loader = new TlkpYearLoader(ctx);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => loader.LoadAsync(1899, CancellationToken.None));

        await tx.RollbackAsync();

        Assert.Contains("fps.tblcurrentmonth", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public async Task LoadAsync_TblDbVariablesValues_HaveNoEffectOnResult()
    {
        // Documents the architectural decision: even with DB_Name=FPS2026 and Month=0,
        // the loader must insert the value from fps.tblcurrentmonth, not fps.tbldb_variables.
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var ctx = CreateDbContext();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        const int targetYear = 1898;

        var expectedMonth = await ctx.MaSrcTblCurrentMonth
            .AsNoTracking()
            .Select(x => x.CurrentMonth)
            .SingleAsync();

        // Confirm tbldb_variables is in the stale Planned-year state.
        var dbNameValue = await ctx.MaSrcTblDbVariable
            .AsNoTracking()
            .Where(v => v.DbVarName == "DB_Name")
            .Select(v => v.DbVarValue)
            .FirstOrDefaultAsync();

        var loader = new TlkpYearLoader(ctx);
        await loader.LoadAsync(targetYear, CancellationToken.None);

        var inserted = await ctx.MaDstTlkpYear
            .AsNoTracking()
            .SingleAsync(x => x.Year == targetYear);

        await tx.RollbackAsync();

        // Result must reflect tblcurrentmonth, not tbldb_variables.Month (which is 0).
        Assert.Equal(expectedMonth, inserted.LatestMonthReleased);
        Assert.NotEqual(0, inserted.LatestMonthReleased);
    }

    [SkippableFact]
    public async Task LoadAsync_IsIdempotent_WhenCalledTwiceForSameYear()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var ctx = CreateDbContext();
        await using var tx = await ctx.Database.BeginTransactionAsync();

        const int targetYear = 1897;
        var loader = new TlkpYearLoader(ctx);

        await loader.LoadAsync(targetYear, CancellationToken.None);
        var secondCallRows = await loader.LoadAsync(targetYear, CancellationToken.None);

        var count = await ctx.MaDstTlkpYear.CountAsync(x => x.Year == targetYear);

        await tx.RollbackAsync();

        Assert.Equal(1, secondCallRows);
        Assert.Equal(1, count);
    }

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);
}
