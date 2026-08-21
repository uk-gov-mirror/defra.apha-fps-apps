using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.Common;

namespace Apha.BatchJobs.Infrastructure.YearEnd.Repositories;

/// <summary>
/// Executes the year-status transition for Year End Cutover inside a single transaction.
/// </summary>
public sealed class YearEndCutoverRepository : IYearEndCutoverRepository
{
    private const string PlannedStatus = "Planned";
    private const string OpenStatus = "Open";
    private const string ClosedStatus = "Closed";

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;

    public YearEndCutoverRepository(IDbContextFactory<BatchJobsDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    public async Task ExecuteCutoverAsync(int currentYear, int targetYear, CancellationToken cancellationToken = default)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        var executionStrategy = dbContext.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var connection = dbContext.Database.GetDbConnection();
            var dbTransaction = transaction.GetDbTransaction();

            try
            {
                var targetState = await GetYearStateForUpdateAsync(connection, dbTransaction, targetYear, cancellationToken);
                if (targetState is null)
                {
                    throw new InvalidOperationException(
                        $"Target year {targetYear} does not exist in fps.tblyearmaster. Year End Data Setup must complete before cutover.");
                }

                if (!string.Equals(targetState.YearStatus, PlannedStatus, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Target year {targetYear} is in status '{targetState.YearStatus}', expected '{PlannedStatus}' before cutover.");
                }

                if (!targetState.Active)
                {
                    throw new InvalidOperationException($"Target year {targetYear} is inactive in fps.tblyearmaster.");
                }

                var currentState = await GetYearStateForUpdateAsync(connection, dbTransaction, currentYear, cancellationToken);
                if (currentState is null)
                {
                    throw new InvalidOperationException($"Current year {currentYear} does not exist in fps.tblyearmaster.");
                }

                if (!string.Equals(currentState.YearStatus, OpenStatus, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Current year {currentYear} is in status '{currentState.YearStatus}', expected '{OpenStatus}' before cutover.");
                }

                if (!currentState.Active)
                {
                    throw new InvalidOperationException($"Current year {currentYear} is inactive in fps.tblyearmaster.");
                }

                await UpdateYearStatusAsync(connection, dbTransaction, currentYear, ClosedStatus, cancellationToken);
                await UpdateYearStatusAsync(connection, dbTransaction, targetYear, OpenStatus, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        });
    }

    private static async Task<YearState?> GetYearStateForUpdateAsync(
        DbConnection connection,
        DbTransaction transaction,
        int fpsYear,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            SELECT ym.yearstatus, ym.active
            FROM fps.tblyearmaster ym
            WHERE ym.fpsyear = @fpsyear
            FOR UPDATE;";

        AddParameter(command, "fpsyear", fpsYear);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var yearStatus = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
        var active = !reader.IsDBNull(1) && reader.GetBoolean(1);

        return new YearState(yearStatus, active);
    }

    private static async Task UpdateYearStatusAsync(
        DbConnection connection,
        DbTransaction transaction,
        int fpsYear,
        string newStatus,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = @"
            UPDATE fps.tblyearmaster
            SET yearstatus = @yearstatus
            WHERE fpsyear = @fpsyear;";

        AddParameter(command, "yearstatus", newStatus);
        AddParameter(command, "fpsyear", fpsYear);

        var updated = await command.ExecuteNonQueryAsync(cancellationToken);
        if (updated != 1)
        {
            throw new InvalidOperationException(
                $"Expected to update exactly one row for fpsyear {fpsYear}, but updated {updated}.");
        }
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private sealed record YearState(string YearStatus, bool Active);
}
