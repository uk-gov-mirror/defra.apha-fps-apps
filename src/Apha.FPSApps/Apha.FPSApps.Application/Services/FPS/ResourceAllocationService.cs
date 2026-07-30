using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ResourceAllocationService : IResourceAllocationService
    {
        private readonly IFpsApiClient _fpsClient;

        public ResourceAllocationService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<ResourceStaffAllocationDto>>> GetPagedStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade, QueryParameters<string> query)
        {
            return await _fpsClient.FpsResourceAllocation.GetPagedStaffAllocationsByWorkGroupGradeAsync(workGroupGrade, query);
        }

        public async Task<ApiResponseDto<List<ResourceStaffJobDetailDto>>> GetPagedStaffJobDetailsByStaffIdAsync(string staffId, QueryParameters<string> query)
        {
            return await _fpsClient.FpsResourceAllocation.GetPagedStaffJobDetailsByStaffIdAsync(staffId, query);
        }
    }
}