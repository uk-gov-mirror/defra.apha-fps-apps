using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsAccessUserLevelApiClient
    {
        Task<ApiResponseDto<PaginatedResult<AccessUserLevelDto>>> GetPagedAsync(QueryParameters<string> request);

        Task<ApiResponseDto<List<AccessUserLevelDto>>> GetBySystemIdAsync(int systemid);

        Task<ApiResponseDto<List<AccessUserLevelDto>>> GetByUserAsync(int systemid, string ntlogin);

        Task<ApiResponseDto<AccessUserLevelDto>> GetByIdAsync(int systemid, string ntlogin, int accesslevelid);

        Task<ApiResponseDto<AccessUserLevelDto>> CreateAsync(AccessUserLevelDto dto);

        Task<ApiResponseDto<bool>> DeleteAsync(int systemid, string ntlogin, int accesslevelid);
    }
}
