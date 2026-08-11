using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IYearEndService
    {
        Task<PaginatedResult<BatchJobHistoryDto>> GetBatchJobsHistoryAsync(QueryParameters<string> query, string jobName);

        Task<bool> CanInitiateYearEndDataSetupRequestAsync(string jobName);

        Task<bool> CanApproveOrRejectYearEndDataSetupRequestAsync(string jobName);

        Task<BatchJobQueueDto> EnqueueYearEndDataSetupInitiationJobAsync(int plannedYear, int contextyear, string requestedBy, string correlationId);

        Task<BatchJobEventTriggerDto> EnqueueYearEndDataSetupApprovalJobAsync(int plannedYear, int contextYear, string requestedBy, string correlationId);

        Task<bool> EnqueueYearEndDataSetupRejectJobAsync(int plannedYear, int contextYear, string requestedBy, string correlationId);
    }
}
