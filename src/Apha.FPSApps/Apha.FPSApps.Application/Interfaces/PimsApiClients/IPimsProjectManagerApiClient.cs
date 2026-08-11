using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsProjectManagerApiClient
    {
        Task<ApiResponseDto<List<ProjectManagerDto>>> GetAllProjectManagersAsync(QueryParameters<string>? query = null);

        Task<ApiResponseDto<PaginatedResult<ProjectManagerDto>>> GetPagedProjectManagersAsync(QueryParameters<string> query);

        Task<ApiResponseDto<List<string>>> GetManagerNamesAsync();

        Task<ApiResponseDto<ProjectManagerDto>> GetProjectManagerByNameAsync(string projectManagerName);

        Task<ApiResponseDto<ProjectManagerDto>> CreateProjectManagerAsync(ProjectManagerDto dto);

        Task<ApiResponseDto<ProjectManagerDto>> UpdateProjectManagerAsync(string projectManagerName, ProjectManagerDto dto);

        Task<ApiResponseDto<bool>> DeleteProjectManagerAsync(string projectManagerName);
    }
}
