using Apha.BatchJobs.Domain.Entities.BulkRates;

namespace Apha.BatchJobs.Domain.Interfaces;

/// <summary>
/// Contract for all BulkRates worker data operations:
/// reading the approved job_queue entry, reading request-scoped staging rows,
/// writing permanent rate-change history, and deleting staging rows after commit.
/// </summary>
public interface IBulkRatesRepository
{
    /// <summary>
    /// Loads the fps.job_queue row for the given jobexecutionid. By the time a job's service
    /// calls this, JobOrchestrator has already transitioned the row from Approved to Running —
    /// the row's persisted approval metadata (approved_by/approved_at_utc) still reflects the
    /// earlier approval step. Returns null when no row exists with that identity.
    /// </summary>
    Task<BulkRatesJobQueueEntry?> GetRunningRequestAsync(
        Guid jobExecutionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all tblstagingtestorproduct rows scoped to the request.
    /// </summary>
    Task<IReadOnlyList<FecStagingRow>> GetFecStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all tblstagingtlkptestreqmt rows scoped to the request.
    /// </summary>
    Task<IReadOnlyList<AgrupStagingRow>> GetAgrupStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all tblstagingprofitcentregrade rows scoped to the request.
    /// </summary>
    Task<IReadOnlyList<StaffStagingRow>> GetStaffStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads all tblstaginganimals rows scoped to the request.
    /// </summary>
    Task<IReadOnlyList<AnimalStagingRow>> GetAnimalStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends a batch of rows to fps.rate_change_history.
    /// Must be called inside the execution transaction before it is committed.
    /// </summary>
    Task WriteHistoryBatchAsync(
        IReadOnlyList<RateChangeHistoryRow> rows,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all FEC staging rows (tblstagingtestorproduct and tblstagingtlkptestreqmt)
    /// scoped to the request. Called after successful transaction commit.
    /// </summary>
    Task DeleteFecStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all tblstagingprofitcentregrade rows scoped to the request.
    /// Called after successful transaction commit.
    /// </summary>
    Task DeleteStaffStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all tblstaginganimals rows scoped to the request.
    /// Called after successful transaction commit.
    /// </summary>
    Task DeleteAnimalStagingRowsAsync(
        Guid jobQueueId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends a chronology entry to fps.job_queue_log for the given request.
    /// Resolves the current statusid automatically; skips silently if the row is not found.
    /// </summary>
    Task WriteJobQueueLogAsync(
        Guid jobQueueId,
        string note,
        string? actor,
        CancellationToken cancellationToken = default);
}
