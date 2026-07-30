using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsMilestoneApiClient
    {
        Task<ApiResponseDto<List<MilestoneDto>>> GetAllMilestonesAsync(QueryParameters<string> query, string project);        
        Task<ApiResponseDto<MilestoneDto>> GetMilestoneAsync(string project, string number);
        Task<ApiResponseDto<MilestoneDto>> SaveMilestoneAsync(string project, MilestoneDto dto);
        Task<ApiResponseDto<MilestoneDto>> UpdateMilestoneAsync(string project, string number, MilestoneDto dto);
        Task<ApiResponseDto<object>> DeleteMilestoneAsync(string project, string number);
        Task<ApiResponseDto<object>> UpdateFormRequiredAsync(string parentProject, bool formRequired);

        Task<ApiResponseDto<List<MilestoneTypeDto>>> GetMilestoneTypesAsync(string? milestoneDeliverable = null);

        Task<ApiResponseDto<List<MilestoneFormDatesDto>>> GetAllMilestoneFormDatesAsync(string parentProject, QueryParameters<string> parameters);
        Task<ApiResponseDto<MilestoneFormDatesDto>> GetMilestoneFormDatesAsync(string parentProject, short year);
        Task<ApiResponseDto<MilestoneFormDatesDto>> SaveMilestoneFormDatesAsync(string parentProject, MilestoneFormDatesDto dto);
        Task<ApiResponseDto<object>> DeleteMilestoneFormDatesAsync(string parentProject, short year);

        Task<ApiResponseDto<List<LogMilestoneDto>>> GetLogMilestonesAsync(QueryParameters<string> parameters, string? project, string? numberPart1, string? numberPart2);

        // Staging / Import

        Task<ApiResponseDto<List<StagingMilestoneDto>>> GetAllStagingRowsAsync(QueryParameters<string> parameters);
        Task<ApiResponseDto<List<StagingMilestoneDto>>> GetStagingRowsAsync(int id);        
        Task<ApiResponseDto<StagingMilestoneDto>> AddStagingRowAsync(StagingMilestoneDto dto, int year);
        Task<ApiResponseDto<StagingMilestoneDto>> UpdateStagingRowAsync(int id, StagingMilestoneDto dto);
        Task<ApiResponseDto<object>> DeleteStagingRowAsync(int id);
        Task<ApiResponseDto<object>> ClearStagingAsync(string project);
        Task<ApiResponseDto<object>> ValidateStagingAsync(string project, string? typeId, bool isDeliverableMode);
        Task<ApiResponseDto<object>> ImportStagingAsync(string project);
        Task<ApiResponseDto<object>> ImportWithOverwriteAsync(string project);
        Task<ApiResponseDto<List<ProjectYearManagerDto>>> GetProjectYearManagersAsync(int year);
        Task<ApiResponseDto<List<MilestoneDto>>> GetPMDMilestonesAsync(QueryParameters<string> query, string project);
    }
}
