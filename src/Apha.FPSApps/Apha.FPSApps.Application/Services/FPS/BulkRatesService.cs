using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

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

        public Task<ApiResponseDto<BulkRatesUploadResultDto>> UploadFileAsync(Guid id, byte[] fileBytes, string fileName)
            => _fpsClient.FpsBulkRates.UploadFileAsync(id, fileBytes, fileName);

        public Task<ApiResponseDto<BulkRatesUploadResultDto>> GetValidationResultsAsync(Guid id)
            => _fpsClient.FpsBulkRates.GetValidationResultsAsync(id);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> ReleaseForApprovalAsync(Guid id)
            => _fpsClient.FpsBulkRates.ReleaseForApprovalAsync(id);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> ApproveAsync(Guid id)
            => _fpsClient.FpsBulkRates.ApproveAsync(id);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> RejectAsync(Guid id, string reason)
            => _fpsClient.FpsBulkRates.RejectAsync(id, reason);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> CancelAsync(Guid id, string? reason)
            => _fpsClient.FpsBulkRates.CancelAsync(id, reason);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto?>> GetRequestAsync(Guid id)
            => _fpsClient.FpsBulkRates.GetRequestAsync(id);

        public Task<ApiResponseDto<List<BulkRatesQueueEntryDto>>> GetRequestsAsync(
            string? jobName = null, int? fpsYear = null, string? status = null)
            => _fpsClient.FpsBulkRates.GetRequestsAsync(jobName, fpsYear, status);
    }
}
