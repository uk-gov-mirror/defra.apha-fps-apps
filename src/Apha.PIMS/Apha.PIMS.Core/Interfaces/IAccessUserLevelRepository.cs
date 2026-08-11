using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IAccessUserLevelRepository
    {
        Task<PagedData<AccessUserLevel>> GetPagedAccessUserLevelAllAsync(PaginationParameters<string> query);

        Task<List<AccessUserLevel>> GetBySystemIdAsync(int systemId);

        Task<List<AccessUserLevel>> GetByUserAsync(int systemId, string ntLogin);

        Task<AccessUserLevel?> GetByIdAsync(int systemId, string ntLogin, int accessLevelId);

        Task<AccessUserLevel> AddAsync(AccessUserLevel entity);

        Task<bool> DeleteAsync(int systemId, string ntLogin, int accessLevelId);

        Task<bool> ExistsAsync(int systemId, string ntLogin, int accessLevelId);
    }
}
