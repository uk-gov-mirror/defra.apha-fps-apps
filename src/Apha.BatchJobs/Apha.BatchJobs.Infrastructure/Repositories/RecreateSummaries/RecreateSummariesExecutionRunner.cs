using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class RecreateSummariesExecutionRunner : IRecreateSummariesExecutionRunner
{
    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly ILogger<RecreateSummariesExecutionRunner> _logger;

    // 2 minutes: steps exceeding this are logged as slow for operational investigation.
    private const int SlowStepThresholdMs = 120_000;

    public RecreateSummariesExecutionRunner(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        ILogger<RecreateSummariesExecutionRunner> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<StepResult>> ExecuteAsync(
        string jobExecutionId,
        int month,
        int year,
        string triggeredBy,
        IRecreateSummariesStepCatalog stepCatalog,
        CancellationToken cancellationToken)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        IReadOnlyList<StepResult>? completedResults = null;
        var executionStrategy = context.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            var results = new List<StepResult>();

            context.ChangeTracker.Clear();

            var npgsqlConnection = (NpgsqlConnection)context.Database.GetDbConnection();
            if (npgsqlConnection.State != System.Data.ConnectionState.Open)
                await context.Database.OpenConnectionAsync(cancellationToken);

            var executionContext = new RecreateSummariesExecutionContext(context, npgsqlConnection, year);

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                _logger.LogInformation("[{JobExecutionId}] RecreateSummaries implementation: DotNetLinq", jobExecutionId);

                // --- Steps 1–14 (mandatory, ordered) ---
                var mandatorySteps = stepCatalog.BuildMandatorySteps(month, year, triggeredBy);

                foreach (var step in mandatorySteps)
                {
                    results.Add(await ExecuteStepAsync(step, executionContext, jobExecutionId, cancellationToken));
                }

                // --- Period-lock check (Phase 6) ---
                var periodLocked = await GetPeriodLockedAsync(context, month, year, cancellationToken);

                _logger.LogInformation(
                    "[{JobExecutionId}] Period lock check | Month={Month} | Year={Year} | PeriodLocked={PeriodLocked}",
                    jobExecutionId, month, year, periodLocked);

                if (periodLocked == 0)
                {
                    // Steps 15–17: conditional refresh when period is not locked
                    var refreshSteps = stepCatalog.BuildRefreshSteps(month);

                    foreach (var step in refreshSteps)
                    {
                        results.Add(await ExecuteStepAsync(step, executionContext, jobExecutionId, cancellationToken));
                    }
                }
                else
                {
                    // Period is locked — skip refresh steps, record as Skipped
                    foreach (var stepName in stepCatalog.BuildRefreshSteps(month).Select(step => step.StepName))
                    {
                        var skipped = new StepResult(stepName, 0, DateTime.UtcNow, DateTime.UtcNow,
                            StepStatus.Skipped, "Period is locked");
                        results.Add(skipped);
                        _logger.LogInformation("[{JobExecutionId}] Step {StepName} skipped - period is locked.", jobExecutionId, stepName);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
                context.ChangeTracker.Clear();

                _logger.LogInformation("[{JobExecutionId}] Transaction committed. All steps completed.", jobExecutionId);
                completedResults = results;
            }
            catch (Exception)
            {
                await SafeRollbackAsync(transaction, jobExecutionId);
                context.ChangeTracker.Clear();
                throw;
            }
        });

        return completedResults ?? Array.Empty<StepResult>();
    }

    private async Task<StepResult> ExecuteStepAsync(
        IRecreateSummariesExecutionStep step,
        IRecreateSummariesExecutionContext executionContext,
        string jobExecutionId,
        CancellationToken cancellationToken)
    {
        using var stepScope = _logger.BeginScope(new Dictionary<string, object?> { ["StepName"] = step.StepName });
        _logger.LogInformation("[{JobExecutionId}] Executing step: {StepName}", jobExecutionId, step.StepName);

        var result = await step.ExecuteAsync(executionContext, cancellationToken);

        var stepDurationMs = (int)(result.EndTime - result.StartTime).TotalMilliseconds;
        _logger.LogInformation(
            "[{JobExecutionId}] Step {StepName} -> {Status} | RowsAffected={Rows} | Duration={Ms}ms",
            jobExecutionId, result.StepName, result.Status, result.RowsAffected,
            stepDurationMs);

        if (stepDurationMs > SlowStepThresholdMs)
        {
            _logger.LogInformation(
                "[{JobExecutionId}] SLOW STEP DETECTED | StepName={StepName} | Duration={Ms}ms | RowsAffected={Rows}",
                jobExecutionId, result.StepName, stepDurationMs, result.RowsAffected);
        }

        if (result.Status == StepStatus.Failed)
        {
            _logger.LogError(
                "[{JobExecutionId}] Step {StepName} failed: {Error}",
                jobExecutionId, result.StepName, result.ErrorMessage);

            throw new RecreateSummariesStepException(result.StepName, result.ErrorMessage);
        }

        return result;
    }

    private async Task<int> GetPeriodLockedAsync(
        BatchJobsDbContext context,
        int month,
        int year,
        CancellationToken cancellationToken)
    {
        var periodLocked = await context.RsTblPeriod
            .AsNoTracking()
            .Where(p => p.EndPeriod == month && p.FpsYear == year)
            .Select(p => p.PeriodLocked)
            .FirstOrDefaultAsync(cancellationToken);

        return periodLocked ?? 1;
    }

    private async Task SafeRollbackAsync(IDbContextTransaction transaction, string jobExecutionId)
    {
        try
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }
        catch (Exception rollbackEx)
        {
            _logger.LogError(rollbackEx, "[{JobExecutionId}] Rollback failed.", jobExecutionId);
            throw new InvalidOperationException($"Rollback failed for RecreateSummaries job ({jobExecutionId}).", rollbackEx);
        }
    }
}
