using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IWorkGroupRepository
    {
        Task<IEnumerable<WorkGroup>> GetAllWorkGroupsAsync();
        Task<List<string>> GetAllWorkGroupNamesAsync();
        Task<IEnumerable<SummarisedWgTimeView>> GetSummarisedWorkgroupTimeAsync(string workGroup);
        Task<PactProfitCentreView?> GetProfitCentreAsync(string profitCentre);
        Task<IEnumerable<WorkGroup>> GetWorkGroupsForEmailAsync(string profitCentre);
        Task<IEnumerable<TimeSheetTemplateRow>> GetTimeSheetTemplateAsync(string workGroup, short month, short layout);
        Task<IEnumerable<OutputSheetTemplateRow>> GetOutputSheetTemplateAsync(string workGroup, short month);
        Task<PagedData<WorkGroupTimeCode>> GetWorkGroupTimeCodeAsync(PaginationParameters<string> query, string? workGroup, int? monthNumber);
        Task<PagedData<WorkGroupValidTimeCode>> GetWorkGroupValidTimeCodeAsync(PaginationParameters<string> query, string workGroup);
        Task<PagedData<WorkGroup>> GetWorkGroupsByProfitCentreAsync(PaginationParameters<string> query, string profitCentre);
        Task<List<WorkGroupView>> GetWorkGroupsByProfitCentreForBudgetAsync(string profitCentre);
        Task<PagedData<WorkGroupView>> GetWorkGroupsByProfitCentreForBudgetPagedAsync(PaginationParameters<string> query, string profitCentre);
        Task<bool> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag);
        Task<bool> SetSendEmailForAllWorkGroupsAsync(short flag);
        Task<bool> UpdateWorkGroupEmailAsync(string workGroupName, short sendEmail, string? emailRecipient);
        Task<IEnumerable<WgSummarisedStaffTimeUsageView>> GetWgSummarisedStaffTimeUsageAsync(string staffName);

        // WorkGroup Maintenance CRUD + lookup operations (migrated from FPS).
        Task<PagedData<WorkGroup>> GetPagedAsync(PaginationParameters<string> query);
        Task<WorkGroup?> GetByKeyAsync(string workGroupName);
        Task<WorkGroup> CreateAsync(WorkGroup workGroup);
        Task<WorkGroup> UpdateAsync(string originalWorkGroupName, WorkGroup workGroup);
        Task<bool> DeleteAsync(string workGroupName);
        Task<bool> ExistsAsync(string workGroupName);
        Task<IEnumerable<string>> GetAllProfitCentresAsync();
        Task<IEnumerable<Owner>> GetOwnersAsync();
        Task<IEnumerable<double?>> GetCostCentresByProfitCentreAsync(string profitCentre);
    }
}
