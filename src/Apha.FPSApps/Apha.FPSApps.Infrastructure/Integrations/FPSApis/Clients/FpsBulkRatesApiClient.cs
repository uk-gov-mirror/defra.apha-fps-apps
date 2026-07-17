using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    /// <summary>
    /// Infrastructure implementation of <see cref="IFpsBulkRatesApiClient"/>.
    /// Calls the FPS API Bulk Rates endpoints (Phase 3, US-API-01–US-API-14).
    /// </summary>
    public class FpsBulkRatesApiClient : IFpsBulkRatesApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsBulkRatesApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesRequestDetailDto>> CreateRequestAsync(
            string jobName, int fpsYear)
        {
            var response = await _http.PostAsync<object, BulkRatesRequestDetailDto>(
                FpsApiEndpoints.CreateBulkRatesRequest,
                new { JobName = jobName, FpsYear = fpsYear });

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);
            return ApiResponseDto<BulkRatesRequestDetailDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesUploadResultDto>> UploadFileAsync(
            Guid jobExecutionId, byte[] fileBytes, string fileName)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "file", fileName);

            var url = string.Format(FpsApiEndpoints.UploadBulkRatesFile, jobExecutionId);
            var response = await _http.PostMultipartAsync<BulkRatesUploadResultDto>(url, content);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesUploadResultDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesUploadResultDto>>(response);
            return ApiResponseDto<BulkRatesUploadResultDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesUploadResultDto>> GetValidationResultsAsync(Guid jobExecutionId)
        {
            var url = string.Format(FpsApiEndpoints.GetBulkRatesValidation, jobExecutionId);
            var response = await _http.GetAsync<BulkRatesUploadResultDto>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesUploadResultDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesUploadResultDto>>(response);
            return ApiResponseDto<BulkRatesUploadResultDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesRequestDetailDto>> ReleaseForApprovalAsync(Guid jobExecutionId)
        {
            var url = string.Format(FpsApiEndpoints.ReleaseBulkRatesRequest, jobExecutionId);
            var response = await _http.PostAsync<object, BulkRatesRequestDetailDto>(url, new { });

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);
            return ApiResponseDto<BulkRatesRequestDetailDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesRequestDetailDto>> ApproveAsync(Guid jobExecutionId)
        {
            var url = string.Format(FpsApiEndpoints.ApproveBulkRatesRequest, jobExecutionId);
            var response = await _http.PostAsync<object, BulkRatesRequestDetailDto>(url, new { });

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);
            return ApiResponseDto<BulkRatesRequestDetailDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesRequestDetailDto>> RejectAsync(Guid jobExecutionId, string reason)
        {
            var url = string.Format(FpsApiEndpoints.RejectBulkRatesRequest, jobExecutionId);
            var response = await _http.PostAsync<object, BulkRatesRequestDetailDto>(
                url, new { Reason = reason });

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);
            return ApiResponseDto<BulkRatesRequestDetailDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesRequestDetailDto>> CancelAsync(Guid jobExecutionId, string? reason)
        {
            var url = string.Format(FpsApiEndpoints.CancelBulkRatesRequest, jobExecutionId);
            var response = await _http.PostAsync<object, BulkRatesRequestDetailDto>(
                url, new { Reason = reason });

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);
            return ApiResponseDto<BulkRatesRequestDetailDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesRequestDetailDto?>> GetRequestAsync(Guid jobExecutionId)
        {
            var url = string.Format(FpsApiEndpoints.GetBulkRatesRequest, jobExecutionId);
            var response = await _http.GetAsync<BulkRatesRequestDetailDto?>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto?>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto?>>(response);
            return ApiResponseDto<BulkRatesRequestDetailDto?>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<List<BulkRatesQueueEntryDto>>> GetRequestsAsync(
            string? jobName = null, int? fpsYear = null, string? status = null)
        {
            var url = BuildGetRequestsUrl(jobName, fpsYear, status);
            var response = await _http.GetAsync<List<BulkRatesQueueEntryDto>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<BulkRatesQueueEntryDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<BulkRatesQueueEntryDto>>>(response);
            return ApiResponseDto<List<BulkRatesQueueEntryDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesQueueEntryDto?>> GetActiveRequestAsync(string jobName)
        {
            var url = string.Format(FpsApiEndpoints.GetActiveBulkRatesRequest, Uri.EscapeDataString(jobName));
            var response = await _http.GetAsync<BulkRatesQueueEntryDto?>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesQueueEntryDto?>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesQueueEntryDto?>>(response);
            return ApiResponseDto<BulkRatesQueueEntryDto?>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesStagingDataDto>> GetStagingDataAsync(Guid jobExecutionId)
        {
            var url = string.Format(FpsApiEndpoints.GetBulkRatesStagingData, jobExecutionId);
            var response = await _http.GetAsync<BulkRatesStagingDataDto>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesStagingDataDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesStagingDataDto>>(response);
            return ApiResponseDto<BulkRatesStagingDataDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        // ── Helpers ────────────────────────────────────────────────────────────────
        /// <inheritdoc/>
        public Task<byte[]> DownloadFecTestDataAsync(int fpsYear)
        {
            var url = string.Format(FpsApiEndpoints.ExportBulkRatesFecTestData, fpsYear);
            return _http.GetFileAsync(url);
        }

        /// <inheritdoc/>
        public Task<byte[]> DownloadStaffTestDataAsync(int fpsYear)
        {
            var url = string.Format(FpsApiEndpoints.ExportBulkRatesStaffTestData, fpsYear);
            return _http.GetFileAsync(url);
        }

        /// <inheritdoc/>
        public Task<byte[]> DownloadAnimalTestDataAsync(int fpsYear)
        {
            var url = string.Format(FpsApiEndpoints.ExportBulkRatesAnimalTestData, fpsYear);
            return _http.GetFileAsync(url);
        }

        /// <inheritdoc/>
        public Task<byte[]> DownloadStagingDataAsync(Guid jobExecutionId)
        {
            var url = string.Format(FpsApiEndpoints.ExportBulkRatesStagingData, jobExecutionId);
            return _http.GetFileAsync(url);
        }
        private static string BuildGetRequestsUrl(string? jobName, int? fpsYear, string? status)
        {
            var queryParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(jobName))
                queryParts.Add($"jobName={Uri.EscapeDataString(jobName)}");
            if (fpsYear.HasValue)
                queryParts.Add($"fpsYear={fpsYear.Value}");
            if (!string.IsNullOrWhiteSpace(status))
                queryParts.Add($"status={Uri.EscapeDataString(status)}");

            return queryParts.Count == 0
                ? FpsApiEndpoints.GetBulkRatesRequests
                : $"{FpsApiEndpoints.GetBulkRatesRequests}?{string.Join("&", queryParts)}";
        }
    }
}
