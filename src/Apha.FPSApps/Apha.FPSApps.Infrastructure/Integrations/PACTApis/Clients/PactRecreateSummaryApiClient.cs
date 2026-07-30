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
    public class PactRecreateSummaryApiClient : IPactRecreateSummaryApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactRecreateSummaryApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>> GetRecreateSummaryLogAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetRecreateSummaryLog, query);
            var response = await _http.GetAsync<List<RecreateSummaryLogRes>>(url);

            if (response.Success)
            {
                var dto = _mapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(response);
                var pagination = response.Pagination;
                var result = new PaginatedResult<RecreateSummaryLogDto>(
                    dto.Data ?? new List<RecreateSummaryLogDto>(),
                    pagination?.TotalRecords ?? 0,
                    pagination?.PageNumber ?? query.Page,
                    pagination?.PageSize ?? query.PageSize);
                return ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>.SuccessResponse(result);
            }

            var failDto = _mapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(response);
            return ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>.FailureResponse(failDto.Errors, failDto.Meta);
        }


        public async Task<ApiResponseDto<List<BatchJobHistoryDto>>> GetRecreateSummaryBatchJobHistoryAsync(QueryParameters<string> query, string jobName)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetRecreateSummaryBatchJobHistory, query);
            url += $"&jobName={Uri.EscapeDataString(jobName)}";
            var response = await _http.GetAsync<List<BatchJobHistoryRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(response);
            }
            else
            {
                var failDto = _mapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(response);
                return ApiResponseDto<List<BatchJobHistoryDto>>.FailureResponse(failDto.Errors, failDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> CanRunRecreateSummaryBatchJobAsync(string jobName)
        {
            var url = $"{PactApiEndpoints.CanRunRecreateSummaryBatchJob}?jobName={Uri.EscapeDataString(jobName)}";
            var response = await _http.GetAsync<bool>(url);

            if (response.Success)
                return ApiResponseDto<bool>.SuccessResponse(response.Data);

            var failDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(failDto.Errors, failDto.Meta);
        }

        public async Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerRecreateSummariesBatchJobAsync(int month)
        {
            var request = new RecreateSummariesReq { Month = month };
            var response = await _http.PostAsync<RecreateSummariesReq, BatchJobEventTriggerRes>(
                PactApiEndpoints.TriggerRecreateSummariesBatchJob, request);

            if (response.Success && response.Data is not null)
                return _mapper.Map<ApiResponseDto<BatchJobEventTriggerDto>>(response);

            var failDto = _mapper.Map<ApiResponseDto<BatchJobEventTriggerDto>>(response);
            return ApiResponseDto<BatchJobEventTriggerDto>.FailureResponse(failDto.Errors, failDto.Meta);
        }
    }
}
