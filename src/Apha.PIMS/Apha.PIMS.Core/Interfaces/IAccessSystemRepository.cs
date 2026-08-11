using Apha.PIMS.Core.Entities;

namespace Apha.PIMS.Core.Interfaces
{
    
    public interface IAccessSystemRepository
    {
        Task<List<AccessSystem>> GetAllAsync();

        Task<AccessSystem?> GetByIdAsync(int systemid);

        Task<bool> ExistsAsync(int systemid);
    }
}
