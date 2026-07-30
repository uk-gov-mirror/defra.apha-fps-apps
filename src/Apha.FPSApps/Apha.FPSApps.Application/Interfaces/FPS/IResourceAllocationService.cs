using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IResourceAllocationService
    {
        Task<ApiResponseDto<List<ResourceStaffAllocationDto>>> GetPagedStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade, QueryParameters<string> query);
        Task<ApiResponseDto<List<ResourceStaffJobDetailDto>>> GetPagedStaffJobDetailsByStaffIdAsync(string staffId, QueryParameters<string> query);
    }
}