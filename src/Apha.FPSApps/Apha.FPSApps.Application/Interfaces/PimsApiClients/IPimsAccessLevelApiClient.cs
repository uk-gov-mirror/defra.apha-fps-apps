using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // mirrors AccessLevelController — composite PK (systemid int + accesslevelid int)
    public interface IPimsAccessLevelApiClient
    {
        // GET /api/v1/accesslevel — full lookup list
        Task<ApiResponseDto<List<AccessLevelDto>>> GetAllAsync();

        // GET /api/v1/accesslevel/{systemid:int} — scoped by system
        Task<ApiResponseDto<List<AccessLevelDto>>> GetBySystemIdAsync(int systemid);

        // GET /api/v1/accesslevel/{systemid:int}/{accesslevelid:int} — composite PK get
        Task<ApiResponseDto<AccessLevelDto>> GetByIdAsync(int systemid, int accesslevelid);

        // POST /api/v1/accesslevel
        Task<ApiResponseDto<AccessLevelDto>> CreateAsync(AccessLevelDto dto);

        // PUT /api/v1/accesslevel/{systemid:int}/{accesslevelid:int} — composite PK is authoritative
        Task<ApiResponseDto<AccessLevelDto>> UpdateAsync(int systemid, int accesslevelid, AccessLevelDto dto);

        // DELETE /api/v1/accesslevel/{systemid:int}/{accesslevelid:int}
        Task<ApiResponseDto<bool>> DeleteAsync(int systemid, int accesslevelid);
    }
}
