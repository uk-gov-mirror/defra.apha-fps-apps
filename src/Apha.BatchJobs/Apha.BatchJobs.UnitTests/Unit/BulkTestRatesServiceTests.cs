using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class BulkTestRatesServiceTests
{
    // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static BulkRatesExecutionContext ValidContext(Guid? jobExecutionId = null, int? triggerYear = null)
        => new(jobExecutionId ?? Guid.NewGuid(), BatchJobNames.BulkTestRatesUpdate, triggerYear);

    private static BulkRatesJobQueueEntry ApprovedEntry(
        Guid? jobQueueId = null,
        Guid? jobExecutionId = null,
        string? status = "Running",
        string? jobName = null,
        int fpsYear = 2027,
        string? approvedBy = "approver@test",
        int? uploadVersion = 1)
        => new(
            JobQueueId:       jobQueueId ?? Guid.NewGuid(),
            JobExecutionId:   jobExecutionId ?? Guid.NewGuid(),
            JobId:            10,
            JobName:          jobName ?? BatchJobNames.BulkTestRatesUpdate,
            Status:           status ?? "Running",
            FpsYear:          fpsYear,
            RequestedBy:      "requester@test",
            ApprovedBy:       approvedBy,
            ApprovedAtUtc:    DateTime.UtcNow,
            UploadVersion:    uploadVersion);

    private static BulkTestRatesService CreateService(
        IBulkRatesRepository? repo = null)
        => new(
            repo ?? Substitute.For<IBulkRatesRepository>(),
            Substitute.For<IJobExecutionRepository>(),
            NullLogger<BulkTestRatesService>.Instance);

    // â”€â”€ GetRunningRequestAsync returns null â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public async Task ExecuteAsync_WhenJobQueueEntryNotFound_ShouldThrow()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRunningRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((BulkRatesJobQueueEntry?)null);

        var svc = CreateService(repo);
        var ctx = ValidContext();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => svc.ExecuteAsync(ctx));

        Assert.Contains(ctx.JobExecutionId.ToString("D"), ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // â”€â”€ Precondition: Status â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public async Task ExecuteAsync_WhenStatusNotRunning_ShouldThrow()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRunningRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApprovedEntry(status: "Submitted"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(repo).ExecuteAsync(ValidContext()));

        Assert.Contains("Submitted", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Running", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // â”€â”€ Precondition: JobName â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public async Task ExecuteAsync_WhenJobNameMismatch_ShouldThrow()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRunningRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApprovedEntry(jobName: BatchJobNames.BulkStaffRatesUpdate));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(repo).ExecuteAsync(ValidContext()));

        Assert.Contains(BatchJobNames.BulkStaffRatesUpdate, ex.Message, StringComparison.Ordinal);
    }

    // â”€â”€ Precondition: FpsYear â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public async Task ExecuteAsync_WhenFpsYearZero_ShouldThrow()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRunningRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApprovedEntry(fpsYear: 0));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(repo).ExecuteAsync(ValidContext()));

        Assert.Contains("fpsyear", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // â”€â”€ Precondition: TriggerYear mismatch â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public async Task ExecuteAsync_WhenTriggerYearMismatchesPersistedYear_ShouldThrow()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRunningRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApprovedEntry(fpsYear: 2027));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(repo).ExecuteAsync(ValidContext(triggerYear: 2026)));

        Assert.Contains("2026", ex.Message, StringComparison.Ordinal);
        Assert.Contains("2027", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTriggerYearMatchesPersistedYear_ShouldProceedPastYearCheck()
    {
        // Arrange: matching trigger year â€” should pass the year check and reach staging guard
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRunningRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApprovedEntry(fpsYear: 2027));
        repo.GetFecStagingRowsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<FecStagingRow>());
        repo.GetAgrupStagingRowsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<AgrupStagingRow>());

        // Act: should NOT throw a year-mismatch error; will throw a staging-empty error instead
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(repo).ExecuteAsync(ValidContext(triggerYear: 2027)));

        Assert.DoesNotContain("2027", ex.Message.Split("trigger", StringSplitOptions.None)[0],
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("staging", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // â”€â”€ Precondition: ApprovalMetadata â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public async Task ExecuteAsync_WhenApprovedByMissing_ShouldThrow()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRunningRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApprovedEntry(approvedBy: null));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(repo).ExecuteAsync(ValidContext()));

        Assert.Contains("approval metadata", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WhenApprovedAtUtcMissing_ShouldThrow()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRunningRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApprovedEntry() with { ApprovedAtUtc = null });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(repo).ExecuteAsync(ValidContext()));

        Assert.Contains("approval metadata", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // â”€â”€ Staging guard â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public async Task ExecuteAsync_WhenBothFecAndAgrupStagingEmpty_ShouldThrow()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRunningRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApprovedEntry());
        repo.GetFecStagingRowsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<FecStagingRow>());
        repo.GetAgrupStagingRowsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<AgrupStagingRow>());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(repo).ExecuteAsync(ValidContext()));

        Assert.Contains("staging", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}

