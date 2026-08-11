using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IYearEndRepository
    {
        Task<PagedData<BatchJobHistory>> GetBatchJobsHistoryAsync(PaginationParameters<string> query, string jobName);

        Task<bool> CanInitiateYearEndDataSetupRequestAsync(string jobName);
       
        Task<bool> CanApproveOrRejectYearEndDataSetupRequestAsync(string jobName);
        
        Task<string> GetYearEndDataSetupRequestInitiatorAsync(string jobName);
        
        Task<BatchJobQueue> EnqueueDataSetupInitiationBatchJobAsync(string jobName, string requestedBy, string correlationId, string note);
        
        Task<BatchJobQueue> EnqueueDataSetupApprovalBatchJobAsync(string jobName, string requestedBy, string correlationId, string note);
        
        Task<BatchJobQueue> EnqueueDataSetupRejectBatchJobAsync(string jobName, string requestedBy, string correlationId, string note);
    }
}
