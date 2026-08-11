using Apha.PIMS.Application.Dtos;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IAccessLevelService
    {
        Task<List<AccessLevelDto>> GetAllAsync();
        Task<List<AccessLevelDto>> GetBySystemIdAsync(int systemid);

        Task<AccessLevelDto?> GetByIdAsync(int systemid, int accesslevelid);

        Task<AccessLevelDto> CreateAsync(AccessLevelDto dto);

        Task<AccessLevelDto> UpdateAsync(AccessLevelDto dto);

        Task DeleteAsync(int systemid, int accesslevelid);

        Task<bool> ExistsAsync(int systemid, int accesslevelid);
    }
}
