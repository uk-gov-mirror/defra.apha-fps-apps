using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProfitCentreService : IProfitCentreService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProfitCentreService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync()
        {
            return await _fpsClient.FpsProfitCentre.GetProfitCentresAsync();
        }

        public async Task<ApiResponseDto<IEnumerable<ProfitCentreDto>>> GetAllProfitCentresAsync()
        {
            return await _fpsClient.FpsProfitCentre.GetAllProfitCentresAsync();
        }

        public async Task<ApiResponseDto<List<ProfitCentreDto>>> GetAllProfitCentresPagedAsync(QueryParameters<string> query)
        {
            return await _fpsClient.FpsProfitCentre.GetAllProfitCentresPagedAsync(query);
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> GetProfitCentreByIdAsync(string profitCentreId)
        {
            return await _fpsClient.FpsProfitCentre.GetProfitCentreByIdAsync(profitCentreId);
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> CreateProfitCentreAsync(ProfitCentreDto profitCentreDto)
        {
            return await _fpsClient.FpsProfitCentre.CreateProfitCentreAsync(profitCentreDto);
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> UpdateProfitCentreAsync(string profitCentreId, ProfitCentreDto profitCentreDto)
        {
            return await _fpsClient.FpsProfitCentre.UpdateProfitCentreAsync(profitCentreId, profitCentreDto);
        }

        public async Task<ApiResponseDto<bool>> DeleteProfitCentreAsync(string profitCentreId)
        {
            return await _fpsClient.FpsProfitCentre.DeleteProfitCentreAsync(profitCentreId);
        }

        public async Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(
            string profitCentre, int timesheet, int outputsheet, short timesheetLayout)
        {
            return await _fpsClient.FpsProfitCentre.UpdateProfitCentreSettingsAsync(
                profitCentre, timesheet, outputsheet, timesheetLayout);
        }

        public async Task<ApiResponseDto<List<ProfitCentreCostDto>>> GetPagedProfitCenterCostSummaryAsync(
            QueryParameters<string> query, double monthNumber)
            => await _fpsClient.FpsProfitCentre.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);

        public async Task<ApiResponseDto<List<WgStaffPlanViewDto>>> GetPagedWgStaffPlanAsync(
            QueryParameters<string> query, string workGroup)
            => await _fpsClient.FpsProfitCentre.GetPagedWgStaffPlanAsync(query, workGroup);
    }
}
