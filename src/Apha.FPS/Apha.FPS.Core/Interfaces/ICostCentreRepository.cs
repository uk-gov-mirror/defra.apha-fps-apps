using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for CostCentre CRUD and paged-list operations against fps.costcentre.
    /// </summary>
    public interface ICostCentreRepository
    {
        /// <summary>Returns a paginated list of CostCentre records for the maintenance grid.</summary>
        Task<PagedData<CostCentre>> GetAllPagedAsync(PaginationParameters<string> query);

        /// <summary>Returns a single CostCentre by its composite key (costCentreNo + fpsYear), or null if not found.</summary>
        Task<CostCentre?> GetByIdAsync(double costCentreNo, int fpsYear);

        /// <summary>Inserts a new CostCentre record and returns the persisted entity.</summary>
        Task<CostCentre> CreateAsync(CostCentre entity);

        /// <summary>Updates an existing CostCentre record identified by originalCostCentreNo + fpsYear and returns the updated entity.</summary>
        Task<CostCentre> UpdateAsync(double originalCostCentreNo, int fpsYear, CostCentre entity);

        /// <summary>Deletes the CostCentre row for the given composite key. Returns true if a row was deleted.</summary>
        Task<bool> DeleteAsync(double costCentreNo, int fpsYear);

        /// <summary>Returns true if a CostCentre row with the given composite key already exists.</summary>
        Task<bool> ExistsAsync(double costCentreNo, int fpsYear);
    }
}
