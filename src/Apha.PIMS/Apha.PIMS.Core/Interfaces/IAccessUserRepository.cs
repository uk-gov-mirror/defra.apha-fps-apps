using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IAccessUserRepository
    {
        Task<PagedData<AccessUser>> GetPagedAsync(PaginationParameters<string> query);

        Task<List<AccessUser>> GetAllAsync();

        Task<List<AccessUser>> GetBySystemIdAsync(int systemid);

        Task<List<AccessUser>> GetByNtLoginAsync(string ntlogin);

        Task<AccessUser?> GetByIdAsync(int systemid, string ntlogin);

        Task<AccessUser> AddAsync(AccessUser entity);

        Task<AccessUser> UpdateAsync(AccessUser entity);

        Task<bool> DeleteAsync(int systemid, string ntlogin);

        Task<bool> ExistsAsync(int systemid, string ntlogin);
    }
}
