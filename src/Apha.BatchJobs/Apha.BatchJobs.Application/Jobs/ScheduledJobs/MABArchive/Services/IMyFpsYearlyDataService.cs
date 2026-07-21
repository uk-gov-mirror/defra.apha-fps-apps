namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;

/// <summary>
/// Service for managing yearly FPS archive data operations.
/// </summary>
public interface IMyFpsYearlyDataService
{
    /// <summary>
    /// Deletes all archive data for the specified year in dependency order.
    /// </summary>
    /// <param name="year">The year to delete archive data for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of rows affected.</returns>
    Task<int> DeleteYearDataAsync(int year, CancellationToken cancellationToken);

    /// <summary>
    /// Loads fresh archive data from FPS source for the specified year in dependency order.
    /// </summary>
    /// <param name="year">The year to load archive data for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of rows affected.</returns>
    Task<int> LoadYearDataAsync(int year, CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes project master (g_tlkpproject), project lookup (my_tlkpproject), and
    /// project cross-reference (my_tlkpproject_all) data for the specified year, without
    /// touching FPS totals or MABArchive transactional archive data. Used for the
    /// Planned-year project-only refresh.
    /// </summary>
    /// <param name="year">The year to refresh project data for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of rows affected.</returns>
    Task<int> RefreshProjectsOnlyAsync(int year, CancellationToken cancellationToken);
}
