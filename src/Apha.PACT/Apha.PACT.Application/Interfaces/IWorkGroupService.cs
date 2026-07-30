using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IWorkGroupService
    {
        Task<IEnumerable<WorkGroupDto>> GetAllWorkGroupsAsync();
        Task<List<string>> GetAllWorkGroupNamesAsync();
        Task<List<WorkGroupViewDto>> GetWorkGroupsByProfitCentreForBudgetAsync(string profitCentre);
        Task<PaginatedResult<WorkGroupViewDto>> GetWorkGroupsByProfitCentreForBudgetPagedAsync(QueryParameters<string> query, string profitCentre);
        Task<PaginatedResult<WorkGroupTimeCodeDto>> GetWorkGroupTimeCodeAsync(QueryParameters<string> query, string workGroup, int monthNumber);
        Task<PaginatedResult<WorkGroupValidTimeCodeDto>> GetWorkGroupValidTimeCodeAsync(QueryParameters<string> query, string workGroup);
        Task<WgSummarisedStaffTimeUsageDto> GetWgSummarisedStaffTimeUsageAsync(QueryParameters<string> query, string staffName);
        Task<SummarisedWgTimeViewDto> GetSummarisedWorkgroupTimeSummaryAsync(QueryParameters<string> query, string workGroup);
        Task<PaginatedResult<WorkGroupDto>> GetWorkGroupsByProfitCentreAsync(QueryParameters<string> query, string profitCentre);
        Task<bool> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag);
        Task<bool> SetSendEmailForAllWorkGroupsAsync(short flag);
        Task<bool> UpdateWorkGroupEmailAsync(string workGroupName, short sendEmail, string? emailRecipient);

        // WorkGroup Maintenance CRUD + lookup operations (migrated from FPS).
        Task<PaginatedResult<WorkGroupDto>> GetPagedAsync(QueryParameters<string> query);
        Task<WorkGroupDto?> GetByKeyAsync(string workGroupName);
        Task<WorkGroupDto> CreateAsync(WorkGroupDto dto);
        Task<WorkGroupDto> UpdateAsync(string originalWorkGroupName, WorkGroupDto dto);
        Task<bool> DeleteAsync(string workGroupName);
        Task<IEnumerable<string>> GetAllProfitCentresAsync();
        Task<IEnumerable<OwnerDto>> GetOwnersAsync();
        Task<IEnumerable<double?>> GetCostCentresByProfitCentreAsync(string profitCentre);
    }
}
