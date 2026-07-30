using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsCostCentreApiClient
    {
        Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetAllCostCentresAsync();

        Task<ApiResponseDto<List<CostCentreDto>>> GetAllCostCentresPagedAsync(QueryParameters<string> query);

        Task<ApiResponseDto<CostCentreDto>> GetCostCentreByIdAsync(double costCentreNo);

        Task<ApiResponseDto<CostCentreDto>> CreateCostCentreAsync(CostCentreDto costCentreDto);

        Task<ApiResponseDto<CostCentreDto>> UpdateCostCentreAsync(double costCentreNo, CostCentreDto costCentreDto);

        Task<ApiResponseDto<bool>> DeleteCostCentreAsync(double costCentreNo);
    }
}
