using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;

namespace Apha.Costbook.Core.Interfaces
{
    
    public interface ICapsStaffRepository
    {        
        Task<List<Staff>> GetAllStaffAsync();

        Task<PagedData<Staff>> GetPaginatedAsync(PaginationParameters<string> queryFilter);

        Task<Staff?> GetByMNumberAsync(string mNumber);

        Task<bool> ExistsAsync(string mNumber);

        Task<Staff> AddStaffAsync(Staff capsStaff);

        Task<Staff> UpdateStaffAsync(Staff capsStaff);

        Task<bool> DeleteStaffAsync(string mNumber);
    }
}
