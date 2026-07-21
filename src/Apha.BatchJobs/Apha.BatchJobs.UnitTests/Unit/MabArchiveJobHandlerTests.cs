using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Context;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class MabArchiveJobHandlerTests
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=LOCAL_DB_PASSWORD;Timeout=30";

    [Fact]
    public void Constructor_WhenDbContextFactoryIsNull_ShouldThrowArgumentNullException()
    {
        var correlationService = Substitute.For<ICorrelationService>();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var ex = Assert.Throws<ArgumentNullException>(() => new MabArchiveJobHandler(
            null!,
            serviceProvider,
            correlationService,
            NullLogger<MabArchiveJobHandler>.Instance,
            Options.Create(new MabArchiveSettings())));

        Assert.Equal("dbContextFactory", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenServiceProviderIsNull_ShouldThrowArgumentNullException()
    {
        var dbContextFactory = Substitute.For<IDbContextFactory<BatchJobsDbContext>>();
        var correlationService = Substitute.For<ICorrelationService>();

        var ex = Assert.Throws<ArgumentNullException>(() => new MabArchiveJobHandler(
            dbContextFactory,
            null!,
            correlationService,
            NullLogger<MabArchiveJobHandler>.Instance,
            Options.Create(new MabArchiveSettings())));

        Assert.Equal("serviceProvider", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenCorrelationServiceIsNull_ShouldThrowArgumentNullException()
    {
        var dbContextFactory = Substitute.For<IDbContextFactory<BatchJobsDbContext>>();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var ex = Assert.Throws<ArgumentNullException>(() => new MabArchiveJobHandler(
            dbContextFactory,
            serviceProvider,
            null!,
            NullLogger<MabArchiveJobHandler>.Instance,
            Options.Create(new MabArchiveSettings())));

        Assert.Equal("correlationService", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        var dbContextFactory = Substitute.For<IDbContextFactory<BatchJobsDbContext>>();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var correlationService = Substitute.For<ICorrelationService>();

        var ex = Assert.Throws<ArgumentNullException>(() => new MabArchiveJobHandler(
            dbContextFactory,
            serviceProvider,
            correlationService,
            null!,
            Options.Create(new MabArchiveSettings())));

        Assert.Equal("logger", ex.ParamName);
    }

    [Fact]
    public void Constructor_WhenSettingsIsNull_ShouldUseDefaults()
    {
        var dbContextFactory = Substitute.For<IDbContextFactory<BatchJobsDbContext>>();
        var correlationService = Substitute.For<ICorrelationService>();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var subject = new MabArchiveJobHandler(
            dbContextFactory,
            serviceProvider,
            correlationService,
            NullLogger<MabArchiveJobHandler>.Instance,
            settings: null!);

        Assert.Equal("MABArchive", subject.Name);
    }

    [Fact]
    public void Metadata_ShouldMatchExpectedContract()
    {
        var dbContextFactory = Substitute.For<IDbContextFactory<BatchJobsDbContext>>();
        var correlationService = Substitute.For<ICorrelationService>();
        var serviceProvider = new ServiceCollection().BuildServiceProvider();

        var subject = new MabArchiveJobHandler(
            dbContextFactory,
            serviceProvider,
            correlationService,
            NullLogger<MabArchiveJobHandler>.Instance,
            Options.Create(new MabArchiveSettings()));

        Assert.Equal("MABArchive", subject.Name);
        Assert.Equal("YearScopedRebuildWithDeterministicOrdering", subject.IdempotencyStrategy);
        Assert.Equal("cron(0 20 ? * MON-FRI *)", subject.ScheduleExpression);
        Assert.Equal("Weekdays (Monday to Friday) at 8:00 PM UTC", subject.ScheduleDescription);
        Assert.Null(subject.MaxExecutionSeconds);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRunSucceeds_ShouldGenerateCorrelationAndExecuteOrchestrator()
    {
        var dataService = Substitute.For<IMyFpsYearlyDataService>();
        dataService.DeleteYearDataAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(0);
        dataService.LoadYearDataAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(0);
        dataService.RefreshProjectsOnlyAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(0);

        var totalsService = Substitute.For<IReloadFpsTotalsService>();
        totalsService.RebuildSourceTotalsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(0);

        var yearSelectionService = Substitute.For<IMabArchiveYearSelectionService>();
        yearSelectionService.GetProcessableYearsAsync(Arg.Any<CancellationToken>())
            .Returns(new MabArchiveExecutionContext(2026, 2027));

        var notificationService = Substitute.For<IEmailNotificationService>();
        var executionYearContext = new ExecutionYearContext();

        var orchestrator = new MabArchiveLoadOrchestrator(
            totalsService,
            dataService,
            executionYearContext,
            yearSelectionService,
            notificationService,
            NullLogger<MabArchiveLoadOrchestrator>.Instance);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(orchestrator)
            .BuildServiceProvider();

        var correlationService = Substitute.For<ICorrelationService>();
        correlationService.GetCorrelationId().Returns((string?)null);
        correlationService.GenerateCorrelationId().Returns("cid-wave1-success");

        var dbContextFactory = CreateDbContextFactory(GetConnectionString());
        await AssertCanConnectAsync(dbContextFactory);

        var subject = new MabArchiveJobHandler(
            dbContextFactory,
            serviceProvider,
            correlationService,
            NullLogger<MabArchiveJobHandler>.Instance,
            Options.Create(new MabArchiveSettings()));

        await subject.ExecuteAsync(CancellationToken.None);

        correlationService.Received(1).GenerateCorrelationId();

        await totalsService.Received(1).RebuildSourceTotalsAsync(2026, Arg.Any<CancellationToken>());
        await dataService.Received(1).DeleteYearDataAsync(2026, Arg.Any<CancellationToken>());
        await dataService.Received(1).LoadYearDataAsync(2026, Arg.Any<CancellationToken>());
        await dataService.Received(1).RefreshProjectsOnlyAsync(2027, Arg.Any<CancellationToken>());
        await totalsService.DidNotReceive().RebuildSourceTotalsAsync(2027, Arg.Any<CancellationToken>());
        await dataService.DidNotReceive().DeleteYearDataAsync(2027, Arg.Any<CancellationToken>());
        await dataService.DidNotReceive().LoadYearDataAsync(2027, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenOrchestratorWorkFails_ShouldRethrow()
    {
        var dataService = Substitute.For<IMyFpsYearlyDataService>();

        var totalsService = Substitute.For<IReloadFpsTotalsService>();
        totalsService
            .RebuildSourceTotalsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new InvalidOperationException("boom")));

        var yearSelectionService = Substitute.For<IMabArchiveYearSelectionService>();
        yearSelectionService.GetProcessableYearsAsync(Arg.Any<CancellationToken>())
            .Returns(new MabArchiveExecutionContext(2026, null));

        var notificationService = Substitute.For<IEmailNotificationService>();
        var executionYearContext = new ExecutionYearContext();

        var orchestrator = new MabArchiveLoadOrchestrator(
            totalsService,
            dataService,
            executionYearContext,
            yearSelectionService,
            notificationService,
            NullLogger<MabArchiveLoadOrchestrator>.Instance);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(orchestrator)
            .BuildServiceProvider();

        var correlationService = Substitute.For<ICorrelationService>();
        correlationService.GetCorrelationId().Returns("cid-wave1-fail");

        var dbContextFactory = CreateDbContextFactory(GetConnectionString());
        await AssertCanConnectAsync(dbContextFactory);

        var subject = new MabArchiveJobHandler(
            dbContextFactory,
            serviceProvider,
            correlationService,
            NullLogger<MabArchiveJobHandler>.Instance,
            Options.Create(new MabArchiveSettings()));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => subject.ExecuteAsync(CancellationToken.None));

        Assert.Equal("boom", ex.Message);
        await notificationService.Received(1)
            .SendFailureNotificationAsync(
                "cid-wave1-fail",
                "MABArchive",
                "boom",
                Arg.Any<DateTime>(),
                CancellationToken.None);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancellationRequested_ShouldRethrowOperationCanceledException()
    {
        var dataService = Substitute.For<IMyFpsYearlyDataService>();

        var totalsService = Substitute.For<IReloadFpsTotalsService>();
        totalsService
            .RebuildSourceTotalsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromCanceled<int>((CancellationToken)callInfo[1]));

        var yearSelectionService = Substitute.For<IMabArchiveYearSelectionService>();
        yearSelectionService.GetProcessableYearsAsync(Arg.Any<CancellationToken>())
            .Returns(new MabArchiveExecutionContext(2026, null));

        var notificationService = Substitute.For<IEmailNotificationService>();
        var executionYearContext = new ExecutionYearContext();

        var orchestrator = new MabArchiveLoadOrchestrator(
            totalsService,
            dataService,
            executionYearContext,
            yearSelectionService,
            notificationService,
            NullLogger<MabArchiveLoadOrchestrator>.Instance);

        var serviceProvider = new ServiceCollection()
            .AddSingleton(orchestrator)
            .BuildServiceProvider();

        var correlationService = Substitute.For<ICorrelationService>();
        correlationService.GetCorrelationId().Returns("cid-wave2-cancel");

        var dbContextFactory = CreateDbContextFactory(GetConnectionString());
        await AssertCanConnectAsync(dbContextFactory);

        var subject = new MabArchiveJobHandler(
            dbContextFactory,
            serviceProvider,
            correlationService,
            NullLogger<MabArchiveJobHandler>.Instance,
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

    private static IDbContextFactory<BatchJobsDbContext> CreateDbContextFactory(string connectionString)
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TestDbContextFactory(options);
    }

    private static async Task AssertCanConnectAsync(IDbContextFactory<BatchJobsDbContext> dbContextFactory)
    {
        await using var context = dbContextFactory.CreateDbContext();
        var canConnect = await context.Database.CanConnectAsync();
        Assert.True(canConnect, "Integration DB unavailable for MabArchiveJobHandlerTests.");
    }

    private sealed class TestDbContextFactory : IDbContextFactory<BatchJobsDbContext>
    {
        private readonly DbContextOptions<BatchJobsDbContext> _options;

        public TestDbContextFactory(DbContextOptions<BatchJobsDbContext> options)
        {
            _options = options;
        }

        public BatchJobsDbContext CreateDbContext()
        {
            return new BatchJobsDbContext(_options);
        }
    }
}