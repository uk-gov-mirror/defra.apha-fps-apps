using Apha.BatchJobs.Application.FailureHandling;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Worker.Configuration;
using Apha.BatchJobs.Worker.Execution;
using Apha.BatchJobs.Worker.Reporting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for <see cref="BatchWorkerRunner"/>: request resolution, orchestrator invocation,
/// outcome mapping, and that the summary writer is always invoked exactly once. HealthCheck is
/// intentionally not covered — <c>Program.cs</c> never lets it reach this runner.
/// </summary>
public sealed class BatchWorkerRunnerTests
{
    private static IServiceProvider BuildServiceProvider(IJobOrchestrator orchestrator)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => orchestrator);
        return services.BuildServiceProvider();
    }

    private static IHostApplicationLifetime CreateLifetime(out CancellationTokenSource lifetimeCts)
    {
        lifetimeCts = new CancellationTokenSource();
        var hostLifetime = Substitute.For<IHostApplicationLifetime>();
        hostLifetime.ApplicationStopping.Returns(lifetimeCts.Token);
        return hostLifetime;
    }

    private static BatchWorkerRunner CreateRunner(
        IJobOrchestrator orchestrator,
        RecordingSummaryWriter summaryWriter,
        IHostApplicationLifetime? hostLifetime,
        int overallTimeoutSeconds) =>
        new(
            new BatchExecutionRequestResolver(),
            hostLifetime ?? CreateLifetime(out _),
            Options.Create(new BatchRuntimeOptions { WorkerOverallTimeoutSeconds = overallTimeoutSeconds }),
            BuildServiceProvider(orchestrator),
            new BatchFailureClassifier(new ConfigurationBuilder().Build()),
            summaryWriter,
            NullLogger<BatchWorkerRunner>.Instance);

    private static async Task<JobExecutionResult> WaitForCancellationAsync(CancellationToken token)
    {
        await Task.Delay(Timeout.Infinite, token);
        throw new InvalidOperationException("unreachable — Task.Delay should have thrown first");
    }

    [Fact]
    public async Task RunAsync_OnSuccess_ReturnsSuccessExitCodeAndWritesSummaryOnce()
    {
        using var scope = new EnvScopeSet("RecreateSummary", "Manual", Guid.NewGuid().ToString("D"), "arihant");
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync(Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(new JobExecutionResult(Guid.NewGuid(), "RecreateSummary", JobStatus.Completed, TimeSpan.FromSeconds(1), 1));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.Success, exitCode);
        Assert.Equal(1, summaryWriter.CallCount);
        Assert.Equal(BatchRunOutcome.Success, summaryWriter.LastResult!.Outcome);
    }

    [Fact]
    public async Task RunAsync_WhenRequestResolutionFails_NeverCallsOrchestrator()
    {
        using var scope = new EnvScopeSet("<jobName>", "Manual", Guid.NewGuid().ToString("D"), "arihant");
        var orchestrator = Substitute.For<IJobOrchestrator>();
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.ConfigurationFailure, exitCode);
        Assert.Equal(1, summaryWriter.CallCount);
        Assert.Equal(BatchRunOutcome.Failure, summaryWriter.LastResult!.Outcome);
        await orchestrator.DidNotReceive().RunAsync(
            Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenOrchestratorThrowsJobLockException_MapsToLockFailure()
    {
        using var scope = new EnvScopeSet("RecreateSummary", "Manual", Guid.NewGuid().ToString("D"), "arihant");
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync(Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<JobExecutionResult>(new JobLockException("already running")));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.LockFailure, exitCode);
        Assert.Equal(BatchFailureCategory.Concurrency, summaryWriter.LastResult!.FailureCategory);
        Assert.Equal(1, summaryWriter.CallCount);
    }

    [Fact]
    public async Task RunAsync_WhenHostShutdownRequested_MapsToCancelledWithHostShutdownReason()
    {
        using var scope = new EnvScopeSet("RecreateSummary", "Manual", Guid.NewGuid().ToString("D"), "arihant");
        var hostLifetime = CreateLifetime(out var lifetimeCts);
        lifetimeCts.Cancel();

        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync(Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<JobExecutionResult>(new OperationCanceledException()));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(orchestrator, summaryWriter, hostLifetime, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.Cancelled, exitCode);
        Assert.Equal(BatchRunOutcome.Cancelled, summaryWriter.LastResult!.Outcome);
        Assert.Equal(Apha.BatchJobs.Worker.Lifecycle.ExecutionCancellationReason.HostShutdown, summaryWriter.LastResult!.CancellationReason);
    }

    [Fact]
    public async Task RunAsync_WhenOverallTimeoutFires_MapsToCancelledWithTimeoutReason()
    {
        using var scope = new EnvScopeSet("RecreateSummary", "Manual", Guid.NewGuid().ToString("D"), "arihant");
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync(Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => WaitForCancellationAsync((CancellationToken)callInfo[5]));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 1);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.Cancelled, exitCode);
        Assert.Equal(Apha.BatchJobs.Worker.Lifecycle.ExecutionCancellationReason.Timeout, summaryWriter.LastResult!.CancellationReason);
    }

    [Fact]
    public async Task RunAsync_WhenCancelledWithoutShutdownOrTimeout_MapsToUnclassified()
    {
        using var scope = new EnvScopeSet("RecreateSummary", "Manual", Guid.NewGuid().ToString("D"), "arihant");
        var orchestrator = Substitute.For<IJobOrchestrator>();
        orchestrator.RunAsync(Arg.Any<string>(), Arg.Any<RunMode>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<JobExecutionResult>(new OperationCanceledException()));
        var summaryWriter = new RecordingSummaryWriter();
        var runner = CreateRunner(orchestrator, summaryWriter, hostLifetime: null, overallTimeoutSeconds: 3600);

        var exitCode = await runner.RunAsync();

        Assert.Equal(BatchExitCodes.Cancelled, exitCode);
        Assert.Equal(Apha.BatchJobs.Worker.Lifecycle.ExecutionCancellationReason.Unclassified, summaryWriter.LastResult!.CancellationReason);
    }

    private sealed class RecordingSummaryWriter : IBatchRunSummaryWriter
    {
        public int CallCount { get; private set; }
        public BatchExecutionResult? LastResult { get; private set; }

        public void WriteSummary(BatchExecutionResult result, TimeSpan duration)
        {
            CallCount++;
            LastResult = result;
        }
    }

    private sealed class EnvScopeSet : IDisposable
    {
        private readonly List<EnvScope> _scopes = [];

        public EnvScopeSet(string? jobName, string? runMode, string? jobExecutionId, string? requestedBy)
        {
            _scopes.Add(new EnvScope("BATCH_JOB_NAME", jobName));
            _scopes.Add(new EnvScope("BATCH_RUN_MODE", runMode));
            _scopes.Add(new EnvScope("BATCH_JOB_EXECUTION_ID", jobExecutionId));
            _scopes.Add(new EnvScope("BATCH_EXECUTION_ID", null));
            _scopes.Add(new EnvScope("BATCH_REQUESTED_BY", requestedBy));
            _scopes.Add(new EnvScope("BATCH_REQUESTED_AT_UTC", null));
            _scopes.Add(new EnvScope("BATCH_PARAMETERS_JSON", null));
        }

        public void Dispose()
        {
            foreach (var envScope in _scopes)
                envScope.Dispose();
        }
    }

    private sealed class EnvScope : IDisposable
    {
        private readonly string _name;
        private readonly string? _original;

        public EnvScope(string name, string? value)
        {
            _name = name;
            _original = Environment.GetEnvironmentVariable(name);
            Environment.SetEnvironmentVariable(name, value);
        }

        public void Dispose() => Environment.SetEnvironmentVariable(_name, _original);
    }
}
