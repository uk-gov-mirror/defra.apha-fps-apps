namespace Apha.BatchJobs.Domain.Interfaces.MabArchive;

/// <summary>
/// Rebuilds FPS source totals before archive load.
/// </summary>
public interface IFpsTotalsRepository
{
    /// <summary>
    /// Rebuilds FPS source totals for the specified year.
    /// </summary>
    /// <param name="year">The FPS year to rebuild totals for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of rows affected.</returns>
    Task<int> RebuildSourceTotalsAsync(int year, CancellationToken cancellationToken);
}
