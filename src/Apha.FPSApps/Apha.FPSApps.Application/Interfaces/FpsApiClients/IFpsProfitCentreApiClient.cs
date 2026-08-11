using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsProfitCentreApiClient
    {
        Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync();
        Task<ApiResponseDto<List<ProfitCentreDto>>> GetAllProfitCentresPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<ProfitCentreDto>> GetProfitCentreByIdAsync(string profitCentreId);
        Task<ApiResponseDto<ProfitCentreDto>> CreateProfitCentreAsync(ProfitCentreDto profitCentreDto);
        Task<ApiResponseDto<ProfitCentreDto>> UpdateProfitCentreAsync(string profitCentreId, ProfitCentreDto profitCentreDto);
        Task<ApiResponseDto<bool>> DeleteProfitCentreAsync(string profitCentreId);
        Task<ApiResponseDto<IEnumerable<ProfitCentreDto>>> GetAllProfitCentresAsync();
        Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetLayout);
        Task<ApiResponseDto<List<ProfitCentreCostDto>>> GetPagedProfitCenterCostSummaryAsync(
            QueryParameters<string> query, double monthNumber);
        Task<ApiResponseDto<List<WgStaffPlanViewDto>>> GetPagedWgStaffPlanAsync(
            QueryParameters<string> query, string workGroup);
    }
}
