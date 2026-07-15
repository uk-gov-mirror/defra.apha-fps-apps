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
            Guid id, byte[] fileBytes, string fileName)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(fileBytes);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "file", fileName);

            var url = string.Format(FpsApiEndpoints.UploadBulkRatesFile, id);
            var response = await _http.PostMultipartAsync<BulkRatesUploadResultDto>(url, content);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesUploadResultDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesUploadResultDto>>(response);
            return ApiResponseDto<BulkRatesUploadResultDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesUploadResultDto>> GetValidationResultsAsync(Guid id)
        {
            var url = string.Format(FpsApiEndpoints.GetBulkRatesValidation, id);
            var response = await _http.GetAsync<BulkRatesUploadResultDto>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesUploadResultDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesUploadResultDto>>(response);
            return ApiResponseDto<BulkRatesUploadResultDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesRequestDetailDto>> ReleaseForApprovalAsync(Guid id)
        {
            var url = string.Format(FpsApiEndpoints.ReleaseBulkRatesRequest, id);
            var response = await _http.PostAsync<object, BulkRatesRequestDetailDto>(url, new { });

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);
            return ApiResponseDto<BulkRatesRequestDetailDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesRequestDetailDto>> ApproveAsync(Guid id)
        {
            var url = string.Format(FpsApiEndpoints.ApproveBulkRatesRequest, id);
            var response = await _http.PostAsync<object, BulkRatesRequestDetailDto>(url, new { });

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);
            return ApiResponseDto<BulkRatesRequestDetailDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesRequestDetailDto>> RejectAsync(Guid id, string reason)
        {
            var url = string.Format(FpsApiEndpoints.RejectBulkRatesRequest, id);
            var response = await _http.PostAsync<object, BulkRatesRequestDetailDto>(
                url, new { Reason = reason });

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);
            return ApiResponseDto<BulkRatesRequestDetailDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesRequestDetailDto>> CancelAsync(Guid id, string? reason)
        {
            var url = string.Format(FpsApiEndpoints.CancelBulkRatesRequest, id);
            var response = await _http.PostAsync<object, BulkRatesRequestDetailDto>(
                url, new { Reason = reason });

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkRatesRequestDetailDto>>(response);
            return ApiResponseDto<BulkRatesRequestDetailDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<BulkRatesRequestDetailDto?>> GetRequestAsync(Guid id)
        {
            var url = string.Format(FpsApiEndpoints.GetBulkRatesRequest, id);
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

        // ── Helpers ────────────────────────────────────────────────────────────────

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
