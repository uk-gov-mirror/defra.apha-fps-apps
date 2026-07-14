using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;

namespace Apha.Costbook.Application.Interfaces
{
    public interface ICapsStaffService
    {
        Task<List<StaffDto>> GetAllStaffAsync();

        Task<PaginatedResult<StaffDto>> GetPaginatedAsync(QueryParameters<string> queryParameters);

        Task<StaffDto?> GetByMNumberAsync(string mNumber);

        Task<StaffDto> AddStaffAsync(StaffDto dto);
      
        Task<StaffDto> UpdateStaffAsync(string mNumber, StaffDto dto);

        Task DeleteStaffAsync(string mNumber);
    }
}
