using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;

namespace Apha.FPS.Application.Interfaces
{
    public interface IStaffJobService
    {
        Task<PaginatedResult<StaffJobViewDto>> GetJobStaffCostAsync(QueryParameters<string> queryFilter, string jobCode);
        Task<decimal> GetTotalStaffCostAsync(string jobCode);
        Task<List<StaffWorkgroupLookupDto>> GetStaffWorkgroupLookup();
        Task<StaffWorkgroupLookupDto?> GetStaffSummaryByIdAsync(string staffId);
        Task<double> GetZtTotalHoursByStaffIdAsync(string staffId);
        Task<PaginatedResult<StaffJobZtViewDto>> GetZtStaffJobsByStaffIdPagedAsync(QueryParameters<string> query, string staffId);
        Task<StaffJobZtViewDto?> GetZtStaffJobDetailsByIdAsync(string staffId, string jobCode);
        Task<decimal?> GetStaffChargeRate(string staffId, string jobcode);
        Task<StaffJobDto?> GetByIdAsync(string staffId, string jobCode);
        Task<StaffJobViewDto?> GetViewByStaffIdAsync(string staffId, string jobCode);
        Task<StaffJobDto> AddAsync(StaffJobDto staffJob);
        Task<StaffJobDto> UpdateAsync(StaffJobDto staffJob);
        Task<bool> DeleteAsync(string staffId, string jobCode);
        Task<PaginatedResult<StaffResourceUtilisationDto>> GetStaffResourceUtilisationAsync(QueryParameters<string> query, string workgroup);
        Task<PaginatedResult<StaffJobViewDto>> GetStaffJobsAllocationByJobCodeWgGradePagedAsync(QueryParameters<string> query, string jobcode, string wgGrade);
    }
}
