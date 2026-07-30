using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service implementation for the Cost Centre maintenance resource.
    /// Thin delegate — all calls forwarded to <see cref="IFpsApiClient.FpsCostCentre"/> with no business logic.
    /// </summary>
    public class CostCentreService : ICostCentreService
    {
        private readonly IFpsApiClient _fpsClient;

        public CostCentreService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetAllCostCentresAsync()
        {
            return await _fpsClient.FpsCostCentre.GetAllCostCentresAsync();
        }

        public async Task<ApiResponseDto<List<CostCentreDto>>> GetAllCostCentresPagedAsync(QueryParameters<string> query)
        {
            return await _fpsClient.FpsCostCentre.GetAllCostCentresPagedAsync(query);
        }

        public async Task<ApiResponseDto<CostCentreDto>> GetCostCentreByIdAsync(double costCentreNo)
        {
            return await _fpsClient.FpsCostCentre.GetCostCentreByIdAsync(costCentreNo);
        }

        public async Task<ApiResponseDto<CostCentreDto>> CreateCostCentreAsync(CostCentreDto costCentreDto)
        {
            return await _fpsClient.FpsCostCentre.CreateCostCentreAsync(costCentreDto);
        }

        public async Task<ApiResponseDto<CostCentreDto>> UpdateCostCentreAsync(double costCentreNo, CostCentreDto costCentreDto)
        {
            return await _fpsClient.FpsCostCentre.UpdateCostCentreAsync(costCentreNo, costCentreDto);
        }

        public async Task<ApiResponseDto<bool>> DeleteCostCentreAsync(double costCentreNo)
        {
            return await _fpsClient.FpsCostCentre.DeleteCostCentreAsync(costCentreNo);
        }
    }
}
