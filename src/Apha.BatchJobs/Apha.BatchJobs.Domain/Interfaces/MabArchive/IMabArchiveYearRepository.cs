namespace Apha.BatchJobs.Domain.Interfaces.MabArchive;

/// <summary>
/// Manages yearly FPS archive data operations (delete, load, refresh).
/// </summary>
public interface IMabArchiveYearRepository
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
    /// Refreshes project cross-reference (my_tlkpproject_all) data for the specified year, without
    /// touching FPS totals, MABArchive transactional archive data, g_tlkpproject, or my_tlkpproject.
    /// Used for the Planned-year project-only refresh (legacy sp_LoadFromFPS parity: the
    /// future/Planned-year branch only ever called sp_AddMY_tlkpProject_All).
    /// </summary>
    /// <param name="year">The year to refresh project data for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of rows affected.</returns>
    Task<int> RefreshProjectsOnlyAsync(int year, CancellationToken cancellationToken);
}
