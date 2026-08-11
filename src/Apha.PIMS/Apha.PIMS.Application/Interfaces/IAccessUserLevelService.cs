using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IAccessUserLevelService
    {
        Task<PaginatedResult<AccessUserLevelDto>> GetPagedAccessUserLevelAllAsync(QueryParameters<string> query);

        Task<List<AccessUserLevelDto>> GetBySystemIdAsync(int systemid);
        Task<List<AccessUserLevelDto>> GetByUserAsync(int systemid, string ntlogin);

        Task<AccessUserLevelDto?> GetByIdAsync(int systemid, string ntlogin, int accesslevelid);

        Task<AccessUserLevelDto> CreateAsync(AccessUserLevelDto dto);

        Task<bool> DeleteAsync(int systemid, string ntlogin, int accesslevelid);

        Task<bool> ExistsAsync(int systemid, string ntlogin, int accesslevelid);
    }
}
