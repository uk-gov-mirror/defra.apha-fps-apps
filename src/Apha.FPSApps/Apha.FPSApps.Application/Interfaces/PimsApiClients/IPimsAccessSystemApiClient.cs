using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    
    public interface IPimsAccessSystemApiClient
    {
        
        Task<ApiResponseDto<List<AccessSystemDto>>> GetAllAsync();

        
        Task<ApiResponseDto<AccessSystemDto>> GetByIdAsync(int systemid);
    }
}
