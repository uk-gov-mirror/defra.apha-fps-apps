using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsAccessUserApiClient
    {
        Task<ApiResponseDto<PaginatedResult<AccessUserDto>>> GetPagedAsync(QueryParameters<string> request);

        Task<ApiResponseDto<List<AccessUserDto>>> GetAllAsync();
        Task<ApiResponseDto<List<AccessUserDto>>> GetBySystemIdAsync(int systemid);
        Task<ApiResponseDto<AccessUserDto>> GetByIdAsync(int systemid, string ntlogin);
        Task<ApiResponseDto<AccessUserDto>> CreateAsync(AccessUserDto dto);
        Task<ApiResponseDto<AccessUserDto>> UpdateAsync(int systemid, string ntlogin, AccessUserDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(int systemid, string ntlogin);
    }
}
