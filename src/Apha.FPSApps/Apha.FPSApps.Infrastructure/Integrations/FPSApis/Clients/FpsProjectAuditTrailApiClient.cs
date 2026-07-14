using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using Microsoft.AspNetCore.WebUtilities;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProjectAuditTrailApiClient : IFpsProjectAuditTrailApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsProjectAuditTrailApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        //   project (required) appended as ?project={project}; date range appended when non-null
        public async Task<ApiResponseDto<List<ProjectLogDto>>> GetProjectLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            try
            {
                var url = BuildAuditUrl(FpsApiEndpoints.GetProjectLogs, query, project, fromDate, toDate);
                var response = await _http.GetAsync<List<ProjectLogRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProjectLogDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProjectLogDto>>>(response);
                return ApiResponseDto<List<ProjectLogDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProjectLogDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve project logs", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<StaffJobLogDto>>> GetStaffJobLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            try
            {
                var url = BuildAuditUrl(FpsApiEndpoints.GetStaffJobLogs, query, project, fromDate, toDate);
                var response = await _http.GetAsync<List<StaffJobLogRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<StaffJobLogDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<StaffJobLogDto>>>(response);
                return ApiResponseDto<List<StaffJobLogDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<StaffJobLogDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve staff job logs", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<TestRequirementLogDto>>> GetTestRequirementLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            try
            {
                var url = BuildAuditUrl(FpsApiEndpoints.GetTestRequirementLogs, query, project, fromDate, toDate);
                var response = await _http.GetAsync<List<TestRequirementLogRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TestRequirementLogDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<TestRequirementLogDto>>>(response);
                return ApiResponseDto<List<TestRequirementLogDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TestRequirementLogDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve test requirement logs", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<AnimalRequestLogDto>>> GetAnimalRequestLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            try
            {
                var url = BuildAuditUrl(FpsApiEndpoints.GetAnimalRequestLogs, query, project, fromDate, toDate);
                var response = await _http.GetAsync<List<AnimalRequestLogRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AnimalRequestLogDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AnimalRequestLogDto>>>(response);
                return ApiResponseDto<List<AnimalRequestLogDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AnimalRequestLogDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve animal request logs", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<AdditionalCostLogDto>>> GetAdditionalCostLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            try
            {
                var url = BuildAuditUrl(FpsApiEndpoints.GetAdditionalCostLogs, query, project, fromDate, toDate);
                var response = await _http.GetAsync<List<AdditionalCostLogRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AdditionalCostLogDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AdditionalCostLogDto>>>(response);
                return ApiResponseDto<List<AdditionalCostLogDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AdditionalCostLogDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve additional cost logs", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        //   project sent as-is; DateOnly? serialised as ISO 8601 (yyyy-MM-dd) to match [FromQuery] DateOnly? on backend
        private static string BuildAuditUrl(
            string baseEndpoint,
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate,
            DateOnly? toDate)
        {
            // Start with pagination params from QueryParameters<string>
            var url = QueryStringHelper.AddQueryString(baseEndpoint, query);

            // Append required project param
            var queryParams = new Dictionary<string, string?> { { "project", project } };

            // Append optional date range params when supplied
            if (fromDate.HasValue)
                queryParams["fromDate"] = fromDate.Value.ToString("yyyy-MM-dd");
            if (toDate.HasValue)
                queryParams["toDate"] = toDate.Value.ToString("yyyy-MM-dd");

            return QueryHelpers.AddQueryString(url, queryParams);
        }
    }
}
