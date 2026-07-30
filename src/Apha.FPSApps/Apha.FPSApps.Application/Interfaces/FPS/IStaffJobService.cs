using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IStaffJobService
    {
        Task<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>> GetStaffWorkgroupLookupAsync();
        Task<ApiResponseDto<List<StaffJobViewDto>>> GetAllStaffJobsAsync(QueryParameters<string> staffJobReq, string jobCode);
        Task<ApiResponseDto<StaffJobDto>> GetStaffJobByIdAsync(string staffId, string jobCode);
        Task<ApiResponseDto<StaffJobViewDto?>> GetViewByStaffIdAsync(string staffId, string jobCode);
        Task<ApiResponseDto<decimal?>> GetStaffChargeRate(string staffId, string jobcode);
        Task<ApiResponseDto<decimal>> GetTotalStaffCostAsync(string jobCode);
        Task<ApiResponseDto<StaffJobDto>> CreateStaffJobAsync(StaffJobDto staffJob);
        Task<ApiResponseDto<StaffJobDto>> UpdateStaffJobAsync(string staffId, StaffJobDto staffJob);
        Task<ApiResponseDto<bool>> DeleteStaffJobAsync(string staffId, string jobCode);
        Task<ApiResponseDto<List<StaffResourceUtilisationDto>>> GetStaffResourceUtilisationAsync(QueryParameters<string> query, string workgroup);
    }
}
