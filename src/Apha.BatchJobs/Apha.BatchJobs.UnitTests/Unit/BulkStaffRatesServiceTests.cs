using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class BulkStaffRatesServiceTests
{
    // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private static BulkRatesExecutionContext ValidContext(int? triggerYear = null)
        => new(Guid.NewGuid(), BatchJobNames.BulkStaffRatesUpdate, triggerYear);

    private static BulkRatesJobQueueEntry ApprovedEntry(
        string? status = "Running",
        string? jobName = null,
        int fpsYear = 2027,
        string? approvedBy = "approver@test")
        => new(
            JobQueueId:       Guid.NewGuid(),
            JobExecutionId:   Guid.NewGuid(),
            JobId:            11,
            JobName:          jobName ?? BatchJobNames.BulkStaffRatesUpdate,
            Status:           status ?? "Running",
            FpsYear:          fpsYear,
            RequestedBy:      "requester@test",
            ApprovedBy:       approvedBy,
            ApprovedAtUtc:    DateTime.UtcNow);

    private static BulkStaffRatesService CreateService(
        IBulkRatesRepository? repo = null)
        => new(
            repo ?? Substitute.For<IBulkRatesRepository>(),
            NullLogger<BulkStaffRatesService>.Instance);

    // â”€â”€ GetRunningRequestAsync returns null â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public async Task ExecuteAsync_WhenJobQueueEntryNotFound_ShouldThrow()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRunningRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((BulkRatesJobQueueEntry?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(repo).ExecuteAsync(ValidContext()));

        Assert.Contains("no job_queue row", ex.Message, StringComparison.OrdinalIgnoreCase);
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
            .Returns(ApprovedEntry(jobName: BatchJobNames.BulkAnimalRatesUpdate));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(repo).ExecuteAsync(ValidContext()));

        Assert.Contains(BatchJobNames.BulkAnimalRatesUpdate, ex.Message, StringComparison.Ordinal);
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
            () => CreateService(repo).ExecuteAsync(ValidContext(triggerYear: 2025)));

        Assert.Contains("2025", ex.Message, StringComparison.Ordinal);
        Assert.Contains("2027", ex.Message, StringComparison.Ordinal);
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
    public async Task ExecuteAsync_WhenStagingEmpty_ShouldThrow()
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetRunningRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ApprovedEntry());
        repo.GetStaffStagingRowsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<StaffStagingRow>());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(repo).ExecuteAsync(ValidContext()));

        Assert.Contains("staging", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}

