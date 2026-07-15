using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    /// <summary>
    /// Application-layer contract for Bulk Rates Update operations.
    /// MVC controllers and views inject this; the implementation delegates to <see cref="Interfaces.FpsApiClients.IFpsBulkRatesApiClient"/>.
    /// </summary>
    public interface IBulkRatesService
    {
        /// <summary>US-API-01: Create a new Bulk Rates request in Initiated status.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> CreateRequestAsync(string jobName, int fpsYear);

        /// <summary>US-API-02/03/05: Upload (or re-upload) an Excel file; replaces previous staging and re-runs validation.</summary>
        Task<ApiResponseDto<BulkRatesUploadResultDto>> UploadFileAsync(Guid id, byte[] fileBytes, string fileName);

        /// <summary>US-API-04: Retrieve structured validation results for a request.</summary>
        Task<ApiResponseDto<BulkRatesUploadResultDto>> GetValidationResultsAsync(Guid id);

        /// <summary>US-API-06: Release a fully-valid request for approval.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> ReleaseForApprovalAsync(Guid id);

        /// <summary>US-API-07/09: Approve and publish EventBridge trigger.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> ApproveAsync(Guid id);

        /// <summary>US-API-08: Reject with mandatory reason.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> RejectAsync(Guid id, string reason);

        /// <summary>US-API-14: Cancel an Initiated or Rejected request (initiator only).</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> CancelAsync(Guid id, string? reason);

        /// <summary>US-API-11: Get full request detail including log history.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto?>> GetRequestAsync(Guid id);

        /// <summary>US-API-11: List requests, optionally filtered by job name, FPS year and status.</summary>
        Task<ApiResponseDto<List<BulkRatesQueueEntryDto>>> GetRequestsAsync(
            string? jobName = null, int? fpsYear = null, string? status = null);
    }
}
