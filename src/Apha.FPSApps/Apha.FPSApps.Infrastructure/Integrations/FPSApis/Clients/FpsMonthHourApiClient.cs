using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsMonthHourApiClient : IFpsMonthHourApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsMonthHourApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<MonthHourDto>>> GetAllMonthHourAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedMonthHours, query);
            var response = await _http.GetAsync<List<MonthHourRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(response);
            return ApiResponseDto<List<MonthHourDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<IEnumerable<MonthHourDto>>> GetMonthHoursByYearAsync(short year)
        {
            var url = string.Format(FpsApiEndpoints.GetMonthHoursByYear, year);
            var response = await _http.GetAsync<List<MonthHourRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<IEnumerable<MonthHourDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<IEnumerable<MonthHourDto>>>(response);
            return ApiResponseDto<IEnumerable<MonthHourDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<IEnumerable<short>>> GetDistinctYearsAsync()
        {
            var response = await _http.GetAsync<List<short>>(FpsApiEndpoints.GetDistinctMonthHourYears);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<IEnumerable<short>>>(response);

            var dto = _mapper.Map<ApiResponseDto<IEnumerable<short>>>(response);
            return ApiResponseDto<IEnumerable<short>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<YearEndMonthHourDto>>> GetYearEndMonthHoursAsync()
        {
            var response = await _http.GetAsync<List<YearEndMonthHourRes>>(FpsApiEndpoints.GetYearEndMonthHours);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<YearEndMonthHourDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<YearEndMonthHourDto>>>(response);
            return ApiResponseDto<List<YearEndMonthHourDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<MonthHourDto>> SaveMonthHourAsync(MonthHourDto monthHourDto)
        {
            var request = _mapper.Map<MonthHourReq>(monthHourDto);
            var response = await _http.PostAsync<MonthHourReq, MonthHourRes>(FpsApiEndpoints.SaveMonthHour, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<MonthHourDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<MonthHourDto>>(response);
            return ApiResponseDto<MonthHourDto>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
