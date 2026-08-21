using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;
namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Steps;

/// <summary>
/// Copies fps.tblperiod rows from current FPS year into target FPS year with strict year scoping.
/// </summary>
public sealed class PeriodSetupStep : IYearEndDataSetupStep
{
    private const string TableSchema = "fps";
    private const string TableName = "tblperiod";
    private const string YearColumn = "fpsyear";

    private readonly IYearEndDataSetupRepository _repository;
    private readonly ILogger<PeriodSetupStep> _logger;

    public PeriodSetupStep(
        IYearEndDataSetupRepository repository,
        ILogger<PeriodSetupStep> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name => "PeriodSetupStep";

    public async Task ExecuteAsync(YearEndExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.CurrentFpsYear.HasValue || !context.TargetFpsYear.HasValue)
        {
            throw new InvalidOperationException("Year End context must include currentFpsYear and targetFpsYear before period setup.");
        }

        if (!await _repository.TableExistsAsync(TableSchema, TableName, cancellationToken))
        {
            throw new InvalidOperationException($"Required table {TableSchema}.{TableName} was not found.");
        }

        if (!await _repository.ColumnExistsAsync(TableSchema, TableName, YearColumn, cancellationToken))
        {
            throw new InvalidOperationException($"Required column {TableSchema}.{TableName}.{YearColumn} was not found.");
        }

        var currentCount = await _repository.CountRowsByYearAsync(TableSchema, TableName, YearColumn, context.CurrentFpsYear.Value, cancellationToken);
        if (currentCount == 0)
        {
            throw new InvalidOperationException(
                $"Source year {context.CurrentFpsYear.Value} has no rows in {TableSchema}.{TableName}; cannot prepare target period rows.");
        }

        var targetCount = await _repository.CountRowsByYearAsync(TableSchema, TableName, YearColumn, context.TargetFpsYear.Value, cancellationToken);
        if (targetCount > 0)
        {
            throw new InvalidOperationException(
                $"Target year {context.TargetFpsYear.Value} already has {targetCount} rows in {TableSchema}.{TableName}. Cleanup is required before period setup.");
        }

        var inserted = await _repository.CopyPeriodRowsAsync(context.CurrentFpsYear.Value, context.TargetFpsYear.Value, cancellationToken);

        _logger.LogInformation(
            "YearEnd period setup completed | CorrelationId={CorrelationId} | Table={Schema}.{Table} | SourceYear={SourceYear} | TargetYear={TargetYear} | InsertedRows={InsertedRows}",
            context.CorrelationId,
            TableSchema,
            TableName,
            context.CurrentFpsYear,
            context.TargetFpsYear,
            inserted);
    }
}