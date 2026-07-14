using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;

namespace Apha.Costbook.Application.Interfaces
{
    public interface IAccountCategoryMaintenanceService
    {
        Task<List<AccountCategoryMaintenanceDto>> GetAllForMaintenanceAsync();

        Task<PaginatedResult<AccountCategoryMaintenanceDto>> GetPaginatedAsync(QueryParameters<string> query);

        Task<AccountCategoryMaintenanceDto> UpdateCsg7GroupAsync(string accShortName, string? csg7Group);
    }
}
