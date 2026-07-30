using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IBatchJobService
    {
        Task<PaginatedResult<BatchJobHistoryDto>> GetBatchJobsHistoryAsync(QueryParameters<string> query, string jobName);

        Task<bool> CanRunBatchJobAsync(string jobName);

        Task<BatchJobEventTriggerDto> TriggerRecreateSummariesJobAsync(int month, int contextyear, string requestedBy, string correlationId);
    }
}
