using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Services.BulkRates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class BulkTestRatesServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static BulkRatesExecutionContext ValidContext(Guid? jobExecutionId = null, int? triggerYear = null)
        => new(jobExecutionId ?? Guid.NewGuid(), BatchJobNames.BulkTestRatesUpdate, triggerYear);

    private static BulkRatesJobQueueEntry ApprovedEntry(
        Guid? jobQueueId = null,
        Guid? jobExecutionId = null,
        string? status = "Running",
        string? jobName = null,
        int fpsYear = 2027,
        string? approvedBy = "approver@test")
        => new(
            JobQueueId:       jobQueueId ?? Guid.NewGuid(),
            JobExecutionId:   jobExecutionId ?? Guid.NewGuid(),
            JobId:            10,
            JobName:          jobName ?? BatchJobNames.BulkTestRatesUpdate,
            Status:           status ?? "Running",
            FpsYear:          fpsYear,
            RequestedBy:      "requester@test",
            ApprovedBy:       approvedBy,
            ApprovedAtUtc:    DateTime.UtcNow);

    private static BulkTestRatesService CreateService(IBulkRatesRepository? repo = null)
        => new(
            Substitute.For<IDbContextFactory<BatchJobsDbContext>>(),
            repo ?? Substitute.For<IBulkRatesRepository>(),
            Substitute.For<IJobExecutionRepository>(),
            NullLogger<BulkTestRatesService>.Instance);

    // ── GetRunningRequestAsync returns null ──────────────────────────────────

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

    // ── Precondition: Status ─────────────────────────────────────────────────

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

    // ── Precondition: JobName ─────────────────────────────────────────────────

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

    // ── Precondition: FpsYear ─────────────────────────────────────────────────

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

    // ── Precondition: TriggerYear mismatch ───────────────────────────────────

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
        // Arrange: matching trigger year — should pass the year check and reach staging guard
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

    // ── Precondition: ApprovalMetadata ───────────────────────────────────────

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

    // ── Staging guard ─────────────────────────────────────────────────────────

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
