using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.BulkRates;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Services.BulkRates;
using Apha.Common.BulkRates.Validation.StaffAnimal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

public sealed class BulkStaffRatesServiceTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

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
        IBulkRatesRepository? repo = null,
        IStaffAnimalValidationService? validationService = null)
        => new(
            Substitute.For<IDbContextFactory<BatchJobsDbContext>>(),
            repo ?? Substitute.For<IBulkRatesRepository>(),
            validationService ?? new StaffAnimalValidationService(),
            NullLogger<BulkStaffRatesService>.Instance);

    // ── GetRunningRequestAsync returns null ──────────────────────────────────

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
            .Returns(ApprovedEntry(jobName: BatchJobNames.BulkAnimalRatesUpdate));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService(repo).ExecuteAsync(ValidContext()));

        Assert.Contains(BatchJobNames.BulkAnimalRatesUpdate, ex.Message, StringComparison.Ordinal);
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
            () => CreateService(repo).ExecuteAsync(ValidContext(triggerYear: 2025)));

        Assert.Contains("2025", ex.Message, StringComparison.Ordinal);
        Assert.Contains("2027", ex.Message, StringComparison.Ordinal);
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
