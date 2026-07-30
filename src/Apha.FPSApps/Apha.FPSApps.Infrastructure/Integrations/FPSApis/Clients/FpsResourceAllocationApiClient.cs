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
    public class FpsResourceAllocationApiClient : IFpsResourceAllocationApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsResourceAllocationApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ResourceStaffAllocationDto>>> GetPagedStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade, QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedResourceStaffAllocations, query);
            url = QueryHelpers.AddQueryString(url, "workGroupGrade", workGroupGrade);
            var response = await _http.GetAsync<List<ResourceStaffAllocationRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ResourceStaffAllocationDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ResourceStaffAllocationDto>>>(response);
            return ApiResponseDto<List<ResourceStaffAllocationDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<ResourceStaffJobDetailDto>>> GetPagedStaffJobDetailsByStaffIdAsync(string staffId, QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedResourceStaffJobDetails, query);
            url = QueryHelpers.AddQueryString(url, "staffId", staffId);
            var response = await _http.GetAsync<List<ResourceStaffJobDetailRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ResourceStaffJobDetailDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ResourceStaffJobDetailDto>>>(response);
            return ApiResponseDto<List<ResourceStaffJobDetailDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
