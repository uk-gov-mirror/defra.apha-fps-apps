using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectStaffPlanDetailsRepository
    {
        Task<PagedData<ProjectStaffPlanDetailsView>> GetPagedAsync(PaginationParameters<string> query);
    }
}
