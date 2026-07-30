using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProjectStaffPlanDetailsService
    {
        Task<PaginatedResult<ProjectStaffPlanDetailsViewDto>> GetPagedAsync(QueryParameters<string> query);
    }
}
