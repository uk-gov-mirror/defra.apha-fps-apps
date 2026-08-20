using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Copies vetted mabarchive schema tables from current FPS year into target FPS year using strict year scoping.
/// </summary>
public sealed class CopyMabArchiveYearScopedTablesStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyList<string> CopyTables =
    [
        "my_tlkpproject",
        "my_tblstaffjob",
        "my_tlkptestreqmt",
        "my_tblanimalreq",
        "my_tbladditionalcosts"
    ];

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> ResetRulesByTable =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["my_tlkpproject"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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
            ["my_tblstaffjob"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["plannedhours"] = "0"
            },
            ["my_tlkptestreqmt"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["norequired"] = "0"
            },
            ["my_tblanimalreq"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["numberofanimals"] = "0",
                ["numberofdays"] = "0"
            },
            ["my_tbladditionalcosts"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["itemcost"] = "0"
            }
        };

    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<CopyMabArchiveYearScopedTablesStep> _logger;

    public CopyMabArchiveYearScopedTablesStep(
        IYearEndDataSetupRepository repository,
        ILogger<CopyMabArchiveYearScopedTablesStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "CopyMabArchiveYearScopedTablesStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before MABArchive year-scoped copy.");
        }

        foreach (var table in CopyTables)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await _repository.TableExistsAsync("mabarchive", table, cancellationToken))
            {
                _logger.LogWarning(
                    "YearEnd copy skipped missing table | CorrelationId={CorrelationId} | Table=mabarchive.{Table}",
                    context.CorrelationId,
                    table);
                continue;
            }

            if (!await _repository.ColumnExistsAsync("mabarchive", table, "year", cancellationToken))
            {
                throw new InvalidOperationException($"Table mabarchive.{table} does not contain year and cannot be copied safely.");
            }

            var targetRows = await _repository.CountRowsByYearAsync("mabarchive", table, "year", context.TargetFpsYear.Value, cancellationToken);
            if (targetRows > 0)
            {
                throw new InvalidOperationException(
                    $"Table mabarchive.{table} already contains {targetRows} rows for target year {context.TargetFpsYear.Value}. Cleanup is required before Year End copy.");
            }

            var copied = await _repository.CopyMabArchiveYearScopedTableAsync(table, context.CurrentFpsYear.Value, context.TargetFpsYear.Value, cancellationToken);

            _logger.LogInformation(
                "YearEnd MABArchive table copy completed | CorrelationId={CorrelationId} | Table=mabarchive.{Table} | SourceYear={SourceYear} | TargetYear={TargetYear} | CopiedRows={CopiedRows}",
                context.CorrelationId,
                table,
                context.CurrentFpsYear,
                context.TargetFpsYear,
                copied);
        }
    }
}
