using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class RecreateSummariesExecutionStepBaseTests
{
    private static RecreateSummariesExecutionContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new BatchJobsDbContext(options);
        // NpgsqlConnection is unused by the step base; supply a disconnected instance.
        return new RecreateSummariesExecutionContext(db, new NpgsqlConnection(), 2026);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_PropagatesOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var step = new CancellingStep();
        var context = BuildContext();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => step.ExecuteAsync(context, cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_DoesNotReturnFailedStepResult()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var step = new CancellingStep();
        var context = BuildContext();

        StepResult? result = null;
        try
        {
            result = await step.ExecuteAsync(context, cts.Token);
        }
        catch (OperationCanceledException) { }

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExceptionThrown_ReturnsFailedStepResult()
    {
        var step = new ThrowingStep();
        var context = BuildContext();

        var result = await step.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(StepStatus.Failed, result.Status);
        Assert.Contains("simulated failure", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSuccessful_ReturnsSuccessStepResult()
    {
        var step = new SucceedingStep();
        var context = BuildContext();

        var result = await step.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(StepStatus.Success, result.Status);
        Assert.Equal(3, result.RowsAffected);
    }

    // Fake step subclasses â€” defined here to keep tests self-contained

    private sealed class CancellingStep : RecreateSummariesExecutionStepBase
    {
        public override string StepName => "CancellingStep";

        protected override Task<int> ExecuteCoreAsync(
            RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(0);
        }
    }

    private sealed class ThrowingStep : RecreateSummariesExecutionStepBase
    {
        public override string StepName => "ThrowingStep";

        protected override Task<int> ExecuteCoreAsync(
            RecreateSummariesExecutionContext context, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("simulated failure");
    }

    private sealed class SucceedingStep : RecreateSummariesExecutionStepBase
    {
        public override string StepName => "SucceedingStep";

        protected override Task<int> ExecuteCoreAsync(
            RecreateSummariesExecutionContext context, CancellationToken cancellationToken) =>
            Task.FromResult(3);
    }
}
