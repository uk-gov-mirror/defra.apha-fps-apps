using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients;

public interface ICostBookCapsStaffApiClient
{   
    Task<ApiResponseDto<List<StaffDto>>> GetPaginatedCapsStaffAsync(QueryParameters<string> query);

    Task<ApiResponseDto<StaffDto>> GetCapsStaffByMNumberAsync(string mNumber);

    Task<ApiResponseDto<StaffDto>> AddCapsStaffAsync(StaffDto dto);

    Task<ApiResponseDto<StaffDto>> UpdateCapsStaffAsync(string mNumber, StaffDto dto);

    Task<ApiResponseDto<bool>> DeleteCapsStaffAsync(string mNumber);
}
