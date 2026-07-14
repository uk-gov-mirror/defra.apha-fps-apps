using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Core.Entities.BulkRates;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Business logic contract for the Bulk Rates request lifecycle.
    /// Covers all 14 user stories in Phase 3 (US-API-01 through US-API-14).
    /// </summary>
    public interface IBulkRatesRequestService
    {
        /// <summary>US-API-01: Create a new Bulk Rates request in Initiated status.</summary>
        Task<BulkRatesRequestDto> CreateRequestAsync(
            string jobName, int fpsYear, string requestedBy,
            CancellationToken ct = default);

        /// <summary>US-API-02/03/05: Upload (or re-upload) an Excel file, replacing previous staging and re-running validation.</summary>
        Task<BulkRatesUploadResultDto> UploadFileAsync(
            Guid jobQueueId, byte[] fileBytes, string filename,
            string requestedBy, CancellationToken ct = default);

        /// <summary>US-API-04: Retrieve structured validation results for a request.</summary>
        Task<BulkRatesUploadResultDto> GetValidationResultsAsync(
            Guid jobQueueId, string requestedBy, CancellationToken ct = default);

        /// <summary>US-API-06/12/13: Release a fully-valid request for approval.</summary>
        Task<BulkRatesRequestDto> ReleaseForApprovalAsync(
            Guid jobQueueId, string requestedBy, CancellationToken ct = default);

        /// <summary>US-API-07/09/10/12/13: Approve and publish EventBridge trigger.</summary>
        Task<BulkRatesRequestDto> ApproveAsync(
            Guid jobQueueId, string approvedBy, CancellationToken ct = default);

        /// <summary>US-API-08/13: Reject with mandatory reason.</summary>
        Task<BulkRatesRequestDto> RejectAsync(
            Guid jobQueueId, string rejectedBy, string reason, CancellationToken ct = default);

        /// <summary>US-API-14/13: Cancel an Initiated or Rejected request (initiator only).</summary>
        Task<BulkRatesRequestDto> CancelAsync(
            Guid jobQueueId, string cancelledBy, string? reason, CancellationToken ct = default);

        /// <summary>US-API-11: Get full request detail including log history.</summary>
        Task<BulkRatesRequestDto?> GetRequestAsync(
            Guid jobQueueId, CancellationToken ct = default);

        /// <summary>US-API-11: List requests, optionally filtered by job name, year and status.</summary>
        Task<IReadOnlyList<BulkRatesQueueEntry>> GetRequestsAsync(
            string? jobName, int? fpsYear, string? status, CancellationToken ct = default);
    }
}
