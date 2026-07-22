using Apha.BatchJobs.Application;
using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Entities;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Tests for <see cref="JobOrchestrator"/> covering the full execution lifecycle:
/// lock acquire → record start → execute → record complete → release lock.
/// </summary>
public sealed class JobOrchestratorTests
{
    private readonly IBatchJobFactory _factory = Substitute.For<IBatchJobFactory>();
    private readonly IBatchLockRepository _lockRepo = Substitute.For<IBatchLockRepository>();
    private readonly IJobExecutionRepository _execRepo = Substitute.For<IJobExecutionRepository>();
    private readonly ICorrelationService _correlationService = Substitute.For<ICorrelationService>();
    private readonly IConfiguration _configuration = Substitute.For<IConfiguration>();
    private readonly IOptions<BatchJobSettings> _settings = Options.Create(new BatchJobSettings { JobTimeout = 3600 });
    private readonly JobOrchestrator _orchestrator;

    public JobOrchestratorTests()
    {
        _orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
            _correlationService,
            _settings,
            _configuration,
            NullLogger<JobOrchestrator>.Instance);
    }

    // ─────────────────────────────────────────────────────────────
    // Happy path: lock acquired, job succeeds
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_WhenLockAcquired_ExecutesJobAndWritesRecords()
    {
        // Arrange — capture argument state at call time (not at assertion time)
        // because JobExecutionRecord is a mutable reference type that gets updated
        // between CreateExecutionRecordAsync and UpdateExecutionRecordAsync.
        SetupInitiatedExecution("TestJob");
        var capturedCreateStatus = new List<JobStatus>();
        var capturedUpdateStatus = new List<JobStatus>();

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("TestJob");

        _factory.Create("TestJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("TestJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(
                     Arg.Do<JobExecutionRecord>(r => capturedCreateStatus.Add(r.Status)),
                     Arg.Any<CancellationToken>())
                 .Returns(42);
        _execRepo.UpdateExecutionRecordAsync(
                     Arg.Do<JobExecutionRecord>(r => capturedUpdateStatus.Add(r.Status)),
                     Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act
        var jobExecutionId = Guid.NewGuid();
        var result = await _orchestrator.RunAsync("TestJob", RunMode.Manual, jobExecutionId, "test-user");

        // Assert — job was called
        await job.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());

        // Assert — execution record created with Running status
        Assert.Single(capturedCreateStatus);
        Assert.Equal(JobStatus.Running, capturedCreateStatus[0]);

        // Assert — execution record updated with Completed status
        Assert.Single(capturedUpdateStatus);
        Assert.Equal(JobStatus.Completed, capturedUpdateStatus[0]);

        // Assert — lock was released
        await _lockRepo.Received(1).ReleaseLockAsync("TestJob", result.JobQueueId, Arg.Any<CancellationToken>());

        // Assert — result is correct
        Assert.Equal(JobStatus.Completed, result.Status);
        Assert.Equal("TestJob", result.JobName);
        Assert.NotEqual(Guid.Empty, result.JobQueueId);
        Assert.Equal(42, result.ExecutionId);
    }

    /// <summary>
    /// Pins the contract the Worker startup refactor relies on: <c>RunAsync</c> only ever
    /// <em>returns</em> when the job succeeded (<see cref="JobStatus.Completed"/>) — any failure
    /// or cancellation is always thrown, never returned as a <see cref="JobStatus.Failed"/> or
    /// cancelled result. This means <c>BatchWorkerRunner</c> needs no status-mapper branch: a
    /// normal return from <c>RunAsync</c> can be treated as success without inspecting
    /// <see cref="JobExecutionResult.Status"/> at all. If this test ever needs to change, the
    /// worker-side "no status mapper" simplification needs to change with it.
    /// </summary>
    [Fact]
    public async Task RunAsync_OnSuccess_ReturnsCompletedStatus_OnFailure_ThrowsInsteadOfReturningFailedStatus()
    {
        // Success path — the only status a normal return can carry is Completed.
        SetupInitiatedExecution("ContractSuccessJob");
        var successJob = Substitute.For<IBatchJob>();
        successJob.Name.Returns("ContractSuccessJob");
        _factory.Create("ContractSuccessJob").Returns(successJob);
        _lockRepo.TryAcquireLockAsync("ContractSuccessJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>()).Returns(1);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var successResult = await _orchestrator.RunAsync("ContractSuccessJob", RunMode.Manual, Guid.NewGuid(), "test-user");

        Assert.Equal(JobStatus.Completed, successResult.Status);

        // Failure path — the orchestrator never returns a Failed result; it throws instead.
        SetupInitiatedExecution("ContractFailureJob");
        var failingJob = Substitute.For<IBatchJob>();
        failingJob.Name.Returns("ContractFailureJob");
        failingJob.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Simulated failure")));
        _factory.Create("ContractFailureJob").Returns(failingJob);
        _lockRepo.TryAcquireLockAsync("ContractFailureJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>()).Returns(2);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.RunAsync("ContractFailureJob", RunMode.Manual, Guid.NewGuid(), "test-user"));
    }

    [Fact]
    public async Task RunAsync_WhenRequestedAtUtcProvided_PassesRequestedAtUtcToCreateRecord()
    {
        // Arrange
        SetupInitiatedExecution("TestJob");
        JobExecutionRecord? capturedCreateRecord = null;
        var requestedAtUtc = DateTime.UtcNow.AddMinutes(-1);

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("TestJob");

        _factory.Create("TestJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("TestJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(
                     Arg.Do<JobExecutionRecord>(r => capturedCreateRecord = r),
                     Arg.Any<CancellationToken>())
                 .Returns(42);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act
        var jobExecutionId = Guid.NewGuid();
        await _orchestrator.RunAsync("TestJob", RunMode.Manual, jobExecutionId, "test-user", requestedAtUtc);

        // Assert
        Assert.NotNull(capturedCreateRecord);
        Assert.Equal(requestedAtUtc, capturedCreateRecord!.RequestedAtUtc);
    }

    [Fact]
    public async Task RunAsync_WhenYearEndDataSetup_UsesSharedYearEndLockName()
    {
        // Arrange
        SetupApprovedExecution(BatchJobNames.YearEndDataSetup);
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns(BatchJobNames.YearEndDataSetup);

        _factory.Create(BatchJobNames.YearEndDataSetup).Returns(job);
        _lockRepo.TryAcquireLockAsync(BatchJobNames.YearEndLock, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(42);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act
        await _orchestrator.RunAsync(BatchJobNames.YearEndDataSetup, RunMode.Manual, Guid.NewGuid(), "test-user");

        // Assert
        await _lockRepo.Received(1).TryAcquireLockAsync(BatchJobNames.YearEndLock, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _lockRepo.Received(1).ReleaseLockAsync(BatchJobNames.YearEndLock, Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenYearEndCutover_UsesSharedYearEndLockName()
    {
        // Arrange
        SetupApprovedExecution(BatchJobNames.YearEndCutover);
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns(BatchJobNames.YearEndCutover);

        _factory.Create(BatchJobNames.YearEndCutover).Returns(job);
        _lockRepo.TryAcquireLockAsync(BatchJobNames.YearEndLock, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(42);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act
        await _orchestrator.RunAsync(BatchJobNames.YearEndCutover, RunMode.Manual, Guid.NewGuid(), "test-user");

        // Assert
        await _lockRepo.Received(1).TryAcquireLockAsync(BatchJobNames.YearEndLock, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _lockRepo.Received(1).ReleaseLockAsync(BatchJobNames.YearEndLock, Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(BatchJobNames.BulkTestRatesUpdate)]
    [InlineData(BatchJobNames.BulkStaffRatesUpdate)]
    [InlineData(BatchJobNames.BulkAnimalRatesUpdate)]
    public async Task RunAsync_WhenBulkRatesJob_UsesJobNameAsOwnDistinctLockName(string jobName)
    {
        // Arrange — Bulk Rates jobs must NOT share a lock (unlike YearEnd's shared YearEndLock):
        // FEC, Staff, and Animal requests may run concurrently; only two runs of the *same*
        // rate-type job must be mutually exclusive.
        SetupApprovedExecution(jobName);
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns(jobName);

        _factory.Create(jobName).Returns(job);
        _lockRepo.TryAcquireLockAsync(jobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(42);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act
        await _orchestrator.RunAsync(jobName, RunMode.Manual, Guid.NewGuid(), "test-user");

        // Assert — lock is keyed by the job's own name, not a shared constant
        await _lockRepo.Received(1).TryAcquireLockAsync(jobName, Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _lockRepo.Received(1).ReleaseLockAsync(jobName, Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenScheduledMabArchiveAndInitiatedMissing_CreatesInitiatedAndContinues()
    {
        // Arrange
        var jobExecutionId = Guid.NewGuid();
        var createdJobQueueId = Guid.NewGuid();
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("MABArchive");

        _execRepo.GetExecutionByJobExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JobExecutionRecord?>(null));
        _execRepo.CreateInitiatedRecordAsync(
                "MABArchive",
                jobExecutionId,
                "scheduler-user",
                Arg.Any<DateTime>(),
                RunMode.Scheduled,
                Arg.Any<CancellationToken>())
            .Returns(createdJobQueueId);
        _factory.Create("MABArchive").Returns(job);
        _lockRepo.TryAcquireLockAsync("MABArchive", createdJobQueueId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(11);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orchestrator.RunAsync("MABArchive", RunMode.Scheduled, jobExecutionId, "scheduler-user");

        // Assert
        await _execRepo.Received(1).CreateInitiatedRecordAsync(
            "MABArchive",
            jobExecutionId,
            "scheduler-user",
            Arg.Any<DateTime>(),
            RunMode.Scheduled,
            Arg.Any<CancellationToken>());
        await job.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
        Assert.Equal(JobStatus.Completed, result.Status);
        Assert.Equal(createdJobQueueId, result.JobQueueId);
    }

    [Fact]
    public async Task RunAsync_WhenManualAndInitiatedMissing_ThrowsAndDoesNotCreateInitiated()
    {
        // Arrange
        var jobExecutionId = Guid.NewGuid();
        _execRepo.GetExecutionByJobExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JobExecutionRecord?>(null));

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.RunAsync("ManualMissingInitiated", RunMode.Manual, jobExecutionId, "manual-user"));

        await _execRepo.DidNotReceive().CreateInitiatedRecordAsync(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<RunMode>(),
            Arg.Any<CancellationToken>());
        _factory.DidNotReceive().Create(Arg.Any<string>());
    }

    [Fact]
    public async Task RunAsync_WhenScheduledNonMabArchiveAndInitiatedMissing_ThrowsAndDoesNotCreateInitiated()
    {
        // Arrange
        var jobExecutionId = Guid.NewGuid();

        _execRepo.GetExecutionByJobExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<JobExecutionRecord?>(null));

        // Act / Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.RunAsync("RecreateSummary", RunMode.Scheduled, jobExecutionId, "scheduler-user"));

        // Assert
        await _execRepo.DidNotReceive().CreateInitiatedRecordAsync(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<DateTime>(),
            Arg.Any<RunMode>(),
            Arg.Any<CancellationToken>());
        _factory.DidNotReceive().Create(Arg.Any<string>());
    }

    // ─────────────────────────────────────────────────────────────
    // Lock already held — job must be skipped (not executed)
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_WhenLockNotAcquired_SkipsExecutionAndReturnsSkipped()
    {
        // Arrange
        SetupInitiatedExecution("TestJob");
        _lockRepo.TryAcquireLockAsync("TestJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(false);

        // Act / Assert
        var jobExecutionId = Guid.NewGuid();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.RunAsync("TestJob", RunMode.Scheduled, jobExecutionId, "test-user"));

        // Assert — job factory was never called
        _factory.DidNotReceive().Create(Arg.Any<string>());

        // Assert — no execution records written
        await _execRepo.DidNotReceive().CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>());
    }

    // ─────────────────────────────────────────────────────────────
    // Job throws — record must be written as Failed, lock released
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_WhenJobFails_WritesFailedRecordAndReleasesLock()
    {
        // Arrange
        SetupInitiatedExecution("FailingJob");
        var capturedUpdateStatus = new List<JobStatus>();
        var capturedErrorMessage = new List<string?>();

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("FailingJob");
        job.ExecuteAsync(Arg.Any<CancellationToken>())
           .Returns(Task.FromException(new InvalidOperationException("Simulated failure")));

        _factory.Create("FailingJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("FailingJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(99);
        _execRepo.UpdateExecutionRecordAsync(
                     Arg.Do<JobExecutionRecord>(r => { capturedUpdateStatus.Add(r.Status); capturedErrorMessage.Add(r.ErrorMessage); }),
                     Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act — orchestrator should re-throw
        var jobExecutionId = Guid.NewGuid();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.RunAsync("FailingJob", RunMode.Scheduled, jobExecutionId, "test-user"));

        // Assert — failure record written
        Assert.Single(capturedUpdateStatus);
        Assert.Equal(JobStatus.Failed, capturedUpdateStatus[0]);
        Assert.Single(capturedErrorMessage);
        Assert.Equal("Simulated failure", capturedErrorMessage[0]);

        // Assert — lock still released even after failure
        await _lockRepo.Received(1).ReleaseLockAsync("FailingJob", Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenFactoryCreateFails_WritesFailedRecordAndReleasesLock()
    {
        // Arrange
        SetupInitiatedExecution("MissingJob");
        var capturedUpdateStatus = new List<JobStatus>();
        var capturedErrorMessage = new List<string?>();

        _factory.Create("MissingJob")
            .Returns(_ => throw new InvalidOperationException("Job 'MissingJob' is not registered."));

        _lockRepo.TryAcquireLockAsync("MissingJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(100);
        _execRepo.UpdateExecutionRecordAsync(
                     Arg.Do<JobExecutionRecord>(r => { capturedUpdateStatus.Add(r.Status); capturedErrorMessage.Add(r.ErrorMessage); }),
                     Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act
        var jobExecutionId = Guid.NewGuid();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _orchestrator.RunAsync("MissingJob", RunMode.Manual, jobExecutionId, "test-user"));

        // Assert
        Assert.Single(capturedUpdateStatus);
        Assert.Equal(JobStatus.Failed, capturedUpdateStatus[0]);
        Assert.Single(capturedErrorMessage);
        Assert.Equal("Job 'MissingJob' is not registered.", capturedErrorMessage[0]);
        await _lockRepo.Received(1).ReleaseLockAsync("MissingJob", Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenInterruptedByCancellationToken_WritesFailedRecordAndReleasesLock()
    {
        // Arrange
        SetupInitiatedExecution("CancellableJob");
        var capturedUpdateStatus = new List<JobStatus>();

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("CancellableJob");
        job.ExecuteAsync(Arg.Any<CancellationToken>())
           .Returns(Task.FromException<Task>(new OperationCanceledException()));

        _factory.Create("CancellableJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("CancellableJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(77);
        _execRepo.UpdateExecutionRecordAsync(
                     Arg.Do<JobExecutionRecord>(r => capturedUpdateStatus.Add(r.Status)),
                     Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act — should re-throw as OperationCanceledException
        var jobExecutionId = Guid.NewGuid();
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _orchestrator.RunAsync("CancellableJob", RunMode.Manual, jobExecutionId, "test-user"));

        // Assert — interrupted execution is persisted as failed in the 4-state model
        Assert.Single(capturedUpdateStatus);
        Assert.Equal(JobStatus.Failed, capturedUpdateStatus[0]);

        // Assert — lock released
        await _lockRepo.Received(1).ReleaseLockAsync("CancellableJob", Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    // ─────────────────────────────────────────────────────────────
    // JobQueueId is generated per run and remains unique across executions
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_GeneratesUniqueJobQueueIdPerExecution()
    {
        // Arrange
        SetupInitiatedExecution("IdJob");
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("IdJob");

        _factory.Create("IdJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("IdJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(1);

        // Act
        var jobExecutionId1 = Guid.NewGuid();
        var jobExecutionId2 = Guid.NewGuid();
        var result1 = await _orchestrator.RunAsync("IdJob", RunMode.Manual, jobExecutionId1, "test-user");
        var result2 = await _orchestrator.RunAsync("IdJob", RunMode.Manual, jobExecutionId2, "test-user");

        // Assert — each run returns a generated non-empty and unique jobQueueId
        Assert.NotEqual(Guid.Empty, result1.JobQueueId);
        Assert.NotEqual(Guid.Empty, result2.JobQueueId);
        Assert.NotEqual(result2.JobQueueId, result1.JobQueueId);
    }

    // ─────────────────────────────────────────────────────────────
    // JobQueueId is passed consistently between lock and execution record
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_UsesConsistentJobQueueIdAcrossLockAndExecutionRecord()
    {
        // Arrange
        SetupInitiatedExecution("ConsistentJob");
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("ConsistentJob");

        _factory.Create("ConsistentJob").Returns(job);

        Guid? capturedLockJobQueueId = null;
        Guid? capturedRecordJobQueueId = null;
        Guid? capturedReleaseJobQueueId = null;

        _lockRepo.TryAcquireLockAsync("ConsistentJob", Arg.Do<Guid>(id => capturedLockJobQueueId = id),
                 Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(true);

        _execRepo.CreateExecutionRecordAsync(
                 Arg.Do<JobExecutionRecord>(r => capturedRecordJobQueueId = r.JobQueueId),
                 Arg.Any<CancellationToken>())
                 .Returns(5);

        _lockRepo.ReleaseLockAsync("ConsistentJob", Arg.Do<Guid>(id => capturedReleaseJobQueueId = id), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var jobExecutionId = Guid.NewGuid();
        var result = await _orchestrator.RunAsync("ConsistentJob", RunMode.Scheduled, jobExecutionId, "test-user");

        // Assert — JobQueueId correlation is preserved across lock acquire, execution record, lock release, and final result.
        Assert.NotNull(capturedLockJobQueueId);
        Assert.NotNull(capturedRecordJobQueueId);
        Assert.NotNull(capturedReleaseJobQueueId);
        Assert.Equal(capturedRecordJobQueueId, capturedLockJobQueueId);
        Assert.Equal(capturedRecordJobQueueId, capturedReleaseJobQueueId);
        Assert.Equal(capturedRecordJobQueueId, result.JobQueueId);
    }

    // ─────────────────────────────────────────────────────────────
    // Concurrent lock contention — only first caller wins
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunAsync_WhenTwoConcurrentCallsForSameJob_OnlyOneExecutesAndOtherIsSkipped()
    {
        // Arrange � first call acquires lock, second call gets false (DB unique constraint)
        SetupInitiatedExecution("ConcurrentJob");
        var lockCallCount = 0;
        _lockRepo.TryAcquireLockAsync("ConcurrentJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                 .Returns(_ => ++lockCallCount == 1);  // first true, subsequent false

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("ConcurrentJob");
        _factory.Create("ConcurrentJob").Returns(job);

        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(1);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
                 .Returns(Task.CompletedTask);

        // Act — simulate two concurrent runs
        var task1 = _orchestrator.RunAsync("ConcurrentJob", RunMode.Scheduled, Guid.NewGuid(), "test-user");
        var task2 = _orchestrator.RunAsync("ConcurrentJob", RunMode.Scheduled, Guid.NewGuid(), "test-user");
        try
        {
            await Task.WhenAll(task1, task2);
        }
        catch (InvalidOperationException)
        {
            // Expected: one concurrent call fails lock acquisition.
        }

        // Assert — exactly one completed and one failed due to lock contention
        Assert.Equal(1, new[] { task1, task2 }.Count(t => t.Status == TaskStatus.RanToCompletion));
        Assert.Equal(1, new[] { task1, task2 }.Count(t => t.IsFaulted));
        Assert.Contains(new[] { task1, task2 }.First(t => t.IsFaulted).Exception!.Flatten().InnerExceptions,
            ex => ex is InvalidOperationException);

        // Assert — job executed exactly once
        await job.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenFirstAttemptFailsAndSecondSucceeds_RetriesAndCompletes()
    {
        // Arrange
        var retrySettings = Options.Create(new BatchJobSettings
        {
            JobTimeout = 3600,
            RetryAttempts = 1,
            RetryDelaySeconds = 0
        });

        var orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
            _correlationService,
            retrySettings,
            _configuration,
            NullLogger<JobOrchestrator>.Instance);

        SetupInitiatedExecution("RetrySuccessJob");
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("RetrySuccessJob");
        job.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromException(new TimeoutException("Transient failure")),
                Task.CompletedTask);

        _factory.Create("RetrySuccessJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("RetrySuccessJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(200);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await orchestrator.RunAsync("RetrySuccessJob", RunMode.Manual, Guid.NewGuid(), "test-user");

        // Assert
        Assert.Equal(JobStatus.Completed, result.Status);
        await job.Received(2).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenRetriesExhausted_ThrowsAfterConfiguredAttempts()
    {
        // Arrange
        var retrySettings = Options.Create(new BatchJobSettings
        {
            JobTimeout = 3600,
            RetryAttempts = 2,
            RetryDelaySeconds = 0
        });

        var orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
            _correlationService,
            retrySettings,
            _configuration,
            NullLogger<JobOrchestrator>.Instance);

        SetupInitiatedExecution("RetryFailJob");
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("RetryFailJob");
        // Use explicit transient infrastructure exception so all attempts run.
        job.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new TimeoutException("Transient persistent failure")));

        _factory.Create("RetryFailJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("RetryFailJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(201);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await Assert.ThrowsAsync<TimeoutException>(
            () => orchestrator.RunAsync("RetryFailJob", RunMode.Scheduled, Guid.NewGuid(), "test-user"));

        // Assert (first attempt + 2 retries)
        await job.Received(3).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    // ─────────────────────────────────────────────────────────────
    // Non-retryable exceptions — should fail on first attempt even when retries configured
    // ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(NotSupportedException))]
    public async Task RunAsync_WhenNonRetryableExceptionThrown_FailsImmediatelyWithoutRetry(Type exceptionType)
    {
        // Arrange — 2 retries configured, but non-retryable exception must stop on attempt 1
        var retrySettings = Options.Create(new BatchJobSettings
        {
            JobTimeout = 3600,
            RetryAttempts = 2,
            RetryDelaySeconds = 0
        });

        var orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
            _correlationService,
            retrySettings,
            _configuration,
            NullLogger<JobOrchestrator>.Instance);

        SetupInitiatedExecution("NonRetryableJob");
        var nonRetryableEx = (Exception)Activator.CreateInstance(exceptionType, "Non-retryable")!;
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("NonRetryableJob");
        job.ExecuteAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException(nonRetryableEx));

        _factory.Create("NonRetryableJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("NonRetryableJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(300);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await Assert.ThrowsAsync(exceptionType,
            () => orchestrator.RunAsync("NonRetryableJob", RunMode.Manual, Guid.NewGuid(), "test-user"));

        // Assert — only one attempt, no retries
        await job.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task IsRetryable_ClassifiesExceptionTypesCorrectly()
    {
        Assert.False(JobOrchestrator.IsRetryable(new OperationCanceledException()));
        Assert.False(JobOrchestrator.IsRetryable(new ArgumentException()));
        Assert.False(JobOrchestrator.IsRetryable(new InvalidOperationException()));
        Assert.False(JobOrchestrator.IsRetryable(new NotSupportedException()));
        Assert.False(JobOrchestrator.IsRetryable(new NotImplementedException()));
        Assert.False(JobOrchestrator.IsRetryable(new Exception("generic transient")));
        Assert.True(JobOrchestrator.IsRetryable(new TimeoutException()));
    }

    [Fact]
    public async Task RunAsync_WhenRuntimeTimeoutExceeded_FailsWithTimeoutAndNoRetry()
    {
        // Arrange
        var timeoutSettings = Options.Create(new BatchJobSettings
        {
            JobTimeout = 1,
            RetryAttempts = 2,
            RetryDelaySeconds = 0
        });

        var orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
            _correlationService,
            timeoutSettings,
            _configuration,
            NullLogger<JobOrchestrator>.Instance);

        SetupInitiatedExecution("RuntimeTimeoutJob");
        JobStatus? updatedStatus = null;
        string? updatedErrorMessage = null;

        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("RuntimeTimeoutJob");
        job.MaxExecutionSeconds.Returns(1);
        job.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(ci => Task.Delay(TimeSpan.FromSeconds(3), ci.Arg<CancellationToken>()));

        _factory.Create("RuntimeTimeoutJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("RuntimeTimeoutJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(999);
        _execRepo.UpdateExecutionRecordAsync(
            Arg.Do<JobExecutionRecord>(r =>
            {
                updatedStatus = r.Status;
                updatedErrorMessage = r.ErrorMessage;
            }),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await Assert.ThrowsAsync<TimeoutException>(() =>
            orchestrator.RunAsync("RuntimeTimeoutJob", RunMode.Manual, Guid.NewGuid(), "test-user"));

        // Assert
        Assert.Equal(JobStatus.Failed, updatedStatus);
        Assert.Contains("runtime timeout", updatedErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        await job.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenJobTimeoutOverrideExists_OverrideTakesPrecedence()
    {
        // Arrange
        var timeoutSettings = Options.Create(new BatchJobSettings
        {
            JobTimeout = 300,
            JobTimeoutOverridesSeconds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["OverrideTimeoutJob"] = 1
            },
            RetryAttempts = 0,
            RetryDelaySeconds = 0
        });

        var orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
            _correlationService,
            timeoutSettings,
            _configuration,
            NullLogger<JobOrchestrator>.Instance);

        SetupInitiatedExecution("OverrideTimeoutJob");
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("OverrideTimeoutJob");
        job.MaxExecutionSeconds.Returns(1200);
        job.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(ci => Task.Delay(TimeSpan.FromSeconds(3), ci.Arg<CancellationToken>()));

        _factory.Create("OverrideTimeoutJob").Returns(job);
        _lockRepo.TryAcquireLockAsync("OverrideTimeoutJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(1001);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await Assert.ThrowsAsync<TimeoutException>(() =>
            orchestrator.RunAsync("OverrideTimeoutJob", RunMode.Manual, Guid.NewGuid(), "test-user"));

        // Assert
        await job.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_UsesConfiguredLockTimeoutForLockAcquisition()
    {
        // Arrange
        var timeoutSettings = Options.Create(new BatchJobSettings { LockTimeoutSeconds = 1234 });
        var orchestrator = new JobOrchestrator(
            _factory,
            _lockRepo,
            _execRepo,
            _correlationService,
            timeoutSettings,
            _configuration,
            NullLogger<JobOrchestrator>.Instance);

        SetupInitiatedExecution("TimeoutJob");
        var job = Substitute.For<IBatchJob>();
        job.Name.Returns("TimeoutJob");
        _factory.Create("TimeoutJob").Returns(job);

        _lockRepo.TryAcquireLockAsync("TimeoutJob", Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _execRepo.CreateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(11);
        _execRepo.UpdateExecutionRecordAsync(Arg.Any<JobExecutionRecord>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await orchestrator.RunAsync("TimeoutJob", RunMode.Scheduled, Guid.NewGuid(), "test-user");

        // Assert
        await _lockRepo.Received(1).TryAcquireLockAsync(
            "TimeoutJob",
            Arg.Any<Guid>(),
            1234,
            Arg.Any<CancellationToken>());
    }

    // --- Helpers ----------------------------------------------------------------

    /// <summary>
    /// Configures GetExecutionByJobExecutionIdAsync to return an Initiated record
    /// whose JobName matches <paramref name="jobName"/>. Each test that calls RunAsync
    /// must invoke this (or SetupApprovedExecution / a custom override) exactly once.
    /// </summary>
    private void SetupInitiatedExecution(string jobName)
    {
        _execRepo.GetExecutionByJobExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(args => Task.FromResult<JobExecutionRecord?>(new JobExecutionRecord
            {
                ExecutionId = 1,
                JobExecutionId = (Guid)args[0],
                JobQueueId = Guid.NewGuid(),
                JobName = jobName,
                UserId = "test-user",
                JobType = JobType.Unknown,
                RunMode = RunMode.Manual,
                Status = JobStatus.Initiated,
                StartedAt = DateTime.UtcNow,
                RetryAttempts = 0
            }));
    }

    /// <summary>
    /// Configures GetExecutionByJobExecutionIdAsync to return an Approved record for
    /// approval-based jobs. GetApprovalMetadataAsync returns null by default (NSubstitute),
    /// which the orchestrator handles gracefully per the CR025 provisional guard.
    /// </summary>
    private void SetupApprovedExecution(string jobName)
    {
        _execRepo.GetExecutionByJobExecutionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(args => Task.FromResult<JobExecutionRecord?>(new JobExecutionRecord
            {
                ExecutionId = 1,
                JobExecutionId = (Guid)args[0],
                JobQueueId = Guid.NewGuid(),
                JobName = jobName,
                UserId = "test-user",
                JobType = JobType.Unknown,
                RunMode = RunMode.Manual,
                Status = JobStatus.Approved,
                StartedAt = DateTime.UtcNow,
                RetryAttempts = 0
            }));
    }
}




