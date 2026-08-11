using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IProjectManagerRepository
    {
        Task<List<ProjectManager>> GetAllProjectManagersAsync();

        Task<PagedData<ProjectManager>> GetPagedProjectManagersAsync(PaginationParameters<string>? query = null);

        Task<List<string>> GetManagerNamesAsync();

        Task<ProjectManager?> GetProjectManagerByNameAsync(string projectManagerName);

        Task<ProjectManager> AddProjectManagerAsync(ProjectManager entity);

        Task<ProjectManager> UpdateProjectManagerAsync(ProjectManager entity);

        Task<bool> DeleteProjectManagerAsync(string projectManagerName);

        Task<bool> ProjectManagerExistsAsync(string projectManagerName);
    }
}
