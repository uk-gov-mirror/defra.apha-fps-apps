using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Pagination;

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

        /// <summary>
        /// Server-side paged/sorted list, matching the app-wide DataGrid pagination convention.
        /// <paramref name="sortBy"/> is validated against a column whitelist internally — never
        /// interpolated directly into SQL.
        /// </summary>
        Task<PagedData<BulkRatesQueueEntry>> GetRequestsAsync(
            string? jobName, int? fpsYear, string? status,
            int page, int pageSize, string? sortBy, bool descending,
            CancellationToken ct = default);

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

        /// <summary>
        /// Bulk check for DR-VAL-01's ValidationContext.ProjectLookup: returns the subset of
        /// fps.tlkpproject.parentproject codes that exist for the given year.
        /// </summary>
        Task<IReadOnlySet<string>> GetExistingProjectCodesAsync(
            IEnumerable<string> parentProjectCodes, int fpsYear, CancellationToken ct = default);

        /// <summary>
        /// Bulk check for DR-VAL-01's ValidationContext.CapabilityLookup (DR-VAL-02): returns
        /// the subset of (testCode, workGroup) pairs that exist in fps.tlkptestcapability for
        /// the given year.
        /// </summary>
        Task<IReadOnlySet<(string TestCode, string WorkGroup)>> GetExistingCapabilityPairsAsync(
            IEnumerable<(string TestCode, string WorkGroup)> pairs, int fpsYear, CancellationToken ct = default);

        // ── Download snapshot (DR-UI-01, CR057/CR060) ────────────────────────────

        /// <summary>Next monotonic download_version for this request (1 if none exist yet).</summary>
        Task<int> GetNextDownloadVersionAsync(Guid jobQueueId, CancellationToken ct = default);

        /// <summary>
        /// DR-UI-01 steps 1-2: creates the download_version header as 'Generating' and persists
        /// the immutable snapshot rows (keys, source rates, and the descriptive fields the
        /// workbook needs to render — CR060) in one transaction.
        /// </summary>
        Task CreateDownloadSnapshotAsync(
            Guid jobQueueId, int downloadVersion,
            IReadOnlyList<FecStagingRow> fecRows, IReadOnlyList<AgrupStagingRow> agrupRows,
            CancellationToken ct = default);

        /// <summary>
        /// DR-UI-01 step 4: marks the header 'Ready' and sets job_queue.active_download_version,
        /// in one transaction — only called after the workbook has been generated successfully.
        /// </summary>
        Task MarkDownloadReadyAsync(Guid jobQueueId, int downloadVersion, CancellationToken ct = default);

        /// <summary>
        /// Best-effort: marks the header 'Failed' if workbook generation throws after the
        /// snapshot already committed. active_download_version is deliberately left untouched
        /// (plan §3, DR-UI-01) so the previous, still-valid version remains the one an upload is
        /// checked against.
        /// </summary>
        Task MarkDownloadFailedAsync(Guid jobQueueId, int downloadVersion, CancellationToken ct = default);

        /// <summary>
        /// DR-UI-01 step 3: reads back the just-persisted snapshot rows for a download version —
        /// never a live requery of fps.testorproduct (plan §3).
        /// </summary>
        Task<IReadOnlyList<FecStagingRow>> GetFecSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default);

        /// <summary>As GetFecSnapshotRowsAsync, for AGRUP — never a live requery of fps.tlkptestreqmt.</summary>
        Task<IReadOnlyList<AgrupStagingRow>> GetAgrupSnapshotRowsAsync(
            Guid jobQueueId, int downloadVersion, CancellationToken ct = default);

        // ── Export (live table reads for Excel download) ──────────────────────────
        Task<IReadOnlyList<FecStagingRow>> GetFecRowsForExportAsync(int fpsYear, CancellationToken ct = default);
        Task<IReadOnlyList<AgrupStagingRow>> GetAgrupRowsForExportAsync(int fpsYear, CancellationToken ct = default);
        Task<IReadOnlyList<StaffStagingRow>> GetStaffRowsForExportAsync(int fpsYear, CancellationToken ct = default);
        Task<IReadOnlyList<AnimalStagingRow>> GetAnimalRowsForExportAsync(int fpsYear, CancellationToken ct = default);

        // ── DR-API-07: freeze reviewed classification onto staging (CR056) ────────

        /// <summary>
        /// Writes the DR-VAL-01 classification computed at release time onto the matching
        /// FEC/AGRUP staging rows' calculated_action/effective_new_rate/source_current_rate/
        /// validation_version columns (CR056), keyed by business key (TestCode for FEC,
        /// TestCode+Buyer for AGRUP) — never by source row number, which is not stable across
        /// a DB read-back. Called once, at release (DR-API-07), so DR-WK-04's worker
        /// revalidation has a frozen baseline to detect drift against (plan §5.2).
        /// </summary>
        Task FreezeStagingCalculatedActionsAsync(
            Guid jobQueueId, int validationVersion,
            IReadOnlyList<BulkRatesFreezeEntry> fecFreezes,
            IReadOnlyList<BulkRatesFreezeEntry> agrupFreezes,
            CancellationToken ct = default);

        Task FreezeStaffStagingCalculatedActionsAsync(
            Guid jobQueueId,
            IReadOnlyList<StaffFreezeEntry> staffFreezes,
            CancellationToken ct = default);
    }
}
