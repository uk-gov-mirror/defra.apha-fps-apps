using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Removes target-year staff-job rows that map to inactive employees when inactive markers are available.
/// </summary>
public sealed class InactiveEmployeeCleanupStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyList<CleanupTarget> Targets =
    [
        new("fps", "tblstaffjob", "fpsyear", "staffid", "tblwgemployee", "pactid"),
        new("mabarchive", "my_tblstaffjob", "year", "staffid", "my_tblwgemployee", "pactid")
    ];

    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<InactiveEmployeeCleanupStep> _logger;

    public InactiveEmployeeCleanupStep(
        IYearEndDataSetupRepository repository,
        ILogger<InactiveEmployeeCleanupStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "InactiveEmployeeCleanupStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before inactive employee cleanup.");
        }

        var totalDeleted = 0;

        foreach (var target in Targets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await _repository.TableExistsAsync(target.Schema, target.JobTable, cancellationToken))
            {
                _logger.LogWarning(
                    "YearEnd inactive cleanup skipped missing job table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    context.CorrelationId,
                    target.Schema,
                    target.JobTable);
                continue;
            }

            if (!await _repository.TableExistsAsync(target.Schema, target.EmployeeTable, cancellationToken))
            {
                _logger.LogWarning(
                    "YearEnd inactive cleanup skipped missing employee table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    context.CorrelationId,
                    target.Schema,
                    target.EmployeeTable);
                continue;
            }

            var hasYearColumn = await _repository.ColumnExistsAsync(target.Schema, target.JobTable, target.YearColumn, cancellationToken);
            var hasStaffColumn = await _repository.ColumnExistsAsync(target.Schema, target.JobTable, target.JobStaffColumn, cancellationToken);
            var hasEmployeeStaffColumn = await _repository.ColumnExistsAsync(target.Schema, target.EmployeeTable, target.EmployeeStaffColumn, cancellationToken);

            if (!hasYearColumn || !hasStaffColumn || !hasEmployeeStaffColumn)
            {
                throw new InvalidOperationException(
                    $"Inactive cleanup cannot run safely for {target.Schema}.{target.JobTable}; required columns are missing.");
            }

            var deleted = await _repository.DeleteInactiveEmployeeJobRowsAsync(
                target.Schema,
                target.JobTable,
                target.YearColumn,
                target.JobStaffColumn,
                target.EmployeeTable,
                target.EmployeeStaffColumn,
                context.TargetFpsYear.Value,
                cancellationToken);

            totalDeleted += deleted;

            _logger.LogInformation(
                "YearEnd inactive cleanup completed | CorrelationId={CorrelationId} | Table={Schema}.{Table} | TargetYear={TargetYear} | DeletedRows={DeletedRows}",
                context.CorrelationId,
                target.Schema,
                target.JobTable,
                context.TargetFpsYear,
                deleted);
        }

        _logger.LogInformation(
            "YearEnd inactive employee cleanup completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | DeletedRows={DeletedRows}",
            context.CorrelationId,
            context.TargetFpsYear,
            totalDeleted);
    }

    private sealed record CleanupTarget(
        string Schema,
        string JobTable,
        string YearColumn,
        string JobStaffColumn,
        string EmployeeTable,
        string EmployeeStaffColumn);
}
