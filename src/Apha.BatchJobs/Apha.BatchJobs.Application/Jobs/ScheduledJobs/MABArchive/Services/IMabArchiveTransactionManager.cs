namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;

/// <summary>
/// Wraps the full Open+Planned MABArchive cycle in a single atomic transaction.
/// </summary>
public interface IMabArchiveTransactionManager
{
    Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken);
}
