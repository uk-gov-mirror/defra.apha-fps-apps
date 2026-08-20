using System.Collections.Generic;

namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// Persistence contract for Year End Data Setup operations.
/// </summary>
public interface IYearEndDataSetupRepository
{
    /// <summary>
    /// Checks whether a year row exists in fps.tblyearmaster and returns its status and active flag,
    /// or null if no row exists for the given year.
    /// </summary>
    Task<(string YearStatus, bool Active)?> GetYearStateAsync(int fpsYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a new row in fps.tblyearmaster with Planned status and returns the number of rows affected.
    /// </summary>
    Task<int> InsertPlannedYearAsync(int fpsYear, string fpsYearCode, string correlationId, CancellationToken cancellationToken = default);

    /// <summary>Returns true if the specified table exists in information_schema.</summary>
    Task<bool> TableExistsAsync(string schema, string table, CancellationToken cancellationToken = default);

    /// <summary>Returns true if the specified column exists in information_schema.</summary>
    Task<bool> ColumnExistsAsync(string schema, string table, string column, CancellationToken cancellationToken = default);

    /// <summary>Returns true if a row for the given fpsYear exists in fps.tblyearmaster.</summary>
    Task<bool> YearRowExistsAsync(int fpsYear, CancellationToken cancellationToken = default);

    /// <summary>Returns the count of rows in the given table matching the specified year column value.</summary>
    Task<long> CountRowsByYearAsync(string schema, string table, string yearColumn, int year, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the year column for a table by checking for "fpsyear" then "year" in information_schema.
    /// Returns null if neither column exists.
    /// </summary>
    Task<string?> ResolveYearColumnAsync(string schema, string table, CancellationToken cancellationToken = default);

    /// <summary>Deletes rows in the given table matching the specified year column value.</summary>
    Task<int> DeleteRowsByYearAsync(string schema, string table, string yearColumn, int targetYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes target-year staff-job rows linked to inactive employees.
    /// Awaiting implementation from the active Year End branch.
    /// </summary>
    Task<int> DeleteInactiveEmployeeJobRowsAsync(string schema, string jobTable, string yearColumn, string jobStaffColumn, string employeeTable, string employeeStaffColumn, int targetYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies fps.tblperiod rows from sourceYear into targetYear.
    /// Awaiting implementation from the active Year End branch.
    /// </summary>
    Task<int> CopyPeriodRowsAsync(int sourceYear, int targetYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies field reset rules to target-year rows in the specified table.
    /// Awaiting implementation from the active Year End branch.
    /// </summary>
    Task<int> ResetFieldsByYearAsync(string schema, string table, string yearColumn, IReadOnlyDictionary<string, string> rules, int targetYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies fps-schema year-scoped rows from sourceYear into targetYear for the given table.
    /// Awaiting implementation from the active Year End branch.
    /// </summary>
    Task<int> CopyFpsYearScopedTableAsync(string table, int sourceYear, int targetYear, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies mabarchive-schema year-scoped rows from sourceYear into targetYear for the given table.
    /// Awaiting implementation from the active Year End branch.
    /// </summary>
    Task<int> CopyMabArchiveYearScopedTableAsync(string table, int sourceYear, int targetYear, CancellationToken cancellationToken = default);
}
