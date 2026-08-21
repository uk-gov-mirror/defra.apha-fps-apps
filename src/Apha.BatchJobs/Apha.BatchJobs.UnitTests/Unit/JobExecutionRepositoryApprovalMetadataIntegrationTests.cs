using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed integration tests for <see cref="JobExecutionRepository.GetApprovalMetadataAsync"/>.
/// CR025 (fps.job_queue approval metadata columns) is assumed universally deployed — there is no
/// compatibility path for a database that predates it.
/// </summary>
[Trait("Category", "Integration")]
public sealed class JobExecutionRepositoryApprovalMetadataIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Timeout=30";
    private readonly string _connectionString;
    private string? _skipReason;
    private bool _yearEndApprovedCatalogAvailable;

    public JobExecutionRepositoryApprovalMetadataIntegrationTests()
    {
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var context = CreateDbContext();
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                _skipReason = "Integration DB unavailable.";
                return;
            }

            _yearEndApprovedCatalogAvailable = await context.Database
                .SqlQuery<int>($@"
                    SELECT COUNT(*)::int AS ""Value""
                    FROM fps.job_master m
                    JOIN fps.job_status s ON s.jobid = m.jobid
                    WHERE m.jobname = {BatchJobNames.YearEndDataSetup}
                      AND s.status = 'Approved'")
                .SingleAsync() > 0;
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task GetApprovalMetadataAsync_WhenRowNotFound_ReturnsNull()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var repository = CreateRepository();

        var metadata = await repository.GetApprovalMetadataAsync(Guid.NewGuid());

        Assert.Null(metadata);
    }

    [SkippableFact]
    public async Task GetApprovalMetadataAsync_WhenRowExists_ReturnsSeededMetadata()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        Skip.IfNot(
            _yearEndApprovedCatalogAvailable,
            $"job_master/job_status seed for '{BatchJobNames.YearEndDataSetup}' + 'Approved' is not yet provisioned on this database.");

        var jobExecutionId = Guid.NewGuid();
        var jobQueueId = Guid.NewGuid();

        await using (var context = CreateDbContext())
        {
            var jobId = await context.Database
                .SqlQuery<int>($@"
                    SELECT jobid AS ""Value"" FROM fps.job_master WHERE jobname = {BatchJobNames.YearEndDataSetup}")
                .SingleAsync();

            var statusId = await context.Database
                .SqlQuery<int>($@"
                    SELECT statusid AS ""Value"" FROM fps.job_status WHERE jobid = {jobId} AND status = 'Approved'")
                .SingleAsync();

            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO fps.job_queue
                    (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime,
                     approved_by, approved_at_utc)
                VALUES
                    ({jobQueueId}, {jobExecutionId}, {jobId}, {statusId}, 'integration-test-requester', NOW(), NOW(),
                     'integration-test-approver', NOW());");
        }

        try
        {
            var repository = CreateRepository();

            var metadata = await repository.GetApprovalMetadataAsync(jobExecutionId);

            Assert.NotNull(metadata);
            Assert.Equal("integration-test-approver", metadata!.ApprovedBy);
            Assert.NotNull(metadata.ApprovedAtUtc);
            Assert.Null(metadata.RejectedBy);
        }
        finally
        {
            await using var context = CreateDbContext();
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                DELETE FROM fps.job_queue WHERE jobqueueid = {jobQueueId};");
        }
    }

    private JobExecutionRepository CreateRepository() => new(CreateDbContext(), NullLogger<JobExecutionRepository>.Instance);

    private BatchJobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        return new BatchJobsDbContext(options);
    }

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);
}
