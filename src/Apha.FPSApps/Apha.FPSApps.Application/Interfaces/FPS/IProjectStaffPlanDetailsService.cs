using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IProjectStaffPlanDetailsService
    {
        Task<ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>> GetPagedAsync(QueryParameters<string> query);
    }
}
