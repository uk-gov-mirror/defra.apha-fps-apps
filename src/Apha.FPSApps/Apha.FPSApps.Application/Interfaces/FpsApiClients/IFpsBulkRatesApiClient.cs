using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// Client contract for the FPS API Bulk Rates Update endpoints (Phase 3).
    /// </summary>
    public interface IFpsBulkRatesApiClient
    {
        /// <summary>Create a new Bulk Rates request in Initiated status.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> CreateRequestAsync(
            string jobName, int fpsYear);

        /// <summary>Upload (or re-upload) an Excel file; replaces previous staging and re-runs validation.</summary>
        Task<ApiResponseDto<BulkRatesUploadResultDto>> UploadFileAsync(
            Guid jobExecutionId, byte[] fileBytes, string fileName);

        /// <summary>Retrieve structured validation results for a request.</summary>
        Task<ApiResponseDto<BulkRatesUploadResultDto>> GetValidationResultsAsync(Guid jobExecutionId);

        /// <summary>Release a fully-valid request for approval.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> ReleaseForApprovalAsync(Guid jobExecutionId);

        /// <summary>Approve and publish EventBridge trigger.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> ApproveAsync(Guid jobExecutionId);

        /// <summary>Reject with mandatory reason.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> RejectAsync(Guid jobExecutionId, string reason);

        /// <summary>Cancel an Initiated or Rejected request (initiator only).</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto>> CancelAsync(Guid jobExecutionId, string? reason);

        /// <summary>Get full request detail including log history.</summary>
        Task<ApiResponseDto<BulkRatesRequestDetailDto?>> GetRequestAsync(Guid jobExecutionId);

        /// <summary>Server-side paged/sorted list, optionally filtered by job name, FPS year and status.</summary>
        Task<ApiResponseDto<List<BulkRatesQueueEntryDto>>> GetRequestsAsync(
            QueryParameters<string> query, string? jobName = null, int? fpsYear = null, string? status = null);

        /// <summary>Returns the currently active (blocking-status) request for a job name, or null if none exists.</summary>
        Task<ApiResponseDto<BulkRatesQueueEntryDto?>> GetActiveRequestAsync(string jobName);

        /// <summary>Atomically snapshot live FEC/AGRUP data for a specific request and return the workbook.</summary>
        Task<byte[]> DownloadFecTestDataForRequestAsync(Guid jobExecutionId);

        /// <summary>Request-scoped Staff workbook download, parity with DownloadFecTestDataForRequestAsync.</summary>
        Task<byte[]> DownloadStaffTestDataForRequestAsync(Guid jobExecutionId);

        /// <summary>As DownloadStaffTestDataForRequestAsync, for Animal.</summary>
        Task<byte[]> DownloadAnimalTestDataForRequestAsync(Guid jobExecutionId);

        /// <summary>Download current FEC test rates for the given FPS year as an Excel byte array.</summary>
        Task<byte[]> DownloadFecTestDataAsync(int fpsYear);

        /// <summary>Download current Staff rates for the given FPS year as an Excel byte array.</summary>
        Task<byte[]> DownloadStaffTestDataAsync(int fpsYear);

        /// <summary>Download current Animal rates for the given FPS year as an Excel byte array.</summary>
        Task<byte[]> DownloadAnimalTestDataAsync(int fpsYear);

        /// <summary>Returns the staged FEC/AGRUP rows for the "FEC Data (Staging)" / "Agrup Details" grids.</summary>
        Task<ApiResponseDto<BulkRatesStagingDataDto>> GetStagingDataAsync(Guid jobExecutionId);

        /// <summary>Download the staged (not-yet-approved) FEC/AGRUP rows for a request as an Excel byte array.</summary>
        Task<byte[]> DownloadStagingDataAsync(Guid jobExecutionId);
    }
}
