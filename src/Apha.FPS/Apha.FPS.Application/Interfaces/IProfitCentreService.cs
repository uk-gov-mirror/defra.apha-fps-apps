using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProfitCentreService
    {
        Task<List<ProfitCentreDto>> GetProfitCentresAsync();
        Task<PaginatedResult<ProfitCentreDto>> GetAllProfitCentresPagedAsync(QueryParameters<string> query);
        Task<ProfitCentreDto?> GetProfitCentreByIdAsync(string profitCentreId);
        Task<ProfitCentreDto> CreateProfitCentreAsync(ProfitCentreDto profitCentreDto);
        Task<ProfitCentreDto> UpdateProfitCentreAsync(string originalProfitCentreId, ProfitCentreDto profitCentreDto);
        Task<bool> DeleteProfitCentreAsync(string profitCentreId);
        Task<IEnumerable<ProfitCentreDto>> GetAllProfitCentresAsync();        
        Task<bool> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetlayout);
        Task<PaginatedResult<ProfitCentreCostDto>> GetPagedProfitCenterCostSummaryAsync(
            QueryParameters<string> query, double monthNumber);
        Task<PaginatedResult<WgStaffPlanViewDto>> GetPagedWgStaffPlanAsync(
            QueryParameters<string> query, string workGroup);
    }
}
