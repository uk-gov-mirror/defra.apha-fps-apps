using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Entities.MabArchive;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces.MabArchive;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Context;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for <see cref="MabArchiveJob"/> covering job metadata and status-driven Open/Planned
/// year processing (docs/mabarchive-year-selection-processing-spec.md). Failure-notification
/// behavior lives in <c>JobOrchestratorTests</c> now that <see cref="MabArchiveJob"/> no longer
/// sends its own notifications.
/// </summary>
public sealed class MabArchiveJobTests
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=LOCAL_DB_PASSWORD;Timeout=30";

    [Fact]
    public void Constructor_WhenDbContextIsNull_ShouldThrowArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new MabArchiveJob(
            null!,
            Substitute.For<IMabArchiveYearSelectionRepository>(),
            Substitute.For<IFpsTotalsRepository>(),
            Substitute.For<IMabArchiveYearRepository>(),
            new ExecutionYearContext(),
            Substitute.For<ICorrelationService>(),
            NullLogger<MabArchiveJob>.Instance,
            Options.Create(new MabArchiveSettings())));

        Assert.Equal("transactionManager", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenYearSelectionRepositoryIsNull_ShouldThrowArgumentNullException()
    {
        var transactionManager = Substitute.For<IMabArchiveTransactionManager>();

        var ex = Assert.Throws<ArgumentNullException>(() => new MabArchiveJob(
            transactionManager,
            null!,
            Substitute.For<IFpsTotalsRepository>(),
            Substitute.For<IMabArchiveYearRepository>(),
            new ExecutionYearContext(),
            Substitute.For<ICorrelationService>(),
            NullLogger<MabArchiveJob>.Instance,
            Options.Create(new MabArchiveSettings())));

        Assert.Equal("yearSelectionRepository", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenTotalsServiceIsNull_ShouldThrowArgumentNullException()
    {
        var transactionManager = Substitute.For<IMabArchiveTransactionManager>();

        var ex = Assert.Throws<ArgumentNullException>(() => new MabArchiveJob(
            transactionManager,
            Substitute.For<IMabArchiveYearSelectionRepository>(),
            null!,
            Substitute.For<IMabArchiveYearRepository>(),
            new ExecutionYearContext(),
            Substitute.For<ICorrelationService>(),
            NullLogger<MabArchiveJob>.Instance,
            Options.Create(new MabArchiveSettings())));

        Assert.Equal("totalsRepository", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenDataServiceIsNull_ShouldThrowArgumentNullException()
    {
        var transactionManager = Substitute.For<IMabArchiveTransactionManager>();

        var ex = Assert.Throws<ArgumentNullException>(() => new MabArchiveJob(
            transactionManager,
            Substitute.For<IMabArchiveYearSelectionRepository>(),
            Substitute.For<IFpsTotalsRepository>(),
            null!,
            new ExecutionYearContext(),
            Substitute.For<ICorrelationService>(),
            NullLogger<MabArchiveJob>.Instance,
            Options.Create(new MabArchiveSettings())));

        Assert.Equal("yearRepository", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenExecutionYearContextIsNull_ShouldThrowArgumentNullException()
    {
        var transactionManager = Substitute.For<IMabArchiveTransactionManager>();

        var ex = Assert.Throws<ArgumentNullException>(() => new MabArchiveJob(
            transactionManager,
            Substitute.For<IMabArchiveYearSelectionRepository>(),
            Substitute.For<IFpsTotalsRepository>(),
            Substitute.For<IMabArchiveYearRepository>(),
            null!,
            Substitute.For<ICorrelationService>(),
            NullLogger<MabArchiveJob>.Instance,
            Options.Create(new MabArchiveSettings())));

        Assert.Equal("executionYearContext", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenCorrelationServiceIsNull_ShouldThrowArgumentNullException()
    {
        var transactionManager = Substitute.For<IMabArchiveTransactionManager>();

        var ex = Assert.Throws<ArgumentNullException>(() => new MabArchiveJob(
            transactionManager,
            Substitute.For<IMabArchiveYearSelectionRepository>(),
            Substitute.For<IFpsTotalsRepository>(),
            Substitute.For<IMabArchiveYearRepository>(),
            new ExecutionYearContext(),
            null!,
            NullLogger<MabArchiveJob>.Instance,
            Options.Create(new MabArchiveSettings())));

        Assert.Equal("correlationService", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        var transactionManager = Substitute.For<IMabArchiveTransactionManager>();

        var ex = Assert.Throws<ArgumentNullException>(() => new MabArchiveJob(
            transactionManager,
            Substitute.For<IMabArchiveYearSelectionRepository>(),
            Substitute.For<IFpsTotalsRepository>(),
            Substitute.For<IMabArchiveYearRepository>(),
            new ExecutionYearContext(),
            Substitute.For<ICorrelationService>(),
            null!,
            Options.Create(new MabArchiveSettings())));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenSettingsIsNull_ShouldUseDefaults()
    {
        var transactionManager = Substitute.For<IMabArchiveTransactionManager>();

        var subject = new MabArchiveJob(
            transactionManager,
            Substitute.For<IMabArchiveYearSelectionRepository>(),
            Substitute.For<IFpsTotalsRepository>(),
            Substitute.For<IMabArchiveYearRepository>(),
            new ExecutionYearContext(),
            Substitute.For<ICorrelationService>(),
            NullLogger<MabArchiveJob>.Instance,
            settings: null!);

        Assert.Equal("MABArchive", subject.Name);
    }

    [Fact]
    public void Metadata_ShouldMatchExpectedContract()
    {
        var transactionManager = Substitute.For<IMabArchiveTransactionManager>();

        var subject = new MabArchiveJob(
            transactionManager,
            Substitute.For<IMabArchiveYearSelectionRepository>(),
            Substitute.For<IFpsTotalsRepository>(),
            Substitute.For<IMabArchiveYearRepository>(),
            new ExecutionYearContext(),
            Substitute.For<ICorrelationService>(),
            NullLogger<MabArchiveJob>.Instance,
            Options.Create(new MabArchiveSettings()));

        Assert.Equal("MABArchive", subject.Name);
        Assert.Equal("YearScopedRebuildWithDeterministicOrdering", subject.IdempotencyStrategy);
        Assert.Equal("cron(0 20 ? * MON-FRI *)", subject.ScheduleExpression);
        Assert.Equal("Weekdays (Monday to Friday) at 8:00 PM UTC", subject.ScheduleDescription);
        Assert.Null(subject.MaxExecutionSeconds);
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenPlannedYearPresent_RunsOpenYearFullCycleThenPlannedYearProjectOnly()
    {
        var dataService = Substitute.For<IMabArchiveYearRepository>();
        dataService.DeleteYearDataAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(0);
        dataService.LoadYearDataAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(0);
        dataService.RefreshProjectsOnlyAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(0);

        var totalsService = Substitute.For<IFpsTotalsRepository>();
        totalsService.RebuildSourceTotalsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(0);

        var yearSelectionService = Substitute.For<IMabArchiveYearSelectionRepository>();
        yearSelectionService.GetProcessableYearsAsync(Arg.Any<CancellationToken>())
            .Returns(new MabArchiveExecutionContext(2026, 2027));

        var correlationService = Substitute.For<ICorrelationService>();
        correlationService.GetCorrelationId().Returns((string?)null);
        correlationService.GenerateCorrelationId().Returns("cid-open-planned");

        await using var dbContext = CreateDbContext();
        await AssertCanConnectAsync(dbContext);

        var subject = new MabArchiveJob(
            new MabArchiveTransactionManager(dbContext),
            yearSelectionService,
            totalsService,
            dataService,
            new ExecutionYearContext(),
            correlationService,
            NullLogger<MabArchiveJob>.Instance,
            Options.Create(new MabArchiveSettings()));

        await subject.ExecuteAsync(CancellationToken.None);

        correlationService.Received(1).GenerateCorrelationId();

        Received.InOrder(() =>
        {
            _ = totalsService.RebuildSourceTotalsAsync(2026, Arg.Any<CancellationToken>());
            _ = dataService.DeleteYearDataAsync(2026, Arg.Any<CancellationToken>());
            _ = dataService.LoadYearDataAsync(2026, Arg.Any<CancellationToken>());
            _ = dataService.RefreshProjectsOnlyAsync(2027, Arg.Any<CancellationToken>());
        });

        // Spec: Planned-year transactional data must never be touched.
        await totalsService.DidNotReceive().RebuildSourceTotalsAsync(2027, Arg.Any<CancellationToken>());
        await dataService.DidNotReceive().DeleteYearDataAsync(2027, Arg.Any<CancellationToken>());
        await dataService.DidNotReceive().LoadYearDataAsync(2027, Arg.Any<CancellationToken>());
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenNoPlannedYear_RunsOpenYearOnlyAndSkipsProjectOnlyRefresh()
    {
        var dataService = Substitute.For<IMabArchiveYearRepository>();
        dataService.DeleteYearDataAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(0);
        dataService.LoadYearDataAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(0);

        var totalsService = Substitute.For<IFpsTotalsRepository>();
        totalsService.RebuildSourceTotalsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(0);

        var yearSelectionService = Substitute.For<IMabArchiveYearSelectionRepository>();
        yearSelectionService.GetProcessableYearsAsync(Arg.Any<CancellationToken>())
            .Returns(new MabArchiveExecutionContext(2026, null));

        var correlationService = Substitute.For<ICorrelationService>();
        correlationService.GetCorrelationId().Returns("cid-open-only");

        await using var dbContext = CreateDbContext();
        await AssertCanConnectAsync(dbContext);

        var subject = new MabArchiveJob(
            new MabArchiveTransactionManager(dbContext),
            yearSelectionService,
            totalsService,
            dataService,
            new ExecutionYearContext(),
            correlationService,
            NullLogger<MabArchiveJob>.Instance,
            Options.Create(new MabArchiveSettings()));

        await subject.ExecuteAsync(CancellationToken.None);

        await totalsService.Received(1).RebuildSourceTotalsAsync(2026, Arg.Any<CancellationToken>());
        await dataService.Received(1).DeleteYearDataAsync(2026, Arg.Any<CancellationToken>());
        await dataService.Received(1).LoadYearDataAsync(2026, Arg.Any<CancellationToken>());
        await dataService.DidNotReceiveWithAnyArgs().RefreshProjectsOnlyAsync(default, default);
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenProcessingFails_ShouldRethrow()
    {
        var dataService = Substitute.For<IMabArchiveYearRepository>();

        var totalsService = Substitute.For<IFpsTotalsRepository>();
        totalsService
            .RebuildSourceTotalsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new InvalidOperationException("boom")));

        var yearSelectionService = Substitute.For<IMabArchiveYearSelectionRepository>();
        yearSelectionService.GetProcessableYearsAsync(Arg.Any<CancellationToken>())
            .Returns(new MabArchiveExecutionContext(2026, null));

        var correlationService = Substitute.For<ICorrelationService>();
        correlationService.GetCorrelationId().Returns("cid-fail");

        await using var dbContext = CreateDbContext();
        await AssertCanConnectAsync(dbContext);

        var subject = new MabArchiveJob(
            new MabArchiveTransactionManager(dbContext),
            yearSelectionService,
            totalsService,
            dataService,
            new ExecutionYearContext(),
            correlationService,
            NullLogger<MabArchiveJob>.Instance,
            Options.Create(new MabArchiveSettings()));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => subject.ExecuteAsync(CancellationToken.None));

        Assert.Equal("boom", ex.Message);

        // The Planned year must never be reached once the Open year cycle fails.
        await dataService.DidNotReceiveWithAnyArgs().RefreshProjectsOnlyAsync(default, default);
    }

    [SkippableFact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldRethrowOperationCanceledException()
    {
        var dataService = Substitute.For<IMabArchiveYearRepository>();

        var totalsService = Substitute.For<IFpsTotalsRepository>();
        totalsService
            .RebuildSourceTotalsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromCanceled<int>((CancellationToken)callInfo[1]));

        var yearSelectionService = Substitute.For<IMabArchiveYearSelectionRepository>();
        yearSelectionService.GetProcessableYearsAsync(Arg.Any<CancellationToken>())
            .Returns(new MabArchiveExecutionContext(2026, null));

        var correlationService = Substitute.For<ICorrelationService>();
        correlationService.GetCorrelationId().Returns("cid-cancel");

        await using var dbContext = CreateDbContext();
        await AssertCanConnectAsync(dbContext);

        var subject = new MabArchiveJob(
            new MabArchiveTransactionManager(dbContext),
            yearSelectionService,
            totalsService,
            dataService,
            new ExecutionYearContext(),
            correlationService,
            NullLogger<MabArchiveJob>.Instance,
            Options.Create(new MabArchiveSettings()));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => subject.ExecuteAsync(cts.Token));
    }

    private static string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    private static BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(GetConnectionString())
            .Options;

        return new BatchJobsDbContext(options);
    }

    private static async Task AssertCanConnectAsync(BatchJobsDbContext dbContext)
    {
        bool canConnect;
        try
        {
            canConnect = await dbContext.Database.CanConnectAsync();
        }
        catch
        {
            canConnect = false;
        }

        Skip.IfNot(canConnect, "Integration DB unavailable for MabArchiveJobTests.");
    }
}
