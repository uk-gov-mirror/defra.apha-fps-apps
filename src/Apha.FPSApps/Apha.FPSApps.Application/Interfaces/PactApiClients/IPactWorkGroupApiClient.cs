using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactWorkGroupApiClient
    {
        Task<ApiResponseDto<List<string>>> GetAllWorkGroupNamesAsync();
        Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync();
        Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsByProfitCentreForBudgetAsync(string profitCentre);
        Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsByProfitCentreForBudgetPagedAsync(QueryParameters<string> query, string profitCentre);
        Task<ApiResponseDto<List<WorkGroupTimeCodeDto>>> GetPagedWorkGroupTimeCodesAsync(QueryParameters<string> query, string? workGroup, int? monthNumber);
        Task<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>> GetPagedWorkGroupValidTimeCodesAsync(QueryParameters<string> query, string workGroup);
        Task<ApiResponseDto<List<WorkGroupDto>>> GetWorkGroupsByProfitCentreAsync(QueryParameters<string> query, string profitCentre);
        Task<ApiResponseDto<bool>> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag);
        Task<ApiResponseDto<bool>> SetSendEmailForAllWorkGroupsAsync(short flag);
        Task<ApiResponseDto<bool>> UpdateWorkGroupEmailAsync(string workGroupName, short sendEmail, string? emailRecipient);
        Task<ApiResponseDto<WgSummarisedStaffTimeUsageDto>> GetWgSummarisedStaffTimeUsageAsync(QueryParameters<string> query, string staffName);

        // WorkGroup Maintenance (CRUD + lookups)
        Task<ApiResponseDto<List<WorkGroupDto>>> GetPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<WorkGroupDto>> GetByWorkGroupNameAsync(string workGroupName);
        Task<ApiResponseDto<WorkGroupDto>> CreateAsync(WorkGroupDto dto);
        Task<ApiResponseDto<WorkGroupDto>> UpdateAsync(string workGroupName, WorkGroupDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(string workGroupName);
        Task<ApiResponseDto<List<string>>> GetProfitCentresAsync();
        Task<ApiResponseDto<List<OwnerDto>>> GetOwnersAsync();
        Task<ApiResponseDto<List<double?>>> GetCostCentresAsync(string profitCentre);
    }
}
