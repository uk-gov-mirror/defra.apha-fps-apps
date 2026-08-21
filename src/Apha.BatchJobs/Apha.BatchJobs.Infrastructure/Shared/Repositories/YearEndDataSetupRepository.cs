using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data.Common;

namespace Apha.BatchJobs.Infrastructure.Shared.Repositories;

/// <summary>
/// Executes Year End Data Setup persistence operations against fps.tblyearmaster.
/// </summary>
public sealed class YearEndDataSetupRepository : IYearEndDataSetupRepository
{
    private const string PlannedStatus = "Planned";
    private const string BatchCreatedBy = "YearEndBatchWorker";

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;

    public YearEndDataSetupRepository(IDbContextFactory<BatchJobsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task<(string YearStatus, bool Active)?> GetYearStateAsync(int fpsYear, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ym.yearstatus, ym.active
            FROM fps.tblyearmaster ym
            WHERE ym.fpsyear = @fpsyear;";

        AddParameter(command, "fpsyear", fpsYear);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var yearStatus = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
        var active = !reader.IsDBNull(1) && reader.GetBoolean(1);

        return (yearStatus, active);
    }

    public async Task<int> InsertPlannedYearAsync(int fpsYear, string fpsYearCode, string correlationId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO fps.tblyearmaster (fpsyear, fpsyearcode, yearstatus, remarks, active, createdby)
            VALUES (@fpsyear, @fpsyearcode, @yearstatus, @remarks, @active, @createdby);";

        AddParameter(command, "fpsyear", fpsYear);
        AddParameter(command, "fpsyearcode", fpsYearCode);
        AddParameter(command, "yearstatus", PlannedStatus);
        AddParameter(command, "remarks", $"Created by YearEndDataSetup. CorrelationId={correlationId}");
        AddParameter(command, "active", true);
        AddParameter(command, "createdby", BatchCreatedBy);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> TableExistsAsync(string schema, string table, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = @schema
                  AND table_name = @table
            );";

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);

        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    public async Task<bool> ColumnExistsAsync(string schema, string table, string column, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.columns
                WHERE table_schema = @schema
                  AND table_name = @table
                  AND column_name = @column
            );";

        AddParameter(command, "schema", schema);
        AddParameter(command, "table", table);
        AddParameter(command, "column", column);

        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    public async Task<bool> YearRowExistsAsync(int fpsYear, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT EXISTS (
                SELECT 1
                FROM fps.tblyearmaster ym
                WHERE ym.fpsyear = @fpsyear
            );";

        AddParameter(command, "fpsyear", fpsYear);

        return await ExecuteBooleanAsync(command, cancellationToken);
    }

    public async Task<long> CountRowsByYearAsync(string schema, string table, string yearColumn, int year, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {schema}.{table} WHERE {yearColumn} = @target_year;";
        AddParameter(command, "target_year", year);

        var scalar = await command.ExecuteScalarAsync(cancellationToken);
        return scalar is long count ? count : Convert.ToInt64(scalar);
    }

    public async Task<string?> ResolveYearColumnAsync(string schema, string table, CancellationToken cancellationToken = default)
    {
        if (await ColumnExistsAsync(schema, table, "fpsyear", cancellationToken))
        {
            return "fpsyear";
        }

        if (await ColumnExistsAsync(schema, table, "year", cancellationToken))
        {
            return "year";
        }

        return null;
    }

    public async Task<int> DeleteRowsByYearAsync(string schema, string table, string yearColumn, int targetYear, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {schema}.{table} WHERE {yearColumn} = @target_year;";
        AddParameter(command, "target_year", targetYear);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public Task<int> DeleteInactiveEmployeeJobRowsAsync(string schema, string jobTable, string yearColumn, string jobStaffColumn, string employeeTable, string employeeStaffColumn, int targetYear, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("This Year End operation is awaiting the active Year End branch implementation.");

    public Task<int> CopyPeriodRowsAsync(int sourceYear, int targetYear, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("This Year End operation is awaiting the active Year End branch implementation.");

    public Task<int> ResetFieldsByYearAsync(string schema, string table, string yearColumn, IReadOnlyDictionary<string, string> rules, int targetYear, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("This Year End operation is awaiting the active Year End branch implementation.");

    public Task<int> CopyFpsYearScopedTableAsync(string table, int sourceYear, int targetYear, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("This Year End operation is awaiting the active Year End branch implementation.");

    public Task<int> CopyMabArchiveYearScopedTableAsync(string table, int sourceYear, int targetYear, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("This Year End operation is awaiting the active Year End branch implementation.");

    private static async Task<bool> ExecuteBooleanAsync(DbCommand command, CancellationToken cancellationToken)
    {
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is bool value && value;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
