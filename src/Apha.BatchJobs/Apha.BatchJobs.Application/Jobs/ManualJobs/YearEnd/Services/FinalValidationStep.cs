using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Services;

/// <summary>
/// Validates final target-year setup state before Year End Data Setup completion.
/// </summary>
public sealed class FinalValidationStep : IYearEndDataSetupStep
{
    private static readonly IReadOnlyList<(string Schema, string Table, string YearColumn)> RequiredTargetYearDataTables =
    [
        ("fps", "tlkpproject", "fpsyear"),
        ("fps", "tblstaffjob", "fpsyear"),
        ("fps", "tlkptestreqmt", "fpsyear"),
        ("fps", "tblanimalreq", "fpsyear"),
        ("fps", "tbladditionalcosts", "fpsyear"),
        ("fps", "tblperiod", "fpsyear"),
        ("mabarchive", "my_tlkpproject", "year"),
        ("mabarchive", "my_tblstaffjob", "year"),
        ("mabarchive", "my_tlkptestreqmt", "year"),
        ("mabarchive", "my_tblanimalreq", "year"),
        ("mabarchive", "my_tbladditionalcosts", "year")
    ];

    private static readonly IReadOnlyList<string> MustBeEmptyTargetYearTables =
    [
        "monthlyoutput",
        "monthlytime",
        "proj_invoice",
        "proj_subcontract",
        "projectmonth",
        "projectmonthfinal",
        "timecostcalcs"
    ];

    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<FinalValidationStep> _logger;

    public FinalValidationStep(
        IYearEndDataSetupRepository repository,
        ILogger<FinalValidationStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "FinalValidationStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include targetFpsYear before final validation.");
        }

        await ValidateTargetYearMasterStateAsync(context.TargetFpsYear.Value, cancellationToken);
        await ValidateRequiredTargetDataAsync(context.TargetFpsYear.Value, cancellationToken);
        await ValidateTargetYearEmptyTablesAsync(context.TargetFpsYear.Value, cancellationToken);

        _logger.LogInformation(
            "YearEnd final validation completed | CorrelationId={CorrelationId} | TargetYear={TargetYear}",
            context.CorrelationId,
            context.TargetFpsYear);
    }

    private async Task ValidateTargetYearMasterStateAsync(int targetYear, CancellationToken cancellationToken)
    {
        var state = await _repository.GetYearStateAsync(targetYear, cancellationToken);

        if (state is null)
        {
            throw new InvalidOperationException($"Target year {targetYear} does not exist in fps.tblyearmaster.");
        }

        var (yearStatus, active) = state.Value;

        if (!string.Equals(yearStatus, "Planned", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Target year {targetYear} is in status '{yearStatus}', expected 'Planned' before cutover.");
        }

        if (!active)
        {
            throw new InvalidOperationException($"Target year {targetYear} is inactive in fps.tblyearmaster.");
        }
    }

    private async Task ValidateRequiredTargetDataAsync(int targetYear, CancellationToken cancellationToken)
    {
        foreach (var (schema, table, yearColumn) in RequiredTargetYearDataTables)
        {
            if (!await _repository.TableExistsAsync(schema, table, cancellationToken))
            {
                continue;
            }

            if (!await _repository.ColumnExistsAsync(schema, table, yearColumn, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Required validation table {schema}.{table} does not contain year column {yearColumn}.");
            }

            var count = await _repository.CountRowsByYearAsync(schema, table, yearColumn, targetYear, cancellationToken);
            if (count <= 0)
            {
                throw new InvalidOperationException(
                    $"Expected target-year rows in {schema}.{table} for year {targetYear}, but found none.");
            }
        }
    }

    private async Task ValidateTargetYearEmptyTablesAsync(int targetYear, CancellationToken cancellationToken)
    {
        foreach (var table in MustBeEmptyTargetYearTables)
        {
            if (!await _repository.TableExistsAsync("fps", table, cancellationToken))
            {
                continue;
            }

            var yearColumn = await _repository.ResolveYearColumnAsync("fps", table, cancellationToken);
            if (yearColumn is null)
            {
                continue;
            }

            var count = await _repository.CountRowsByYearAsync("fps", table, yearColumn, targetYear, cancellationToken);
            if (count != 0)
            {
                throw new InvalidOperationException(
                    $"Expected no target-year rows in fps.{table} for year {targetYear}, but found {count}.");
            }
        }
    }
}
