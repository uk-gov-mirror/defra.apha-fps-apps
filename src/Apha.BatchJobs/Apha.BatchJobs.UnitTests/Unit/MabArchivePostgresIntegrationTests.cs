using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.MabArchive.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed MABArchive integration tests with skip-safe semantics.
/// Tests use transaction rollback to avoid persistent data/schema changes.
/// </summary>
[Trait("Category", "Integration")]
public sealed class MabArchivePostgresIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;

    public MabArchivePostgresIntegrationTests()
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

            var requiredViews = new[]
            {
                "qrytotaladditionalcosts",
                "qrytotalanimalcosts",
                "qrytotalstaffcosts",
                "qrytotaltestcosts"
            };

            var existingViews = await context.Database
                .SqlQueryRaw<string>(@"
                    SELECT table_name
                    FROM information_schema.views
                    WHERE table_schema = 'fps'")
                .ToListAsync();

            var missingViews = requiredViews
                .Except(existingViews, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (missingViews.Length > 0)
            {
                _skipReason = $"Integration DB missing required fps views: {string.Join(", ", missingViews)}";
            }
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task RebuildSourceTotalsAsync_WhenSourceProjectExists_ShouldInsertRows_AndRollback()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        await using var context = CreateDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync();

        var targetYear = 2099;
        var project = $"W3{Guid.NewGuid():N}"[..8].ToUpperInvariant();

        await context.Database.ExecuteSqlAsync($@"
            INSERT INTO fps.tlkpprogram (programno, sector_name, fpsyear)
            VALUES ('PRG-W3', 'Wave3 Sector', {targetYear});

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 isdefraproject, costcentre, oracleprojectcode, subaccountcode, incomeaccountcode, fpsyear,
                 profit, budget_cvl, manager, pvsincome, plancaseworkdebit)
            VALUES
                ('{project}', 'Wave3 Project', 'PRG-W3', 'Wave3 Customer', 10, 20, 'Active', 'General',
                 0, 10, 'OP-W3', 'SA-W3', 'IA-W3', {targetYear},
                 3, 5, 'Wave3 Manager', 7, 2);
        ");

        var service = new FpsTotalsRepository(
            context,
            NullLogger<FpsTotalsRepository>.Instance,
            Options.Create(new MabArchiveSettings { StrictYearIsolation = false }));

        var rows = await service.RebuildSourceTotalsAsync(targetYear, CancellationToken.None);

        Assert.True(rows > 0, "Expected at least one inserted totals row when source tlkpproject exists.");

        var insertedCount = await context.Database
            .SqlQuery<int>($@"
                SELECT COUNT(*)::int AS ""Value""
                FROM fps.fpsyeartotals
                WHERE fpsyear = {targetYear}")
            .SingleAsync();

        Assert.True(insertedCount >= 1, "Expected inserted fps.fpsyeartotals row(s) for seeded target year.");

        await transaction.RollbackAsync();
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
