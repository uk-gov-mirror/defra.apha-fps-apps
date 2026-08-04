using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class BulkRatesService : IBulkRatesService
    {
        private readonly IFpsApiClient _fpsClient;

        public BulkRatesService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> CreateRequestAsync(string jobName, int fpsYear)
            => _fpsClient.FpsBulkRates.CreateRequestAsync(jobName, fpsYear);

        public Task<ApiResponseDto<BulkRatesUploadResultDto>> UploadFileAsync(Guid jobExecutionId, byte[] fileBytes, string fileName)
            => _fpsClient.FpsBulkRates.UploadFileAsync(jobExecutionId, fileBytes, fileName);

        public Task<ApiResponseDto<BulkRatesUploadResultDto>> GetValidationResultsAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.GetValidationResultsAsync(jobExecutionId);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> ReleaseForApprovalAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.ReleaseForApprovalAsync(jobExecutionId);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> ApproveAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.ApproveAsync(jobExecutionId);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> RejectAsync(Guid jobExecutionId, string reason)
            => _fpsClient.FpsBulkRates.RejectAsync(jobExecutionId, reason);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> CancelAsync(Guid jobExecutionId, string? reason)
            => _fpsClient.FpsBulkRates.CancelAsync(jobExecutionId, reason);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto?>> GetRequestAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.GetRequestAsync(jobExecutionId);

        public Task<ApiResponseDto<List<BulkRatesQueueEntryDto>>> GetRequestsAsync(
            QueryParameters<string> query, string? jobName = null, int? fpsYear = null, string? status = null)
            => _fpsClient.FpsBulkRates.GetRequestsAsync(query, jobName, fpsYear, status);

        public Task<byte[]> DownloadFecTestDataForRequestAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.DownloadFecTestDataForRequestAsync(jobExecutionId);

        public Task<byte[]> DownloadStaffTestDataForRequestAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.DownloadStaffTestDataForRequestAsync(jobExecutionId);

        public Task<byte[]> DownloadAnimalTestDataForRequestAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.DownloadAnimalTestDataForRequestAsync(jobExecutionId);

        public Task<byte[]> DownloadFecTestDataAsync(int fpsYear)
            => _fpsClient.FpsBulkRates.DownloadFecTestDataAsync(fpsYear);

        public Task<byte[]> DownloadStaffTestDataAsync(int fpsYear)
            => _fpsClient.FpsBulkRates.DownloadStaffTestDataAsync(fpsYear);

        public Task<byte[]> DownloadAnimalTestDataAsync(int fpsYear)
            => _fpsClient.FpsBulkRates.DownloadAnimalTestDataAsync(fpsYear);

        public Task<ApiResponseDto<BulkRatesQueueEntryDto?>> GetActiveRequestAsync(string jobName)
            => _fpsClient.FpsBulkRates.GetActiveRequestAsync(jobName);

        public Task<ApiResponseDto<BulkRatesStagingDataDto>> GetStagingDataAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.GetStagingDataAsync(jobExecutionId);

        public Task<byte[]> DownloadStagingDataAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.DownloadStagingDataAsync(jobExecutionId);
    }
}
