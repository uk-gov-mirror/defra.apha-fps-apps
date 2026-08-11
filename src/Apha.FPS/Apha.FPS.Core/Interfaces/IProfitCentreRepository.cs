using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProfitCentreRepository
    {
        Task<List<ProfitCentreView>> GetProfitCentresAsync();
        Task<PagedData<ProfitCentre>> GetAllProfitCentresPagedAsync(PaginationParameters<string> query);
        Task<ProfitCentre?> GetProfitCentreByIdAsync(string profitCentreId);
        Task<ProfitCentre> CreateProfitCentreAsync(ProfitCentre profitCentre);
        Task<ProfitCentre> UpdateProfitCentreAsync(string originalProfitCentreId, ProfitCentre profitCentre);
        Task<bool> DeleteProfitCentreAsync(string profitCentreId);
        Task<bool> ProfitCentreExistsAsync(string profitCentreId);
        Task<bool> HasLinkedGradesAsync(string profitCentreId);
        Task<bool> HasLinkedWorkgroupsAsync(string profitCentreId);
        Task<IEnumerable<ProfitCentre>> GetAllProfitCentresAsync();        
        Task<bool> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetlayout);
        Task<PagedData<ProfitCentreCostSummary>> GetPagedProfitCenterCostSummaryAsync(
            PaginationParameters<string> parameters, double monthNumber);
        Task<PagedData<WgStaffPlanView>> GetPagedWgStaffPlanAsync(
            PaginationParameters<string> query, string workGroup);
    }
}
