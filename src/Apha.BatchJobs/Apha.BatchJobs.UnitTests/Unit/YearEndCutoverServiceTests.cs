using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class YearEndCutoverServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCurrentYearMissing_ShouldThrow()
    {
        var service = CreateService();
        var context = new YearEndExecutionContext("corr-1", null, CurrentFpsYear: null, TargetFpsYear: 2027);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));

        Assert.Contains("currentFpsYear", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTargetYearMissing_ShouldThrow()
    {
        var service = CreateService();
        var context = new YearEndExecutionContext("corr-2", null, CurrentFpsYear: 2026, TargetFpsYear: null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));

        Assert.Contains("targetFpsYear", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTargetYearNotGreaterThanCurrent_ShouldThrow()
    {
        var service = CreateService();
        var context = new YearEndExecutionContext("corr-3", null, CurrentFpsYear: 2027, TargetFpsYear: 2027);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));

        Assert.Contains("targetFpsYear", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoDataSetupExecutionFoundForTargetYear_ShouldThrow()
    {
        var executionRepository = Substitute.For<IJobExecutionRepository>();
        executionRepository
            .GetLastExecutionByFpsYearAsync(BatchJobNames.YearEndDataSetup, 2027, Arg.Any<CancellationToken>())
            .Returns((JobExecutionRecord?)null);

        var service = CreateService(executionRepository);
        var context = new YearEndExecutionContext("corr-4", null, CurrentFpsYear: 2026, TargetFpsYear: 2027);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));

        Assert.Contains(BatchJobNames.YearEndDataSetup, ex.Message, StringComparison.Ordinal);
        Assert.Contains("Completed", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLatestDataSetupExecutionNotCompleted_ShouldThrow()
    {
        var executionRepository = Substitute.For<IJobExecutionRepository>();
        executionRepository
            .GetLastExecutionByFpsYearAsync(BatchJobNames.YearEndDataSetup, 2027, Arg.Any<CancellationToken>())
            .Returns(new JobExecutionRecord
            {
                ExecutionId = 1,
                JobName = BatchJobNames.YearEndDataSetup,
                JobExecutionId = Guid.NewGuid(),
                JobQueueId = Guid.NewGuid(),
                UserId = "test-user",
                JobType = JobType.Unknown,
                RunMode = RunMode.Manual,
                Status = JobStatus.Failed,
                StartedAt = DateTime.UtcNow,
                FpsYear = 2027
            });

        var service = CreateService(executionRepository);
        var context = new YearEndExecutionContext("corr-5", null, CurrentFpsYear: 2026, TargetFpsYear: 2027);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExecuteAsync(context));

        Assert.Contains("Failed", ex.Message, StringComparison.Ordinal);
    }

    private static YearEndCutoverService CreateService(IJobExecutionRepository? executionRepository = null)
    {
        return new YearEndCutoverService(
            Substitute.For<IYearEndCutoverRepository>(),
            executionRepository ?? Substitute.For<IJobExecutionRepository>(),
            NullLogger<YearEndCutoverService>.Instance);
    }
}
