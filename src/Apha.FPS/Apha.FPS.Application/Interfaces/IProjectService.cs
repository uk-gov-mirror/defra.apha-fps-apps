using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
        Task<IEnumerable<ProjectDto>> GetAllProjectsForAllUsersAsync();
        Task<IEnumerable<ProjectDto>> GetAllPactProjectsAsync();
        Task<PaginatedResult<ProjectDto>> GetPagedProjectsAsync(QueryParameters<string> query);
        Task<PaginatedResult<ProjectSpecificQueryDto>> GetPagedProjectSpecificQueryAsync(QueryParameters<string> query);
        Task<PaginatedResult<ProjectDto>> GetPagedProjectsByUserAsync(QueryParameters<string> query);
        Task<PaginatedResult<ProjectDto>> GetPagedPactProjectsAsync(QueryParameters<string> query);
        Task<PaginatedResult<ProjectDto>> GetPagedPactProjectsByProgramAsync(QueryParameters<string> query, string programNo);
        Task<ProjectDto?> GetProjectByIdAsync(string parentProject);
        Task<ProjectDto> CreateProjectAsync(ProjectDto projectDto);
        Task<ProjectDto> UpdateProjectAsync(ProjectDto projectDto);
        Task<ProjectDto?> UpdatePactProjectDetailsAsync(ProjectDto projectDto);
        Task<ProjectDto?> UpdatePactPortfolioDetailsAsync(ProjectDto projectDto);
        Task<ProjectDto?> UpdateFpsPortfolioDetailsAsync(ProjectDto projectDto);
        Task<bool> DeleteProjectAsync(string parentProject);
        Task<PaginatedResult<ProjectDto>> GetProjectsByProgramAsync(QueryParameters<string> query, string programNo);
        Task<PaginatedResult<ProjectDto>> GetProjectsByProjectGroupAsync(QueryParameters<string> query, string projectGroup);
        Task<PaginatedResult<ProjectDto>> GetProjectsByProgramProjectProfitabilityVLAAsync(QueryParameters<string> query, string programNo);
        Task<PaginatedResult<ProjectDto>> GetProjectsByProjectGroupProjectProfitabilityVLAAsync(QueryParameters<string> query, string projectGroup);

        // ProgrammeNewProject operations
        Task<bool> CheckProjectExistsAsync(string newProject);
        Task<bool> CheckProjectExistsInFarmFileAsync(string oldProject);
        Task ChangeProjectCodeAsync(string oldCode, string newCode);
        Task DeleteProjectAndChildrenAsync(string parentProject);

        Task<PaginatedResult<ProjectProfitabilityDto>> GetProjectProfitabilityAsync(QueryParameters<string> query, string programNo, string workTypeFilter);
        Task<PaginatedResult<ProjectProfitabilityDto>> GetProjectGroupProfitabilityAsync(QueryParameters<string> query, string projectGroup, string workTypeFilter);
        Task<PaginatedResult<ProjectProfitabilityVlaDto>> GetProjectProfitabilityVlaAsync(QueryParameters<string> query, string? projectStatus = null, string? programNo = null, string? manager = null, string? customer = null);

        // Resource Replan — staff data for a workgroup, paged/filtered/sorted
        Task<PaginatedResult<ProjectStaffReplanDto>> GetProjectStaffReplanAsync(QueryParameters<string> query, string workgroup);

        // Exceptional (additional) costs — paged/filtered/sorted
        Task<PaginatedResult<ProjectExceptionalCostViewDto>> GetProjectExceptionalCostsPagedAsync(QueryParameters<string> query);
    }
}
