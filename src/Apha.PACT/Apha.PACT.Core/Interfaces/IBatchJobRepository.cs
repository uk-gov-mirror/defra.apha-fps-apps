using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IBatchJobRepository
    {
        Task<PagedData<BatchJobHistory>> GetBatchJobsHistoryAsync(PaginationParameters<string> query, string jobName);

        Task<bool> CanRunBatchJobAsync(string jobName);

        Task<BatchJobQueue> EnqueueBatchJobAsync(string jobName, string requestedBy, string correlationId, string note);
    }
}
