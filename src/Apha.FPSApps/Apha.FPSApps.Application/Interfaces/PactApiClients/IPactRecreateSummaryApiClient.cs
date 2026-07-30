using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactRecreateSummaryApiClient
    {
        Task<ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>> GetRecreateSummaryLogAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<BatchJobHistoryDto>>> GetRecreateSummaryBatchJobHistoryAsync(QueryParameters<string> query, string jobName);
        Task<ApiResponseDto<bool>> CanRunRecreateSummaryBatchJobAsync(string jobName);
        Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerRecreateSummariesBatchJobAsync(int month);
    }
}
