using Apha.FPS.Core.Entities.BulkRates;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Data access contract for all Bulk Rates API operations against fps schema tables.
    /// Implemented by BulkRatesRepository in Apha.FPS.DataAccess using raw Npgsql.
    /// </summary>
    public interface IBulkRatesRepository
    {
        // ── Job master / status lookup ───────────────────────────────────────────
        Task<int?> GetJobIdByNameAsync(string jobName, CancellationToken ct = default);
        Task<int?> GetStatusIdByNameAsync(int jobId, string statusName, CancellationToken ct = default);

        // ── Queue entry CRUD ─────────────────────────────────────────────────────
        Task<BulkRatesQueueEntry> CreateRequestAsync(
            Guid jobQueueId, Guid jobExecutionId, int jobId, int initiatedStatusId,
            string requestedBy, DateTime requestedAtUtc, int fpsYear,
            CancellationToken ct = default);

        Task<BulkRatesQueueEntry?> GetRequestAsync(Guid jobExecutionId, CancellationToken ct = default);

        Task<IReadOnlyList<BulkRatesQueueEntry>> GetRequestsAsync(
            string? jobName, int? fpsYear, string? status, CancellationToken ct = default);

        /// <summary>
        /// Returns the most recent request for <paramref name="jobName"/> that is still in a
        /// blocking status (Initiated, ReleasedForApproval, Approved, Running, or Failed), or
        /// null if none exists. Used to enforce the single-active-request-per-job-type rule.
        /// </summary>
        Task<BulkRatesQueueEntry?> GetActiveRequestAsync(string jobName, CancellationToken ct = default);

        // ── Status transitions ───────────────────────────────────────────────────
        /// <summary>
        /// Performs a guarded UPDATE: only changes status if the current statusId matches
        /// <paramref name="expectedStatusId"/>. Returns true when the row was updated.
        /// </summary>
        Task<bool> TransitionStatusAsync(
            Guid jobQueueId, int expectedStatusId, int newStatusId,
            CancellationToken ct = default);

        Task SetApprovalAsync(
            Guid jobQueueId, Guid jobExecutionId,
            string approvedBy, DateTime approvedAtUtc,
            string triggeredBy, DateTime triggeredAtUtc,
            int approvedStatusId,
            CancellationToken ct = default);

        Task SetRejectionAsync(
            Guid jobQueueId, string rejectedBy, DateTime rejectedAtUtc,
            string reason, int rejectedStatusId,
            CancellationToken ct = default);

        Task SetCancellationAsync(
            Guid jobQueueId, string cancelledBy, DateTime cancelledAtUtc,
            string? reason, int cancelledStatusId,
            CancellationToken ct = default);

        // ── Upload metadata ──────────────────────────────────────────────────────
        Task UpdateUploadMetadataAsync(
            Guid jobQueueId, string filename, string checksumSha256, int uploadVersion,
            DateTime validatedAtUtc, string rowCountsJson, CancellationToken ct = default);

        // ── Audit log ────────────────────────────────────────────────────────────
        Task WriteJobQueueLogAsync(
            Guid jobQueueId, string note, string? actor, CancellationToken ct = default);

        Task<IReadOnlyList<BulkRatesQueueLog>> GetJobQueueLogsAsync(
            Guid jobQueueId, CancellationToken ct = default);

        // ── Staging — replace semantics (delete-then-insert within transaction) ──
        Task ReplaceStagingFecAsync(
            Guid jobQueueId,
            IReadOnlyList<FecStagingRow> fecRows,
            IReadOnlyList<AgrupStagingRow> agrupRows,
            CancellationToken ct = default);

        Task ReplaceStagingStaffAsync(
            Guid jobQueueId,
            IReadOnlyList<StaffStagingRow> rows,
            CancellationToken ct = default);

        Task ReplaceStagingAnimalAsync(
            Guid jobQueueId,
            IReadOnlyList<AnimalStagingRow> rows,
            CancellationToken ct = default);

        /// <summary>Deletes all staging rows for the given request. Used on cancellation.</summary>
        Task ClearStagingByJobQueueIdAsync(
            Guid jobQueueId, string jobName, CancellationToken ct = default);

        Task<IReadOnlyList<FecStagingRow>> GetFecStagingRowsAsync(Guid jobQueueId, CancellationToken ct = default);
        Task<IReadOnlyList<AgrupStagingRow>> GetAgrupStagingRowsAsync(Guid jobQueueId, CancellationToken ct = default);
        Task<IReadOnlyList<StaffStagingRow>> GetStaffStagingRowsAsync(Guid jobQueueId, CancellationToken ct = default);
        Task<IReadOnlyList<AnimalStagingRow>> GetAnimalStagingRowsAsync(Guid jobQueueId, CancellationToken ct = default);

        // ── Validation errors ────────────────────────────────────────────────────
        Task ReplaceValidationErrorsAsync(
            Guid jobQueueId,
            IReadOnlyList<StagingValidationError> errors,
            CancellationToken ct = default);

        Task<IReadOnlyList<StagingValidationError>> GetValidationErrorsAsync(
            Guid jobQueueId, CancellationToken ct = default);

        /// <summary>
        /// Atomically sets status to Cancelled and deletes all staging rows for the request.
        /// Used exclusively for the Cancel workflow where both must succeed or neither should.
        /// </summary>
        Task CancelAndClearStagingAsync(
            Guid jobQueueId, string jobName,
            string cancelledBy, DateTime cancelledAtUtc,
            string? reason, int cancelledStatusId,
            CancellationToken ct = default);

        // ── Reference checks (used during upload validation) ─────────────────────
        Task<bool> FpsYearExistsAsync(int fpsYear, CancellationToken ct = default);

        /// <summary>Bulk check: returns the subset of testCodes that already exist for the given year.</summary>
        Task<IReadOnlySet<string>> GetExistingTestCodesAsync(
            IEnumerable<string> testCodes, int fpsYear, CancellationToken ct = default);

        /// <summary>Bulk check: returns the subset of (testCode, buyer) pairs that already exist for the given year.</summary>
        Task<IReadOnlySet<(string TestCode, string Buyer)>> GetExistingAgrupKeysAsync(
            IEnumerable<(string TestCode, string Buyer)> keys, int fpsYear, CancellationToken ct = default);

        // ── Export (live table reads for Excel download) ──────────────────────────
        Task<IReadOnlyList<FecStagingRow>> GetFecRowsForExportAsync(int fpsYear, CancellationToken ct = default);
        Task<IReadOnlyList<AgrupStagingRow>> GetAgrupRowsForExportAsync(int fpsYear, CancellationToken ct = default);
        Task<IReadOnlyList<StaffStagingRow>> GetStaffRowsForExportAsync(int fpsYear, CancellationToken ct = default);
        Task<IReadOnlyList<AnimalStagingRow>> GetAnimalRowsForExportAsync(int fpsYear, CancellationToken ct = default);
    }
}
