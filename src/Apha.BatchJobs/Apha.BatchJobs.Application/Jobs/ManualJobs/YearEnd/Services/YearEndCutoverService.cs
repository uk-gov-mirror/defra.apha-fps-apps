using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Service-layer entry point for Year End Cutover.
/// Closes the current FPS year and activates the target FPS year in a single transaction.
/// </summary>
public sealed class YearEndCutoverService : IYearEndCutoverService
{
    private readonly IYearEndCutoverRepository _cutoverRepository;
    private readonly IJobExecutionRepository _executionRepository;
    private readonly ILogger<YearEndCutoverService> _logger;

    public YearEndCutoverService(
        IYearEndCutoverRepository cutoverRepository,
        IJobExecutionRepository executionRepository,
        ILogger<YearEndCutoverService> logger)
    {
        _cutoverRepository = cutoverRepository ?? throw new ArgumentNullException(nameof(cutoverRepository));
        _executionRepository = executionRepository ?? throw new ArgumentNullException(nameof(executionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End Cutover requires currentFpsYear in BATCH_JOB_PARAMETERS_JSON.");
        }

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End Cutover requires targetFpsYear in BATCH_JOB_PARAMETERS_JSON.");
        }

        var currentYear = context.CurrentFpsYear.Value;
        var targetYear = context.TargetFpsYear.Value;

        if (targetYear <= currentYear)
        {
            throw new InvalidOperationException("targetFpsYear must be greater than currentFpsYear for Year End Cutover.");
        }

        _logger.LogInformation(
            "YearEndCutover service started | CorrelationId={CorrelationId} | TargetFpsYear={TargetFpsYear} | CurrentFpsYear={CurrentFpsYear}",
            context.CorrelationId,
            targetYear,
            currentYear);

        var latestDataSetupExecution = await _executionRepository.GetLastExecutionByFpsYearAsync(
            BatchJobNames.YearEndDataSetup,
            targetYear,
            cancellationToken);

        if (latestDataSetupExecution is null || latestDataSetupExecution.Status != JobStatus.Completed)
        {
            var actualStatus = latestDataSetupExecution?.Status.ToString() ?? "None";
            throw new InvalidOperationException(
                $"Year End Cutover requires the latest {BatchJobNames.YearEndDataSetup} execution for target year {targetYear} " +
                $"to be Completed, but found '{actualStatus}'.");
        }

        await _cutoverRepository.ExecuteCutoverAsync(currentYear, targetYear, cancellationToken);

        _logger.LogInformation(
            "YearEndCutover completed | CorrelationId={CorrelationId} | ClosedYear={ClosedYear} | ActivatedYear={ActivatedYear}",
            context.CorrelationId,
            currentYear,
            targetYear);
    }
}
