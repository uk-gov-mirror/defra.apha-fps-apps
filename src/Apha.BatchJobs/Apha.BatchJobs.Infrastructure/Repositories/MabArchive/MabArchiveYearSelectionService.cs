using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive;

/// <summary>
/// Implementation of IMabArchiveYearSelectionService.
/// Resolves the Open and Planned FPS years for MABArchive processing from fps.tblyearmaster.
/// Year selection must never depend on the system date, month arithmetic, or physical
/// database naming - fps.tblyearmaster.yearstatus is the sole source of truth.
/// </summary>
public sealed class MabArchiveYearSelectionService : IMabArchiveYearSelectionService
{
    private const string OpenStatus = "Open";
    private const string PlannedStatus = "Planned";

    private readonly IDbContextFactory<BatchJobsDbContext> _dbContextFactory;
    private readonly ILogger<MabArchiveYearSelectionService> _logger;

    public MabArchiveYearSelectionService(
        IDbContextFactory<BatchJobsDbContext> dbContextFactory,
        ILogger<MabArchiveYearSelectionService> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Reads fps.tblyearmaster and validates the year configuration.
    /// </summary>
    public async Task<MabArchiveExecutionContext> GetProcessableYearsAsync(CancellationToken cancellationToken)
    {
        var rows = await ReadYearMasterRowsAsync(cancellationToken);
        var (openYears, plannedYears) = Bucket(rows);
        var selection = Validate(openYears, plannedYears);

        _logger.LogInformation(
            "MABArchive year selection resolved from fps.tblyearmaster | OpenYear={OpenYear} | PlannedYear={PlannedYear} | SelectionSource=fps.tblyearmaster",
            selection.OpenYear,
            selection.PlannedYear);

        return selection;
    }

    private async Task<List<(int FpsYear, string YearStatus)>> ReadYearMasterRowsAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        await dbContext.Database.OpenConnectionAsync(cancellationToken);

        var connection = dbContext.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT ym.fpsyear, ym.yearstatus
            FROM fps.tblyearmaster ym
            ORDER BY ym.fpsyear;";

        var rows = new List<(int FpsYear, string YearStatus)>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var fpsYear = reader.GetInt32(0);
            var yearStatus = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            rows.Add((fpsYear, yearStatus));
        }

        return rows;
    }

    /// <summary>
    /// Splits year-master rows into Open and Planned buckets by positive status match.
    /// Any other status (including Closed) is excluded by not matching either bucket -
    /// years must never be excluded via negative logic (e.g. "status &lt;&gt; Planned").
    /// </summary>
    internal static (IReadOnlyList<int> OpenYears, IReadOnlyList<int> PlannedYears) Bucket(
        IEnumerable<(int FpsYear, string YearStatus)> rows)
    {
        var openYears = new List<int>();
        var plannedYears = new List<int>();

        foreach (var row in rows)
        {
            if (string.Equals(row.YearStatus, OpenStatus, StringComparison.OrdinalIgnoreCase))
            {
                openYears.Add(row.FpsYear);
            }
            else if (string.Equals(row.YearStatus, PlannedStatus, StringComparison.OrdinalIgnoreCase))
            {
                plannedYears.Add(row.FpsYear);
            }
        }

        return (openYears, plannedYears);
    }

    /// <summary>
    /// Validates the bucketed years against the MABArchive selection rules and returns
    /// the resolved execution context.
    /// </summary>
    internal static MabArchiveExecutionContext Validate(
        IReadOnlyList<int> openYears,
        IReadOnlyList<int> plannedYears)
    {
        if (openYears.Count != 1)
        {
            throw new MabArchiveYearConfigurationException(
                $"Expected exactly one Open FPS year in fps.tblyearmaster, but found {openYears.Count}.");
        }

        if (plannedYears.Count > 1)
        {
            throw new MabArchiveYearConfigurationException(
                $"Expected zero or one Planned FPS year in fps.tblyearmaster, but found {plannedYears.Count}.");
        }

        var openYear = openYears[0];
        int? plannedYear = plannedYears.Count == 1 ? plannedYears[0] : null;

        if (plannedYear.HasValue && plannedYear.Value != openYear + 1)
        {
            throw new MabArchiveYearConfigurationException(
                $"Invalid FPS year configuration. Open year is {openYear}, but Planned year is {plannedYear.Value}. Expected {openYear + 1}.");
        }

        return new MabArchiveExecutionContext(openYear, plannedYear);
    }
}
