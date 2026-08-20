using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Resets project financial fields for target-year project rows using strict year scoping.
/// </summary>
public sealed class ProjectFinancialResetStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyDictionary<string, string> ResetRules =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["transferincome"] = "0",
            ["custincome"] = "0",
            ["wip_eoy"] = "0",
            ["feccost"] = "0",
            ["profit"] = "0",
            ["budget_cvl"] = "0",
            ["carryover"] = "0",
            ["wip_limit"] = "NULL",
            ["wip_current"] = "NULL",
            ["pvsincome"] = "NULL",
            ["plancaseworkdebit"] = "NULL"
        };

    private static readonly IReadOnlyList<ResetTarget> ResetTargets =
    [
        new("fps", "tlkpproject", "fpsyear"),
        new("mabarchive", "my_tlkpproject", "year")
    ];

    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<ProjectFinancialResetStep> _logger;

    public ProjectFinancialResetStep(
        IYearEndDataSetupRepository repository,
        ILogger<ProjectFinancialResetStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ProjectFinancialResetStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before project financial reset.");
        }

        foreach (var target in ResetTargets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await _repository.TableExistsAsync(target.Schema, target.Table, cancellationToken))
            {
                _logger.LogWarning(
                    "YearEnd project reset skipped missing table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    context.CorrelationId,
                    target.Schema,
                    target.Table);
                continue;
            }

            if (!await _repository.ColumnExistsAsync(target.Schema, target.Table, target.YearColumn, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Table {target.Schema}.{target.Table} does not contain required year column {target.YearColumn} for safe reset.");
            }

            var updated = await _repository.ResetFieldsByYearAsync(
                target.Schema,
                target.Table,
                target.YearColumn,
                ResetRules,
                context.TargetFpsYear.Value,
                cancellationToken);

            _logger.LogInformation(
                "YearEnd project financial reset completed | CorrelationId={CorrelationId} | Table={Schema}.{Table} | TargetYear={TargetYear} | UpdatedRows={UpdatedRows}",
                context.CorrelationId,
                target.Schema,
                target.Table,
                context.TargetFpsYear,
                updated);
        }
    }

    private sealed record ResetTarget(string Schema, string Table, string YearColumn);
}