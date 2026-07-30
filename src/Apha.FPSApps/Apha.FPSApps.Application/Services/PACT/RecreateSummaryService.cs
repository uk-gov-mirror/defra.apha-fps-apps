using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class RecreateSummaryService : IRecreateSummaryService
    {
        private readonly IPactApiClient _pactClient;

        public RecreateSummaryService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>> GetRecreateSummaryLogAsync(QueryParameters<string> query)
            => await _pactClient.PactRecreateSummary.GetRecreateSummaryLogAsync(query);

        public async Task<ApiResponseDto<List<BatchJobHistoryDto>>> GetRecreateSummaryBatchJobHistoryAsync(QueryParameters<string> query, string jobName)
            => await _pactClient.PactRecreateSummary.GetRecreateSummaryBatchJobHistoryAsync(query, jobName);

        public async Task<ApiResponseDto<bool>> CanRunRecreateSummaryBatchJobAsync(string jobName) 
            => await _pactClient.PactRecreateSummary.CanRunRecreateSummaryBatchJobAsync(jobName);

        public async Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerRecreateSummariesBatchJobAsync(int month)
            => await _pactClient.PactRecreateSummary.TriggerRecreateSummariesBatchJobAsync(month);
    }
}
