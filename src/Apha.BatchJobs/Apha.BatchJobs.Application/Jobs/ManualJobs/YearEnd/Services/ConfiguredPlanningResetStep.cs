using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Applies configured planning-field resets for target-year rows using strict year scoping.
/// </summary>
public sealed class ConfiguredPlanningResetStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> FpsResetRulesByTable =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
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

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> MabArchiveResetRulesByTable =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
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
    private readonly ILogger<ConfiguredPlanningResetStep> _logger;

    public ConfiguredPlanningResetStep(
        IYearEndDataSetupRepository repository,
        ILogger<ConfiguredPlanningResetStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "ConfiguredPlanningResetStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before configured planning reset.");
        }

        var totalUpdated = 0;

        foreach (var (tableName, resetRules) in FpsResetRulesByTable)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await _repository.TableExistsAsync("fps", tableName, cancellationToken))
            {
                _logger.LogWarning(
                    "YearEnd planning reset skipped missing table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    context.CorrelationId,
                    "fps",
                    tableName);
                continue;
            }

            if (!await _repository.ColumnExistsAsync("fps", tableName, "fpsyear", cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Table fps.{tableName} does not contain required year column fpsyear for safe planning reset.");
            }

            totalUpdated += await _repository.ResetFieldsByYearAsync("fps", tableName, "fpsyear", resetRules, context.TargetFpsYear.Value, cancellationToken);
        }

        foreach (var (tableName, resetRules) in MabArchiveResetRulesByTable)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await _repository.TableExistsAsync("mabarchive", tableName, cancellationToken))
            {
                _logger.LogWarning(
                    "YearEnd planning reset skipped missing table | CorrelationId={CorrelationId} | Table={Schema}.{Table}",
                    context.CorrelationId,
                    "mabarchive",
                    tableName);
                continue;
            }

            if (!await _repository.ColumnExistsAsync("mabarchive", tableName, "year", cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Table mabarchive.{tableName} does not contain required year column year for safe planning reset.");
            }

            totalUpdated += await _repository.ResetFieldsByYearAsync("mabarchive", tableName, "year", resetRules, context.TargetFpsYear.Value, cancellationToken);
        }

        _logger.LogInformation(
            "YearEnd configured planning reset completed | CorrelationId={CorrelationId} | TargetYear={TargetYear} | UpdatedRows={UpdatedRows}",
            context.CorrelationId,
            context.TargetFpsYear,
            totalUpdated);
    }
}
