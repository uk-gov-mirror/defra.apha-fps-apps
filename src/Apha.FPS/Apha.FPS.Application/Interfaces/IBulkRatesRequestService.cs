using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities.BulkRates;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Business logic contract for the Bulk Rates request lifecycle.
    /// </summary>
    public interface IBulkRatesRequestService
    {
        /// <summary>Create a new Bulk Rates request in Initiated status.</summary>
        Task<BulkRatesRequestDto> CreateRequestAsync(
            string jobName, int fpsYear, string requestedBy,
            CancellationToken ct = default);

        /// <summary>Upload (or re-upload) an Excel file, replacing previous staging and re-running validation.</summary>
        Task<BulkRatesUploadResultDto> UploadFileAsync(
            Guid jobExecutionId, byte[] fileBytes, string filename,
            string requestedBy, CancellationToken ct = default);

        /// <summary>Retrieve structured validation results for a request.</summary>
        Task<BulkRatesUploadResultDto> GetValidationResultsAsync(
            Guid jobExecutionId, string requestedBy, CancellationToken ct = default);

        /// <summary>Release a fully-valid request for approval.</summary>
        Task<BulkRatesRequestDto> ReleaseForApprovalAsync(
            Guid jobExecutionId, string requestedBy, CancellationToken ct = default);

        /// <summary>Approve and publish EventBridge trigger.</summary>
        Task<BulkRatesRequestDto> ApproveAsync(
            Guid jobExecutionId, string approvedBy, CancellationToken ct = default);

        /// <summary>Reject with mandatory reason.</summary>
        Task<BulkRatesRequestDto> RejectAsync(
            Guid jobExecutionId, string rejectedBy, string reason, CancellationToken ct = default);

        /// <summary>Cancel an Initiated or Rejected request (initiator only).</summary>
        Task<BulkRatesRequestDto> CancelAsync(
            Guid jobExecutionId, string cancelledBy, string? reason, CancellationToken ct = default);

        /// <summary>Get full request detail including log history.</summary>
        Task<BulkRatesRequestDto?> GetRequestAsync(
            Guid jobExecutionId, CancellationToken ct = default);

        /// <summary>Server-side paged/sorted list, optionally filtered by job name, year and status.</summary>
        Task<PaginatedResult<BulkRatesQueueEntry>> GetRequestsAsync(
            string? jobName, int? fpsYear, string? status,
            QueryParameters<string> query, CancellationToken ct = default);

        /// <summary>Export current FEC test rates for the given year as an Excel (.xlsx) byte array.</summary>
        Task<byte[]> ExportFecTestDataAsync(int fpsYear, CancellationToken ct = default);

        /// <summary>
        /// Request-scoped FEC/AGRUP workbook download. Atomically captures an immutable
        /// snapshot of live data (fps.bulk_rates_downloaded_key) as the new active download version
        /// for this request before generating the workbook from that snapshot, and embeds the
        /// download version in protected workbook metadata so upload validation can reject a re-upload of a
        /// stale download. Only permitted while the request is Initiated or Rejected.
        /// </summary>
        Task<byte[]> DownloadFecTestDataAsync(Guid jobExecutionId, CancellationToken ct = default);

        /// <summary>Export current Staff rates for the given year as an Excel (.xlsx) byte array.</summary>
        Task<byte[]> ExportStaffTestDataAsync(int fpsYear, CancellationToken ct = default);

        /// <summary>Export current Animal rates for the given year as an Excel (.xlsx) byte array.</summary>
        Task<byte[]> ExportAnimalTestDataAsync(int fpsYear, CancellationToken ct = default);

        /// <summary>
        /// Request-scoped Staff workbook download, parity with
        /// DownloadFecTestDataAsync. Atomically captures an immutable snapshot of live
        /// fps.profitcentregrade data (fps.bulk_rates_staff_downloaded_key) as the new
        /// active download version for this request before generating the workbook from that
        /// snapshot, and embeds the download version in protected workbook metadata. Only
        /// permitted while the request is Initiated or Rejected, and only for a Staff request.
        /// </summary>
        Task<byte[]> DownloadStaffTestDataAsync(Guid jobExecutionId, CancellationToken ct = default);

        /// <summary>As DownloadStaffTestDataAsync, for Animal (fps.bulk_rates_animal_downloaded_key).</summary>
        Task<byte[]> DownloadAnimalTestDataAsync(Guid jobExecutionId, CancellationToken ct = default);

        /// <summary>Returns the currently active (blocking-status) request for jobName, or null if none exists.</summary>
        Task<BulkRatesQueueEntry?> GetActiveRequestAsync(string jobName, CancellationToken ct = default);

        /// <summary>
        /// Returns the staged FEC/AGRUP rows for a request's "FEC Data (Staging)" / "Agrup Details" grids,
        /// classified as Inserted/Updated/Deleted against live data. Empty for non-FEC requests.
        /// </summary>
        Task<BulkRatesStagingDataDto> GetStagingDataAsync(Guid jobExecutionId, CancellationToken ct = default);

        /// <summary>Exports the staged (not-yet-approved) FEC/AGRUP rows for a request as an Excel (.xlsx) byte array.</summary>
        Task<byte[]> ExportStagingDataAsync(Guid jobExecutionId, CancellationToken ct = default);
    }
}
