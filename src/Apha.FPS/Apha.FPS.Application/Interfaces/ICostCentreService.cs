using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for CostCentre CRUD and paged-list operations.
    /// Orchestrates business logic (FK validation, duplicate detection) on top of ICostCentreRepository.
    /// </summary>
    public interface ICostCentreService
    {
        /// <summary>Returns a paginated list of CostCentre records for the maintenance grid.</summary>
        Task<PaginatedResult<CostCentreDto>> GetAllCostCentresPagedAsync(QueryParameters<string> query);

        /// <summary>Returns a single CostCentre by its composite key (costCentreNo + fpsYear), or null if not found.</summary>
        Task<CostCentreDto?> GetCostCentreByIdAsync(double costCentreNo, int fpsYear);

        /// <summary>
        /// Validates that the ProfitCentre exists and the composite key is not a duplicate, then inserts and returns the persisted DTO.
        /// Throws <see cref="ArgumentNullException"/> if dto is null.
        /// Throws <see cref="InvalidOperationException"/> if the key already exists or ProfitCentre FK is invalid.
        /// </summary>
        Task<CostCentreDto> CreateCostCentreAsync(CostCentreDto costCentreDto);

        /// <summary>
        /// Validates that the original record exists and the new ProfitCentre FK is valid, then updates and returns the updated DTO.
        /// Throws <see cref="ArgumentNullException"/> if dto is null.
        /// Throws <see cref="KeyNotFoundException"/> if the original record does not exist.
        /// Throws <see cref="InvalidOperationException"/> if ProfitCentre FK is invalid.
        /// </summary>
        Task<CostCentreDto> UpdateCostCentreAsync(double originalCostCentreNo, int fpsYear, CostCentreDto costCentreDto);

        /// <summary>
        /// Deletes the CostCentre row for the given composite key.
        /// Throws <see cref="KeyNotFoundException"/> if the record does not exist.
        /// Returns true if a row was deleted.
        /// </summary>
        Task<bool> DeleteCostCentreAsync(double costCentreNo, int fpsYear);
    }
}
