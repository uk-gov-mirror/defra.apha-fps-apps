using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Copies vetted fps schema tables from current FPS year into target FPS year using strict year scoping.
/// </summary>
public sealed class CopyFpsYearScopedTablesStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyList<string> CopyTables =
    [
        "tlkpproject",
        "tblstaffjob",
        "tlkptestreqmt",
        "tblanimalreq",
        "tbladditionalcosts"
    ];

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ResetRulesByTable =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["tlkpproject"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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
            },
            ["tblstaffjob"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["plannedhours"] = "0"
            },
            ["tlkptestreqmt"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["norequired"] = "0"
            },
            ["tblanimalreq"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["numberofanimals"] = "0",
                ["numberofdays"] = "0"
            },
            ["tbladditionalcosts"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["itemcost"] = "0"
            }
        };

    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<CopyFpsYearScopedTablesStep> _logger;

    public CopyFpsYearScopedTablesStep(
        IYearEndDataSetupRepository repository,
        ILogger<CopyFpsYearScopedTablesStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "CopyFpsYearScopedTablesStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before FPS year-scoped copy.");
        }

        foreach (var table in CopyTables)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await _repository.TableExistsAsync("fps", table, cancellationToken))
            {
                _logger.LogWarning(
                    "YearEnd copy skipped missing table | CorrelationId={CorrelationId} | Table=fps.{Table}",
                    context.CorrelationId,
                    table);
                continue;
            }

            if (!await _repository.ColumnExistsAsync("fps", table, "fpsyear", cancellationToken))
            {
                throw new InvalidOperationException($"Table fps.{table} does not contain fpsyear and cannot be copied safely.");
            }

            var targetRows = await _repository.CountRowsByYearAsync("fps", table, "fpsyear", context.TargetFpsYear.Value, cancellationToken);
            if (targetRows > 0)
            {
                throw new InvalidOperationException(
                    $"Table fps.{table} already contains {targetRows} rows for target year {context.TargetFpsYear.Value}. Cleanup is required before Year End copy.");
            }

            var copied = await _repository.CopyFpsYearScopedTableAsync(table, context.CurrentFpsYear.Value, context.TargetFpsYear.Value, cancellationToken);

            _logger.LogInformation(
                "YearEnd table copy completed | CorrelationId={CorrelationId} | Table=fps.{Table} | SourceYear={SourceYear} | TargetYear={TargetYear} | CopiedRows={CopiedRows}",
                context.CorrelationId,
                table,
                context.CurrentFpsYear,
                context.TargetFpsYear,
                copied);
        }
    }
}
