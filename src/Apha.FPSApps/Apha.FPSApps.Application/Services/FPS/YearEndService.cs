using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class YearEndService : IYearEndService
    {
        private readonly IFpsApiClient _fpsClient;

        public YearEndService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>> GetYearEndDataSetupBatchJobHistoryAsync(QueryParameters<string> query, string jobName)
            => await _fpsClient.FpsYearEnd.GetYearEndDataSetupBatchJobHistoryAsync(query, jobName);

        public async Task<ApiResponseDto<bool>> GetCanInitiateDataSetupRequestAsync(string jobName)
            => await _fpsClient.FpsYearEnd.GetCanInitiateDataSetupRequestAsync(jobName);

        public async Task<ApiResponseDto<bool>> GetCanApproveOrRejectDataSetupRequestAsync(string jobName)
            => await _fpsClient.FpsYearEnd.GetCanApproveOrRejectDataSetupRequestAsync(jobName);

        public async Task<ApiResponseDto<BatchJobQueueDto>> EnqueueYearEndDataSetupInitiationJobAsync(int plannedYear)
            => await _fpsClient.FpsYearEnd.EnqueueYearEndDataSetupInitiationJobAsync(plannedYear);

        public async Task<ApiResponseDto<BatchJobEventTriggerDto>> TriggerYearEndDataSetupApprovalJobAsync(int plannedYear)
            => await _fpsClient.FpsYearEnd.TriggerYearEndDataSetupApprovalJobAsync(plannedYear);

        public async Task<ApiResponseDto<bool>> EnqueueYearEndDataSetupRejectJobAsync(int plannedYear)
           => await _fpsClient.FpsYearEnd.EnqueueYearEndDataSetupRejectJobAsync(plannedYear);
    }
}