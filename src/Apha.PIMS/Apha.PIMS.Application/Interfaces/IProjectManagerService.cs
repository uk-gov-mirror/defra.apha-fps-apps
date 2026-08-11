using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IProjectManagerService
    {
        Task<List<ProjectManagerDto>> GetAllProjectManagersAsync();

        Task<PaginatedResult<ProjectManagerDto>> GetPagedProjectManagersAsync(QueryParameters<string>? query = null);

        Task<List<string>> GetManagerNamesAsync();

        Task<ProjectManagerDto?> GetProjectManagerByNameAsync(string projectManagerName);

        Task<ProjectManagerDto> CreateProjectManagerAsync(ProjectManagerDto dto);

        Task<ProjectManagerDto> UpdateProjectManagerAsync(ProjectManagerDto dto);

        Task<bool> DeleteProjectManagerAsync(string projectManagerName);

        Task<bool> ProjectManagerExistsAsync(string projectManagerName);
    }
}
