using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class PlanStaffZTCodeService : IPlanStaffZTCodeService
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IPactApiClient _pactClient;

        public PlanStaffZTCodeService(IFpsApiClient fpsClient, IPactApiClient pactClient)
        {
            _fpsClient = fpsClient;
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<IEnumerable<FpsJobCodeZtDto>>> GetZtJobCodesAsync()
        {
            return await _pactClient.PactJobCode.GetZtJobCodesAsync();
        }

        public async Task<ApiResponseDto<StaffWorkgroupLookupDto>> GetStaffSummaryByIdAsync(string staffId)
        {
            return await _fpsClient.FpsStaffJob.GetStaffSummaryByIdAsync(staffId);
        }

        public async Task<ApiResponseDto<double>> GetZtTotalHoursByStaffIdAsync(string staffId)
        {
            return await _fpsClient.FpsStaffJob.GetZtTotalHoursByStaffIdAsync(staffId);
        }

        public async Task<ApiResponseDto<List<StaffJobZtViewDto>>> GetZtStaffJobsByStaffIdPagedAsync(QueryParameters<string> query, string staffId)
        {
            return await _fpsClient.FpsStaffJob.GetZtStaffJobsByStaffIdPagedAsync(query, staffId);
        }

        public async Task<ApiResponseDto<StaffJobZtViewDto>> GetZtStaffJobDetailsByIdAsync(string staffId, string jobCode)
        {
            return await _fpsClient.FpsStaffJob.GetZtStaffJobDetailsByIdAsync(staffId, jobCode);
        }

        public async Task<ApiResponseDto<List<StaffJobViewDto>>> GetStaffJobsByJobCodeAsync(QueryParameters<string> query, string jobCode)
        {
            return await _fpsClient.FpsStaffJob.GetAllStaffJobAsync(query, jobCode);
        }

        public async Task<ApiResponseDto<StaffJobDto>> GetStaffJobAsync(string staffId, string jobCode)
        {
            return await _fpsClient.FpsStaffJob.GetStaffJobByIdAsync(staffId, jobCode);
        }

        public async Task<ApiResponseDto<StaffJobDto>> CreateStaffJobAsync(StaffJobDto staffJob)
        {
            return await _fpsClient.FpsStaffJob.CreateStaffJobAsync(staffJob);
        }

        public async Task<ApiResponseDto<StaffJobDto>> UpdateStaffJobAsync(StaffJobDto staffJob)
        {
            return await _fpsClient.FpsStaffJob.UpdateStaffJobAsync(staffJob);
        }

        public async Task<ApiResponseDto<bool>> DeleteStaffJobAsync(string staffId, string jobCode)
        {
            return await _fpsClient.FpsStaffJob.DeleteStaffJobAsync(staffId, jobCode);
        }

        public async Task<ApiResponseDto<List<StaffJobViewDto>>> GetStaffJobsAllocationByJobCodeWgGradePagedAsync(QueryParameters<string> query, string jobcode, string wgGrade)
        {
            return await _fpsClient.FpsStaffJob.GetStaffJobsAllocationByJobCodeWgGradePagedAsync(query, jobcode, wgGrade);
        }
    }
}
