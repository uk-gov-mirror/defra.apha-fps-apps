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
    /// <summary>
    /// HTTP client for the Resource Management Re-plan API endpoints (frmRM_RePlan).
    /// </summary>
    public class FpsResourceMgmtReplanApiClient : IFpsResourceMgmtReplanApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsResourceMgmtReplanApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<List<ResourceMgmtReplanViewDto>>> GetRePlanGridAsync(string workGroup, QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetResourceMgmtReplanGrid, query);
            url = QueryHelpers.AddQueryString(url, "workGroup", workGroup);
            var response = await _http.GetAsync<List<ResourceMgmtReplanViewRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanViewDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanViewDto>>>(response);
            return ApiResponseDto<List<ResourceMgmtReplanViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>> GetStaffJobsAsync(string jobCode, string wgGrade, QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetResourceMgmtReplanStaffJobs, query);
            url = QueryHelpers.AddQueryString(url, "jobCode", jobCode);
            url = QueryHelpers.AddQueryString(url, "wgGrade", wgGrade);
            var response = await _http.GetAsync<List<ResourceMgmtReplanStaffJobRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>>(response);
            return ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>> GetStagedRowsAsync(string jobCode, string wgGrade)
        {
            var url = QueryHelpers.AddQueryString(FpsApiEndpoints.GetResourceMgmtReplanStaged, "jobCode", jobCode);
            url = QueryHelpers.AddQueryString(url, "wgGrade", wgGrade);
            var response = await _http.GetAsync<List<ResourceMgmtReplanStaffJobRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>>(response);
            return ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<bool>> CommitReplanAsync(string jobCode, string wgGrade)
        {
            var url = QueryHelpers.AddQueryString(FpsApiEndpoints.CommitResourceMgmtReplan, "jobCode", jobCode);
            url = QueryHelpers.AddQueryString(url, "wgGrade", wgGrade);
            var response = await _http.PostAsync<object, bool>(url, new { });

            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
