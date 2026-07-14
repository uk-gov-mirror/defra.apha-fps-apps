using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;

namespace Apha.Costbook.Core.Interfaces
{
    
    public interface IFpsAccountCategoryRepository
    {
        Task<List<FpsAccountCategory>> GetAllForMaintenanceAsync();

        Task<PagedData<FpsAccountCategory>> GetPaginatedAsync(PaginationParameters<string> query);

        Task<FpsAccountCategory?> GetByAccShortNameAsync(string accShortName);

        Task<bool> ExistsAsync(string accShortName);

        Task<FpsAccountCategory> AddAsync(FpsAccountCategory accountCategory);

        Task<FpsAccountCategory> UpdateAsync(FpsAccountCategory accountCategory);

        Task<bool> UpdateCsg7GroupAsync(string accShortName, string? csg7Group);
        
        Task<bool> DeleteAsync(string accShortName);
    }
}
