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

        public async Task<ApiResponseDto<List<MonthlyTimeDto>>> GetLiveAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? timeCode,
            string? pactStaffId,
            string? parentProject,
            double? month)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedMonthlyTimeLive, query);
            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";
            if (!string.IsNullOrWhiteSpace(timeCode))
                url += $"&timeCode={Uri.EscapeDataString(timeCode)}";
            if (!string.IsNullOrWhiteSpace(pactStaffId))
                url += $"&pactStaffId={Uri.EscapeDataString(pactStaffId)}";
            if (!string.IsNullOrWhiteSpace(parentProject))
                url += $"&parentProject={Uri.EscapeDataString(parentProject)}";
            if (month.HasValue)
                url += $"&month={month.Value}";

            var response = await _http.GetAsync<List<MonthlyTimeRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthlyTimeDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthlyTimeDto>>>(response);
            return ApiResponseDto<List<MonthlyTimeDto>>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<MonthlyTimeDto>> GetLiveByKeyAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            var url = string.Format(PactApiEndpoints.GetMonthlyTimeLiveByKey,
                Uri.EscapeDataString(pactStaffId),
                Uri.EscapeDataString(timeCode),
                month,
                Uri.EscapeDataString(parentProject));
            var response = await _http.GetAsync<MonthlyTimeRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(response);
            return ApiResponseDto<MonthlyTimeDto>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<MonthlyTimeDto>> UpdateLiveAsync(MonthlyTimeDto dto)
        {
            var request = _mapper.Map<MonthlyTimeReq>(dto);
            var response = await _http.PutAsync<MonthlyTimeReq, MonthlyTimeRes>(PactApiEndpoints.UpdateMonthlyTimeLive, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(response);
            return ApiResponseDto<MonthlyTimeDto>.FailureResponse(responseDto.Errors, responseDto.Meta ?? new ApiMetaDto());
        }        

        public async Task<ApiResponseDto<List<StagingMonthlyTimeDto>>> GetStagingAsync(QueryParameters<string> query, bool? passed)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedStagingMonthlyTime, query);
            if (passed.HasValue)
                url += $"&passed={passed.Value.ToString().ToLowerInvariant()}";

            var response = await _http.GetAsync<List<StagingMonthlyTimeRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<StagingMonthlyTimeDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<StagingMonthlyTimeDto>>>(response);
            return ApiResponseDto<List<StagingMonthlyTimeDto>>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<StagingMonthlyTimeDto>> GetStagingByIdAsync(int id)
        {
            var response = await _http.GetAsync<StagingMonthlyTimeRes>(string.Format(PactApiEndpoints.GetStagingMonthlyTimeById, id));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<StagingMonthlyTimeDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<StagingMonthlyTimeDto>>(response);
            return ApiResponseDto<StagingMonthlyTimeDto>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<StagingMonthlyTimeDto>> CreateStagingAsync(StagingMonthlyTimeDto dto)
        {
            var request = _mapper.Map<StagingMonthlyTimeReq>(dto);
            var response = await _http.PostAsync<StagingMonthlyTimeReq, StagingMonthlyTimeRes>(PactApiEndpoints.CreateStagingMonthlyTime, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<StagingMonthlyTimeDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<StagingMonthlyTimeDto>>(response);
            return ApiResponseDto<StagingMonthlyTimeDto>.FailureResponse(responseDto.Errors, responseDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<StagingMonthlyTimeDto>> UpdateStagingAsync(int id, StagingMonthlyTimeDto dto)
        {
            var request = _mapper.Map<StagingMonthlyTimeReq>(dto);
            var response = await _http.PutAsync<StagingMonthlyTimeReq, StagingMonthlyTimeRes>(string.Format(PactApiEndpoints.UpdateStagingMonthlyTime, id), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<StagingMonthlyTimeDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<StagingMonthlyTimeDto>>(response);
            return ApiResponseDto<StagingMonthlyTimeDto>.FailureResponse(responseDto.Errors, responseDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>> BulkUpdateStagingNamesAsync(BulkUpdateStagingMonthlyTimeNamesDto dto)
        {
            var request = _mapper.Map<BulkUpdateStagingMonthlyTimeNamesReq>(dto);
            var response = await _http.PostAsync<BulkUpdateStagingMonthlyTimeNamesReq, BulkUpdateStagingMonthlyTimeNamesRes>(PactApiEndpoints.BulkUpdateStagingMonthlyTimeNames, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>>(response);
            return ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>.FailureResponse(responseDto.Errors, responseDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<bool>> DeleteStagingAsync(int id)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(PactApiEndpoints.DeleteStagingMonthlyTime, id));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<bool>> DeleteAllStagingByUserAsync()
        {
            var response = await _http.DeleteAsync<bool?>(PactApiEndpoints.DeleteAllStagingMonthlyTimeByUser);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<bool>> DeleteFailedStagingByUserAsync()
        {
            var response = await _http.DeleteAsync<bool?>(PactApiEndpoints.DeleteFailedStagingMonthlyTimeByUser);
            if (response.Success)
            {
                var mappedResponse = _mapper.Map<ApiResponseDto<bool>>(response);

                // If no records were deleted (Data is false), add a specific message
                if (!mappedResponse.Data)
                {
                    mappedResponse.Errors = new List<ApiErrorDto>
                    {
                        new ApiErrorDto { Message = "No failed imported records found to delete." }
                    };
                }

                return mappedResponse;
            }

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<MonthlyTimeImportResultDto>> ImportStagingAsync(MonthlyTimeImportReqDto request)
        {
            var req = _mapper.Map<MonthlyTimeImportReq>(request);
            var response = await _http.PostAsync<MonthlyTimeImportReq, MonthlyTimeImportRes>(PactApiEndpoints.ImportStagingMonthlyTime, req);
            if (response.Success)
            {
                var dto = _mapper.Map<MonthlyTimeImportResultDto>(response.Data);
                return ApiResponseDto<MonthlyTimeImportResultDto>.SuccessResponse(dto);
            }

            var failDto = _mapper.Map<ApiResponseDto<MonthlyTimeImportResultDto>>(response);
            return ApiResponseDto<MonthlyTimeImportResultDto>.FailureResponse(failDto.Errors, failDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<MonthlyTimeValidateResultDto>> ValidateStagingAsync()
        {
            var response = await _http.PostAsync<object, MonthlyTimeValidateRes>(PactApiEndpoints.ValidateStagingMonthlyTime, new { });
            if (response.Success)
            {
                var dto = _mapper.Map<MonthlyTimeValidateResultDto>(response.Data);
                return ApiResponseDto<MonthlyTimeValidateResultDto>.SuccessResponse(dto);
            }

            var failDto = _mapper.Map<ApiResponseDto<MonthlyTimeValidateResultDto>>(response);
            return ApiResponseDto<MonthlyTimeValidateResultDto>.FailureResponse(failDto.Errors, failDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<MonthlyTimeMakeLiveResultDto>> MakeLiveAsync()
        {
            var response = await _http.PostAsync<object, MonthlyTimeMakeLiveRes>(PactApiEndpoints.MakeLiveMonthlyTime, new { });
            if (response.Success)
            {
                var dto = _mapper.Map<MonthlyTimeMakeLiveResultDto>(response.Data);
                return ApiResponseDto<MonthlyTimeMakeLiveResultDto>.SuccessResponse(dto);
            }

            var failDto = _mapper.Map<ApiResponseDto<MonthlyTimeMakeLiveResultDto>>(response);
            return ApiResponseDto<MonthlyTimeMakeLiveResultDto>.FailureResponse(failDto.Errors, failDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<List<MonthlyTimeLogDto>>> SearchAsync(
            QueryParameters<string> query,
            MonthlyTimeLogFilterDto filter)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.SearchMonthlyTimeLog, query);

            if (!string.IsNullOrWhiteSpace(filter.WorkGroup))
                url += $"&workGroup={Uri.EscapeDataString(filter.WorkGroup)}";
            if (!string.IsNullOrWhiteSpace(filter.TimeCode))
                url += $"&timeCode={Uri.EscapeDataString(filter.TimeCode)}";
            if (!string.IsNullOrWhiteSpace(filter.PactStaffId))
                url += $"&pactStaffId={Uri.EscapeDataString(filter.PactStaffId)}";
            if (!string.IsNullOrWhiteSpace(filter.ParentProject))
                url += $"&parentProject={Uri.EscapeDataString(filter.ParentProject)}";
            if (filter.DateImported.HasValue)
                url += $"&dateImported={Uri.EscapeDataString(filter.DateImported.Value.ToString("yyyy-MM-dd"))}";
            if (filter.Month.HasValue)
                url += $"&month={filter.Month.Value}";
            if (!string.IsNullOrWhiteSpace(filter.UserId))
                url += $"&userId={Uri.EscapeDataString(filter.UserId)}";
            if (!string.IsNullOrWhiteSpace(filter.InsertDelete))
                url += $"&insertDelete={Uri.EscapeDataString(filter.InsertDelete)}";

            var response = await _http.GetAsync<List<MonthlyTimeLogRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthlyTimeLogDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthlyTimeLogDto>>>(response);
            return ApiResponseDto<List<MonthlyTimeLogDto>>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }
    }
}
