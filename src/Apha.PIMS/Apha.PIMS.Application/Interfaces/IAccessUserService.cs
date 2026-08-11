using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IAccessUserService
    {
        Task<PaginatedResult<AccessUserDto>> GetPagedAsync(QueryParameters<string> query);

        Task<List<AccessUserDto>> GetAllAsync();

        Task<List<AccessUserDto>> GetBySystemIdAsync(int systemid);
        Task<List<AccessUserDto>> GetByNtLoginAsync(string ntlogin);

        Task<AccessUserDto?> GetByIdAsync(int systemid, string ntlogin);

        Task<AccessUserDto> CreateAsync(AccessUserDto dto);

        Task<AccessUserDto> UpdateAsync(AccessUserDto dto);

        Task<bool> DeleteAsync(int systemid, string ntlogin);

        Task<bool> ExistsAsync(int systemid, string ntlogin);
    }
}
