using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Clears configured target-year rows from tables that must start empty after setup.
/// </summary>
public sealed class TargetYearEmptyTablesStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyList<string> CandidateTables =
    [
        "additionalcosts_log",
        "animalreq_log",
        "fpsyeartotals",
        "mo_log",
        "monthlyoutput",
        "monthlytime",
        "mt_log",
        "proj_invoice",
        "proj_subcontract",
        "project_log",
        "projectmonth",
        "projectmonthfinal",
        "recreatesummaries_log",
        "staffjob_log",
        "tblbid",
        "tblpurchase",
        "tblsurvff_fees",
        "tblsurvff_submissions",
        "tbltestreqbaseline",
        "testreq_log",
        "timecostcalcs"
    ];

    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<TargetYearEmptyTablesStep> _logger;

    public TargetYearEmptyTablesStep(
        IYearEndDataSetupRepository repository,
        ILogger<TargetYearEmptyTablesStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "TargetYearEmptyTablesStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before target-year empty-table cleanup.");
        }

        var totalDeleted = 0;

        foreach (var table in CandidateTables)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await _repository.TableExistsAsync("fps", table, cancellationToken))
            {
                continue;
            }

            var yearColumn = await _repository.ResolveYearColumnAsync("fps", table, cancellationToken);
            if (yearColumn is null)
            {
                _logger.LogWarning(
                    "YearEnd empty-table cleanup skipped non-year-scoped table | CorrelationId={CorrelationId} | Table=fps.{Table}",
                    context.CorrelationId,
                    table);
                continue;
            }

            var deleted = await _repository.DeleteRowsByYearAsync("fps", table, yearColumn, context.TargetFpsYear.Value, cancellationToken);
            totalDeleted += deleted;

            _logger.LogInformation(
                "YearEnd empty-table cleanup completed | CorrelationId={CorrelationId} | Table=fps.{Table} | YearColumn={YearColumn} | TargetYear={TargetYear} | DeletedRows={DeletedRows}",
                context.CorrelationId,
                table,
                yearColumn,
                context.TargetFpsYear,
                deleted);
        }

        _logger.LogInformation(
            "YearEnd target-year empty-table cleanup completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | DeletedRows={DeletedRows}",
            context.CorrelationId,
            context.TargetFpsYear,
            totalDeleted);
    }


}
