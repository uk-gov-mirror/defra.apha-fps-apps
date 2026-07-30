using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class StaffJobService : IStaffJobService
    {
        private readonly IFpsApiClient _fpsClient;

        public StaffJobService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>> GetStaffWorkgroupLookupAsync()
        {
            var workgroups = await _fpsClient.FpsStaffJob.GetStaffWorkgroupLookupAsync();
            return workgroups;
        }

        public async Task<ApiResponseDto<List<StaffJobViewDto>>> GetAllStaffJobsAsync(QueryParameters<string> staffJobReq, string jobCode)
        {
            var staffJobs = await _fpsClient.FpsStaffJob.GetAllStaffJobAsync(staffJobReq, jobCode);
            return staffJobs;
        }

        public async Task<ApiResponseDto<StaffJobDto>> GetStaffJobByIdAsync(string staffId, string jobCode)
        {
            var staffJob = await _fpsClient.FpsStaffJob.GetStaffJobByIdAsync(staffId, jobCode);
            return staffJob;
        }

        public async Task<ApiResponseDto<StaffJobViewDto?>> GetViewByStaffIdAsync(string staffId, string jobCode)
        {
            var staffJobView = await _fpsClient.FpsStaffJob.GetViewByStaffIdAsync(staffId, jobCode);
            return staffJobView;
        }

        public async Task<ApiResponseDto<decimal?>> GetStaffChargeRate(string staffId, string jobcode)
        {
            var staffJob = await _fpsClient.FpsStaffJob.GetStaffChargeRate(staffId, jobcode);
            return staffJob;
        }

        public async Task<ApiResponseDto<decimal>> GetTotalStaffCostAsync(string jobCode)
        {
            return await _fpsClient.FpsStaffJob.GetTotalStaffCostAsync(jobCode);
        }

        public async Task<ApiResponseDto<StaffJobDto>> CreateStaffJobAsync(StaffJobDto staffJob)
        {
            var result = await _fpsClient.FpsStaffJob.CreateStaffJobAsync(staffJob);
            return result;
        }

        public async Task<ApiResponseDto<StaffJobDto>> UpdateStaffJobAsync(string staffId, StaffJobDto staffJob)
        {
            var result = await _fpsClient.FpsStaffJob.UpdateStaffJobAsync(staffJob);
            return result;
        }

        public async Task<ApiResponseDto<bool>> DeleteStaffJobAsync(string staffId, string jobCode)
        {
            var result = await _fpsClient.FpsStaffJob.DeleteStaffJobAsync(staffId, jobCode);
            return result;
        }

        public async Task<ApiResponseDto<List<StaffResourceUtilisationDto>>> GetStaffResourceUtilisationAsync(QueryParameters<string> query, string workgroup)
        {
            return await _fpsClient.FpsStaffJob.GetStaffResourceUtilisationAsync(query, workgroup);
        }
    }
}
