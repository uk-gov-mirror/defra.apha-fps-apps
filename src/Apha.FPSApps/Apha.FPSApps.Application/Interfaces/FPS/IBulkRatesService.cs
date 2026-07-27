using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;
using Apha.FPSApps.Application.Pagination;

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
        Task<ApiResponseDto<BulkRatesUploadResultDto>> UploadFileAsync(Guid jobExecutionId, byte[] fileBytes, string fileName);

        /// <summary>US-API-04: Retrieve structured validation results for a request.</summary>
        Task<ApiResponseDto<BulkRatesUploadResultDto>> GetValidationResultsAsync(Guid jobExecutionId);

        /// <summary>US-API-06: Release a fully-valid request for approval.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> ReleaseForApprovalAsync(Guid jobExecutionId);

        /// <summary>US-API-07/09: Approve and publish EventBridge trigger.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> ApproveAsync(Guid jobExecutionId);

        /// <summary>US-API-08: Reject with mandatory reason.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> RejectAsync(Guid jobExecutionId, string reason);

        /// <summary>US-API-14: Cancel an Initiated or Rejected request (initiator only).</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> CancelAsync(Guid jobExecutionId, string? reason);

        /// <summary>US-API-11: Get full request detail including log history.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto?>> GetRequestAsync(Guid jobExecutionId);

        /// <summary>US-API-11: Server-side paged/sorted list, optionally filtered by job name, FPS year and status.</summary>
        Task<ApiResponseDto<List<BulkRatesQueueEntryDto>>> GetRequestsAsync(
            QueryParameters<string> query, string? jobName = null, int? fpsYear = null, string? status = null);

        /// <summary>DR-UI-01: Atomically snapshot live FEC/AGRUP data for a specific request and return the workbook.</summary>
        Task<byte[]> DownloadFecTestDataForRequestAsync(Guid jobExecutionId);

        /// <summary>Download current FEC test rates for the given FPS year as an Excel byte array.</summary>
        Task<byte[]> DownloadFecTestDataAsync(int fpsYear);

        /// <summary>Download current Staff rates for the given FPS year as an Excel byte array.</summary>
        Task<byte[]> DownloadStaffTestDataAsync(int fpsYear);

        /// <summary>Download current Animal rates for the given FPS year as an Excel byte array.</summary>
        Task<byte[]> DownloadAnimalTestDataAsync(int fpsYear);

        /// <summary>Returns the currently active (blocking-status) request for a job name, or null if none exists.</summary>
        Task<ApiResponseDto<BulkRatesQueueEntryDto?>> GetActiveRequestAsync(string jobName);

        /// <summary>Returns the staged FEC/AGRUP rows for the "FEC Data (Staging)" / "Agrup Details" grids.</summary>
        Task<ApiResponseDto<BulkRatesStagingDataDto>> GetStagingDataAsync(Guid jobExecutionId);

        /// <summary>Download the staged (not-yet-approved) FEC/AGRUP rows for a request as an Excel byte array.</summary>
        Task<byte[]> DownloadStagingDataAsync(Guid jobExecutionId);
    }
}
