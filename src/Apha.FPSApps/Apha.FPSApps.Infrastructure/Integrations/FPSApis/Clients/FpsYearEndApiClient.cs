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
    public class FpsYearEndApiClient : IFpsYearEndApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsYearEndApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>> GetYearEndDataSetupBatchJobHistoryAsync(QueryParameters<string> query, string jobName)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetYearEndBatchJobHistory, query);
            url += $"&jobName={Uri.EscapeDataString(jobName)}";
            var response = await _http.GetAsync<List<BatchJobHistoryRes>>(url);

            if (response.Success)
            {
                var dto = _mapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(response);
                var pagination = response.Pagination;
                var result = new PaginatedResult<BatchJobHistoryDto>(
                    dto.Data ?? new List<BatchJobHistoryDto>(),
                    pagination?.TotalRecords ?? 0,
                    pagination?.PageNumber ?? query.Page,
                    pagination?.PageSize ?? query.PageSize);
                return ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.SuccessResponse(result);
            }

            var failDto = _mapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(response);
            return ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.FailureResponse(failDto.Errors, failDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> GetCanInitiateDataSetupRequestAsync(string jobName)
        {
            var url = $"{FpsApiEndpoints.GetCanInitiateDataSetupRequest}?jobName={Uri.EscapeDataString(jobName)}";
            var response = await _http.GetAsync<bool>(url);

            if (response.Success)
                return ApiResponseDto<bool>.SuccessResponse(response.Data);

            var failDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(failDto.Errors, failDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> GetCanApproveOrRejectDataSetupRequestAsync(string jobName)
        {
            var url = $"{FpsApiEndpoints.GetCanApproveDataSetupRequest}?jobName={Uri.EscapeDataString(jobName)}";
            var response = await _http.GetAsync<bool>(url);

            if (response.Success)
                return ApiResponseDto<bool>.SuccessResponse(response.Data);

            var failDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(failDto.Errors, failDto.Meta);
        }
        public async Task<ApiResponseDto<BatchJobQueueDto>> EnqueueYearEndDataSetupInitiationJobAsync(int plannedYear)
        {
            var request = new YearEndDataSetupReq { PlannedYear = plannedYear };
            var response = await _http.PostAsync<YearEndDataSetupReq, BatchJobQueueRes>(
                FpsApiEndpoints.EnqueueYearEndDataSetupInitiationJob, request);

            if (response.Success && response.Data is not null)
                return _mapper.Map<ApiResponseDto<BatchJobQueueDto>>(response);

            var failDto = _mapper.Map<ApiResponseDto<BatchJobQueueDto>>(response);
            return ApiResponseDto<BatchJobQueueDto>.FailureResponse(failDto.Errors, failDto.Meta);
        }

        public async Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerYearEndDataSetupApprovalJobAsync(int plannedYear)
        {
            var request = new YearEndDataSetupReq { PlannedYear = plannedYear };
            var response = await _http.PostAsync<YearEndDataSetupReq, BatchJobEventTriggerRes>(
                FpsApiEndpoints.EnqueueYearEndDataSetupApprovalJob, request);

            if (response.Success && response.Data is not null)
                return _mapper.Map<ApiResponseDto<BatchJobEventTriggerDto>>(response);

            var failDto = _mapper.Map<ApiResponseDto<BatchJobEventTriggerDto>>(response);
            return ApiResponseDto<BatchJobEventTriggerDto>.FailureResponse(failDto.Errors, failDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> EnqueueYearEndDataSetupRejectJobAsync(int plannedYear)
        {
            var request = new YearEndDataSetupReq { PlannedYear = plannedYear };
            var response = await _http.PostAsync<YearEndDataSetupReq, bool>(
                FpsApiEndpoints.EnqueueYearEndDataSetupRejectJob, request);

            if (response.Success)
                return ApiResponseDto<bool>.SuccessResponse(response.Data); 

            var failDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(failDto.Errors, failDto.Meta);
        }
    }
}
