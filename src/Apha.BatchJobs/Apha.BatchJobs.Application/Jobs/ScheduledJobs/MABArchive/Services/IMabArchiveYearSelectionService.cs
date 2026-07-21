namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;

/// <summary>
/// Resolves the FPS years MABArchive must process from fps.tblyearmaster.
/// </summary>
public interface IMabArchiveYearSelectionService
{
    /// <summary>
    /// Reads fps.tblyearmaster and returns the Open year (required, exactly one) and the
    /// Planned year (optional, at most one, must equal Open year + 1 when present).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="Domain.Exceptions.MabArchiveYearConfigurationException">
    /// Thrown when zero or more than one Open year exists, more than one Planned year
    /// exists, or a Planned year exists that is not Open year + 1.
    /// </exception>
    Task<MabArchiveExecutionContext> GetProcessableYearsAsync(CancellationToken cancellationToken);
}
