using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Orchestration tests for status-driven Open/Planned year processing
/// (docs/mabarchive-year-selection-processing-spec.md).
/// </summary>
public sealed class MabArchiveLoadOrchestratorParityTests
{
    private readonly IReloadFpsTotalsService _totalsService = Substitute.For<IReloadFpsTotalsService>();
    private readonly IMyFpsYearlyDataService _dataService = Substitute.For<IMyFpsYearlyDataService>();
    private readonly IExecutionYearContext _executionYearContext = Substitute.For<IExecutionYearContext>();
    private readonly IMabArchiveYearSelectionService _yearSelectionService = Substitute.For<IMabArchiveYearSelectionService>();
    private readonly IEmailNotificationService _emailNotificationService = Substitute.For<IEmailNotificationService>();

    private MabArchiveLoadOrchestrator CreateSubject()
    {
        return new MabArchiveLoadOrchestrator(
            _totalsService,
            _dataService,
            _executionYearContext,
            _yearSelectionService,
            _emailNotificationService,
            NullLogger<MabArchiveLoadOrchestrator>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlannedYearPresent_RunsOpenYearFullCycleThenPlannedYearProjectOnly()
    {
        var subject = CreateSubject();
        var ct = CancellationToken.None;

        var openYear = 2026;
        var plannedYear = 2027;
        var context = new MabArchiveExecutionContext(openYear, plannedYear);

        Func<Func<Task>, Task> transactionWrapper = work => work();

        await subject.ExecuteAsync("run-open-planned", context, transactionWrapper, ct);

        Received.InOrder(() =>
        {
            _ = _totalsService.RebuildSourceTotalsAsync(openYear, ct);
            _ = _dataService.DeleteYearDataAsync(openYear, ct);
            _ = _dataService.LoadYearDataAsync(openYear, ct);
            _ = _dataService.RefreshProjectsOnlyAsync(plannedYear, ct);
        });

        // Spec §22 scenario 8: Planned-year transactional data must never be touched.
        await _totalsService.DidNotReceive().RebuildSourceTotalsAsync(plannedYear, ct);
        await _dataService.DidNotReceive().DeleteYearDataAsync(plannedYear, ct);
        await _dataService.DidNotReceive().LoadYearDataAsync(plannedYear, ct);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoPlannedYear_RunsOpenYearOnlyAndSkipsProjectOnlyRefresh()
    {
        var subject = CreateSubject();
        var ct = CancellationToken.None;

        var openYear = 2026;
        var context = new MabArchiveExecutionContext(openYear, null);

        Func<Func<Task>, Task> transactionWrapper = work => work();

        await subject.ExecuteAsync("run-open-only", context, transactionWrapper, ct);

        await _totalsService.Received(1).RebuildSourceTotalsAsync(openYear, ct);
        await _dataService.Received(1).DeleteYearDataAsync(openYear, ct);
        await _dataService.Received(1).LoadYearDataAsync(openYear, ct);

        await _dataService.DidNotReceiveWithAnyArgs().RefreshProjectsOnlyAsync(default, default);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWorkFails_SendsFailureNotificationAndRethrows()
    {
        var subject = CreateSubject();
        var ct = CancellationToken.None;

        var context = new MabArchiveExecutionContext(2026, 2027);

        _totalsService.RebuildSourceTotalsAsync(2026, ct)
            .Returns(Task.FromException<int>(new InvalidOperationException("boom")));

        Func<Func<Task>, Task> transactionWrapper = work => work();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => subject.ExecuteAsync("run-fail", context, transactionWrapper, ct));

        Assert.Equal("boom", ex.Message);
        await _emailNotificationService.Received(1)
            .SendFailureNotificationAsync(
                "run-fail",
                "MABArchive",
                Arg.Is<string>(m => m.Contains("boom")),
                Arg.Any<DateTime>(),
                ct);

        // The Planned year must never be reached once the Open year cycle fails.
        await _dataService.DidNotReceiveWithAnyArgs().RefreshProjectsOnlyAsync(default, default);
    }

    [Fact]
    public async Task ResolveExecutionContextAsync_DelegatesToYearSelectionService()
    {
        var subject = CreateSubject();
        var ct = CancellationToken.None;
        var expected = new MabArchiveExecutionContext(2026, 2027);
        _yearSelectionService.GetProcessableYearsAsync(ct).Returns(expected);

        var actual = await subject.ResolveExecutionContextAsync(ct);

        Assert.Equal(expected, actual);
    }
}
