namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Ports;

/// <summary>
/// Represents one MABArchive year-load step.
/// </summary>
public interface IMabArchiveLoader
{
    /// <summary>
    /// Baseline sequence position aligned with sp_AddYearsFPSData.
    /// </summary>
    int Sequence { get; }

    /// <summary>
    /// Logical loader name used in logs.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes this loader for the supplied year.
    /// </summary>
    Task<int> LoadAsync(int year, CancellationToken cancellationToken);
}
