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
    public class PactMonthlyOutputApiClient : IPactMonthlyOutputApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactMonthlyOutputApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<MonthlyOutputLogDto>>> SearchAsync(
            QueryParameters<string> query,
            MonthlyOutputLogFilterDto filter)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.SearchMonthlyOutputLog, query);

            if (!string.IsNullOrWhiteSpace(filter.WorkGroup))
                url += $"&workGroup={Uri.EscapeDataString(filter.WorkGroup)}";
            if (!string.IsNullOrWhiteSpace(filter.TestCode))
                url += $"&testCode={Uri.EscapeDataString(filter.TestCode)}";
            if (!string.IsNullOrWhiteSpace(filter.Buyer))
                url += $"&buyer={Uri.EscapeDataString(filter.Buyer)}";
            if (filter.DateImported.HasValue)
                url += $"&dateImported={Uri.EscapeDataString(filter.DateImported.Value.ToString("yyyy-MM-dd"))}";
            if (filter.Month.HasValue)
                url += $"&month={filter.Month.Value}";
            if (!string.IsNullOrWhiteSpace(filter.UserId))
                url += $"&userId={Uri.EscapeDataString(filter.UserId)}";
            if (!string.IsNullOrWhiteSpace(filter.InsertDelete))
                url += $"&insertDelete={Uri.EscapeDataString(filter.InsertDelete)}";

            var response = await _http.GetAsync<List<MonthlyOutputLogRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthlyOutputLogDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthlyOutputLogDto>>>(response);
            return ApiResponseDto<List<MonthlyOutputLogDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
        
        public async Task<ApiResponseDto<List<PactMonthlyOutputDto>>> GetLiveAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedMonthlyOutputLive, query);
            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";
            if (!string.IsNullOrWhiteSpace(testCode))
                url += $"&testCode={Uri.EscapeDataString(testCode)}";
            if (!string.IsNullOrWhiteSpace(buyer))
                url += $"&buyer={Uri.EscapeDataString(buyer)}";
            if (month.HasValue)
                url += $"&month={month.Value}";

            var response = await _http.GetAsync<List<MonthlyOutputRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<PactMonthlyOutputDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<PactMonthlyOutputDto>>>(response);
            return ApiResponseDto<List<PactMonthlyOutputDto>>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<PactMonthlyOutputDto>> GetLiveByKeyAsync(string testCode, string buyer, double month, string workGroup)
        {
            var url = string.Format(PactApiEndpoints.GetMonthlyOutputLiveByKey,
                Uri.EscapeDataString(testCode),
                Uri.EscapeDataString(buyer),
                month,
                Uri.EscapeDataString(workGroup));

            var response = await _http.GetAsync<MonthlyOutputRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<PactMonthlyOutputDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<PactMonthlyOutputDto>>(response);
            return ApiResponseDto<PactMonthlyOutputDto>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<PactMonthlyOutputDto>> UpdateLiveAsync(PactMonthlyOutputDto dto)
        {
            var request = _mapper.Map<MonthlyOutputReq>(dto);
            var response = await _http.PutAsync<MonthlyOutputReq, MonthlyOutputRes>(PactApiEndpoints.UpdateMonthlyOutputLive, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<PactMonthlyOutputDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<PactMonthlyOutputDto>>(response);
            return ApiResponseDto<PactMonthlyOutputDto>.FailureResponse(responseDto.Errors, responseDto.Meta ?? new ApiMetaDto());
        }       

        public async Task<ApiResponseDto<List<StagingMonthlyOutputDto>>> GetStagingAsync(QueryParameters<string> query, bool? passed)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedStagingMonthlyOutput, query);
            if (passed.HasValue)
                url += $"&passed={passed.Value.ToString().ToLowerInvariant()}";

            var response = await _http.GetAsync<List<StagingMonthlyOutputRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<StagingMonthlyOutputDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<StagingMonthlyOutputDto>>>(response);
            return ApiResponseDto<List<StagingMonthlyOutputDto>>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<StagingMonthlyOutputDto>> GetStagingByIdAsync(int id)
        {
            var response = await _http.GetAsync<StagingMonthlyOutputRes>(
                string.Format(PactApiEndpoints.GetStagingMonthlyOutputById, id));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<StagingMonthlyOutputDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<StagingMonthlyOutputDto>>(response);
            return ApiResponseDto<StagingMonthlyOutputDto>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<StagingMonthlyOutputDto>> CreateStagingAsync(StagingMonthlyOutputDto dto)
        {
            var request = _mapper.Map<StagingMonthlyOutputReq>(dto);
            var response = await _http.PostAsync<StagingMonthlyOutputReq, StagingMonthlyOutputRes>(
                PactApiEndpoints.CreateStagingMonthlyOutput, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<StagingMonthlyOutputDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<StagingMonthlyOutputDto>>(response);
            return ApiResponseDto<StagingMonthlyOutputDto>.FailureResponse(responseDto.Errors, responseDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<StagingMonthlyOutputDto>> UpdateStagingAsync(int id, StagingMonthlyOutputDto dto)
        {
            var request = _mapper.Map<StagingMonthlyOutputReq>(dto);
            var response = await _http.PutAsync<StagingMonthlyOutputReq, StagingMonthlyOutputRes>(
                string.Format(PactApiEndpoints.UpdateStagingMonthlyOutput, id), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<StagingMonthlyOutputDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<StagingMonthlyOutputDto>>(response);
            return ApiResponseDto<StagingMonthlyOutputDto>.FailureResponse(responseDto.Errors, responseDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<bool>> DeleteStagingAsync(int id)
        {
            var response = await _http.DeleteAsync<bool?>(
                string.Format(PactApiEndpoints.DeleteStagingMonthlyOutput, id));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<bool>> DeleteAllStagingByUserAsync()
        {
            var response = await _http.DeleteAsync<bool?>(PactApiEndpoints.DeleteAllStagingMonthlyOutputByUser);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<bool>> DeleteFailedStagingByUserAsync()
        {
            var response = await _http.DeleteAsync<bool?>(PactApiEndpoints.DeleteFailedStagingMonthlyOutputByUser);
            if (response.Success)
            {
                var mappedResponse = _mapper.Map<ApiResponseDto<bool>>(response);
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

        public async Task<ApiResponseDto<MonthlyOutputImportResultDto>> ImportStagingAsync(MonthlyOutputImportReqDto request)
        {
            var req = _mapper.Map<MonthlyOutputImportReq>(request);
            var response = await _http.PostAsync<MonthlyOutputImportReq, MonthlyOutputImportRes>(
                PactApiEndpoints.ImportStagingMonthlyOutput, req);
            if (response.Success)
            {
                var dto = _mapper.Map<MonthlyOutputImportResultDto>(response.Data);
                return ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(dto);
            }

            var failDto = _mapper.Map<ApiResponseDto<MonthlyOutputImportResultDto>>(response);
            return ApiResponseDto<MonthlyOutputImportResultDto>.FailureResponse(failDto.Errors, failDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<MonthlyOutputValidateResultDto>> ValidateStagingAsync()
        {
            var response = await _http.PostAsync<object, MonthlyOutputValidateRes>(
                PactApiEndpoints.ValidateStagingMonthlyOutput, new { });
            if (response.Success)
            {
                var dto = _mapper.Map<MonthlyOutputValidateResultDto>(response.Data);
                return ApiResponseDto<MonthlyOutputValidateResultDto>.SuccessResponse(dto);
            }

            var failDto = _mapper.Map<ApiResponseDto<MonthlyOutputValidateResultDto>>(response);
            return ApiResponseDto<MonthlyOutputValidateResultDto>.FailureResponse(failDto.Errors, failDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<MonthlyOutputMakeLiveResultDto>> MakeLiveAsync()
        {
            var response = await _http.PostAsync<object, MonthlyOutputMakeLiveRes>(
                PactApiEndpoints.MakeLiveMonthlyOutput, new { });
            if (response.Success)
            {
                var dto = _mapper.Map<MonthlyOutputMakeLiveResultDto>(response.Data);
                return ApiResponseDto<MonthlyOutputMakeLiveResultDto>.SuccessResponse(dto);
            }

            var failDto = _mapper.Map<ApiResponseDto<MonthlyOutputMakeLiveResultDto>>(response);
            return ApiResponseDto<MonthlyOutputMakeLiveResultDto>.FailureResponse(failDto.Errors, failDto.Meta ?? new ApiMetaDto());
        }
    }
}

