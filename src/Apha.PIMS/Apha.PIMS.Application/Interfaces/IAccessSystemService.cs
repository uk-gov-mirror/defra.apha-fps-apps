using Apha.PIMS.Application.Dtos;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IAccessSystemService
    {
        Task<List<AccessSystemDto>> GetAllAsync();

        Task<AccessSystemDto?> GetByIdAsync(int systemid);

        Task<bool> ExistsAsync(int systemid);
    }
}
