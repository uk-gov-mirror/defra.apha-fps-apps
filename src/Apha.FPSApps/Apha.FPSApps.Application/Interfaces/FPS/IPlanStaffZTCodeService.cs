using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IPlanStaffZTCodeService
    {
        Task<ApiResponseDto<IEnumerable<FpsJobCodeZtDto>>> GetZtJobCodesAsync();
        Task<ApiResponseDto<StaffWorkgroupLookupDto>> GetStaffSummaryByIdAsync(string staffId);
        Task<ApiResponseDto<double>> GetZtTotalHoursByStaffIdAsync(string staffId);
        Task<ApiResponseDto<List<StaffJobZtViewDto>>> GetZtStaffJobsByStaffIdPagedAsync(QueryParameters<string> query, string staffId);
        Task<ApiResponseDto<StaffJobZtViewDto>> GetZtStaffJobDetailsByIdAsync(string staffId, string jobCode);
        Task<ApiResponseDto<List<StaffJobViewDto>>> GetStaffJobsByJobCodeAsync(QueryParameters<string> query, string jobCode);
        Task<ApiResponseDto<StaffJobDto>> GetStaffJobAsync(string staffId, string jobCode);
        Task<ApiResponseDto<StaffJobDto>> CreateStaffJobAsync(StaffJobDto staffJob);
        Task<ApiResponseDto<StaffJobDto>> UpdateStaffJobAsync(StaffJobDto staffJob);
        Task<ApiResponseDto<bool>> DeleteStaffJobAsync(string staffId, string jobCode);
        Task<ApiResponseDto<List<StaffJobViewDto>>> GetStaffJobsAllocationByJobCodeWgGradePagedAsync(QueryParameters<string> query, string jobcode, string wgGrade);
    }
}
