using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactMonthlyTimeApiClient : IPactMonthlyTimeApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactMonthlyTimeApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<MonthlyTimeDto>>> GetMonthlyTimeByTimeCodeAndProjectAsync(string timeCode, string workGroup, string parentProject)
        {
            var url = string.Format(PactApiEndpoints.GetMonthlyTimeByTimeCodeAndProject,
                Uri.EscapeDataString(timeCode),
                Uri.EscapeDataString(workGroup),
                Uri.EscapeDataString(parentProject));
            var response = await _http.GetAsync<List<MonthlyTimeRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthlyTimeDto>>>(response);
            var dto = _mapper.Map<ApiResponseDto<List<MonthlyTimeDto>>>(response);
            return ApiResponseDto<List<MonthlyTimeDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<MonthlyTimeDto>>> GetPagedMonthlyTimeAsync(QueryParameters<string> query, string? timeCode, string? workGroup, string? parentProject)
        {
            var baseUrl = PactApiEndpoints.GetPagedMonthlyTime;
            var url = QueryStringHelper.AddQueryString(baseUrl, query);
            if (!string.IsNullOrWhiteSpace(timeCode))
                url += $"&timeCode={Uri.EscapeDataString(timeCode)}";
            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";
            if (!string.IsNullOrWhiteSpace(parentProject))
                url += $"&parentProject={Uri.EscapeDataString(parentProject)}";

            var response = await _http.GetAsync<List<MonthlyTimeRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthlyTimeDto>>>(response);
            var dto = _mapper.Map<ApiResponseDto<List<MonthlyTimeDto>>>(response);
            return ApiResponseDto<List<MonthlyTimeDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<MonthlyTimeDto>> GetMonthlyTimeByIdAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            var url = string.Format(PactApiEndpoints.GetMonthlyTimeById,
                Uri.EscapeDataString(pactStaffId),
                Uri.EscapeDataString(timeCode),
                month,
                Uri.EscapeDataString(parentProject));
            var response = await _http.GetAsync<MonthlyTimeRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(response);
            var dto = _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(response);
            return ApiResponseDto<MonthlyTimeDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<MonthlyTimeDto>> CreateMonthlyTimeAsync(MonthlyTimeDto dto)
        {
            var request = _mapper.Map<MonthlyTimeReq>(dto);
            var response = await _http.PostAsync<MonthlyTimeReq, MonthlyTimeRes>(PactApiEndpoints.CreateMonthlyTime, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(response);
            var mapped = _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(response);
            return ApiResponseDto<MonthlyTimeDto>.FailureResponse(mapped.Errors, mapped.Meta);
        }

        public async Task<ApiResponseDto<MonthlyTimeDto>> UpdateMonthlyTimeAsync(MonthlyTimeDto dto)
        {
            var request = _mapper.Map<MonthlyTimeReq>(dto);
            var response = await _http.PutAsync<MonthlyTimeReq, MonthlyTimeRes>(PactApiEndpoints.UpdateMonthlyTime, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(response);
            var mapped = _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(response);
            return ApiResponseDto<MonthlyTimeDto>.FailureResponse(mapped.Errors, mapped.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteMonthlyTimeAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            var url = string.Format(PactApiEndpoints.DeleteMonthlyTime,
                Uri.EscapeDataString(pactStaffId),
                Uri.EscapeDataString(timeCode),
                month,
                Uri.EscapeDataString(parentProject));
            var response = await _http.DeleteAsync<bool?>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);
            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
