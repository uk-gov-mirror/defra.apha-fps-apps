using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    /// <summary>
    /// Frontend service interface for the Cost Centre maintenance resource.
    /// Mirrors all six async methods on <see cref="Apha.FPSApps.Application.Interfaces.FpsApiClients.IFpsCostCentreApiClient"/>.
    /// Injected into <c>CostCentreMaintenanceController</c> in the FPS area.
    /// </summary>
    public interface ICostCentreService
    {
        /// <summary>
        /// Returns the full list of cost centre workgroup entries for dropdown/lookup population.
        /// </summary>
        Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetAllCostCentresAsync();

        /// <summary>
        /// Returns a paginated, optionally filtered and sorted list of cost centres for the active FPS year.
        /// </summary>
        Task<ApiResponseDto<List<CostCentreDto>>> GetAllCostCentresPagedAsync(QueryParameters<string> query);

        /// <summary>
        /// Returns a single cost centre record identified by <paramref name="costCentreNo"/>.
        /// </summary>
        Task<ApiResponseDto<CostCentreDto>> GetCostCentreByIdAsync(double costCentreNo);

        /// <summary>
        /// Creates a new cost centre record.
        /// </summary>
        Task<ApiResponseDto<CostCentreDto>> CreateCostCentreAsync(CostCentreDto costCentreDto);

        /// <summary>
        /// Updates the cost centre record identified by <paramref name="costCentreNo"/>.
        /// </summary>
        Task<ApiResponseDto<CostCentreDto>> UpdateCostCentreAsync(double costCentreNo, CostCentreDto costCentreDto);

        /// <summary>
        /// Deletes the cost centre record identified by <paramref name="costCentreNo"/>.
        /// </summary>
        Task<ApiResponseDto<bool>> DeleteCostCentreAsync(double costCentreNo);
    }
}
