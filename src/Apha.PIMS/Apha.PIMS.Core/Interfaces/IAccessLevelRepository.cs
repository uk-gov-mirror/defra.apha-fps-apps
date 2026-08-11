using Apha.PIMS.Core.Entities;

namespace Apha.PIMS.Core.Interfaces
{
   
    public interface IAccessLevelRepository
    {
        Task<List<AccessLevel>> GetAllAsync();

        Task<List<AccessLevel>> GetBySystemIdAsync(int systemid);

        Task<AccessLevel?> GetByIdAsync(int systemid, int accesslevelid);

        Task<AccessLevel> AddAsync(AccessLevel entity);

        Task<AccessLevel> UpdateAsync(AccessLevel entity);

        Task DeleteAsync(int systemid, int accesslevelid);

        Task<bool> ExistsAsync(int systemid, int accesslevelid);
    }
}
