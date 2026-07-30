using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectRepository
    {
        // ProjectProfitability — project profitability query
        Task<PagedData<ProjectProfitabilityView>> GetProjectProfitabilityAsync(PaginationParameters<string> query, string programNo, string workTypeFilter);
        Task<PagedData<ProjectProfitabilityView>> GetProjectGroupProfitabilityAsync(PaginationParameters<string> query, string projectGroup, string workTypeFilter);

        Task<PagedData<ProjectProfitabilityVlaView>> GetProjectProfitabilityVlaAsync(PaginationParameters<string> query, string? projectStatus = null, string? programNo = null, string? manager = null, string? customer = null);
        Task<IEnumerable<ProjectView>> GetAllProjectsAsync();
        Task<IEnumerable<Project>> GetAllProjectsForAllUsersAsync();
        Task<IEnumerable<PactProjectView>> GetAllPactProjectsAsync();
        Task<PagedData<Project>> GetPagedProjectsAsync(PaginationParameters<string> query);
        Task<PagedData<ProjectView>> GetPagedProjectsByUserAsync(PaginationParameters<string> query);
        Task<PagedData<PactProjectView>> GetPagedPactProjectsAsync(PaginationParameters<string> query);
        Task<PagedData<PactProjectView>> GetPagedPactProjectsByProgramAsync(PaginationParameters<string> query, string programNo);
        Task<Project?> GetProjectByIdAsync(string parentProject);
        Task<Project> CreateProjectAsync(Project project);
        Task<Project> UpdateProjectAsync(Project project);
        Task<Project?> UpdatePactProjectDetailsAsync(Project project);
        Task<Project?> UpdatePactPortfolioDetailsAsync(Project project);
        Task<Project?> UpdateFpsPortfolioDetailsAsync(Project project);
        Task<bool> DeleteProjectAsync(string parentProject);
        Task<bool> HasAssociatedJobCodesAsync(string parentProject);
        Task<PagedData<Project>> GetProjectsByProgramAsync(PaginationParameters<string> query, string programNo);
        Task<PagedData<Project>> GetProjectsByProjectGroupAsync(PaginationParameters<string> query, string projectGroup);
        Task<PagedData<Project>> GetProjectsByProgramProjectProfitabilityVLAAsync(PaginationParameters<string> query, string programNo);
        Task<PagedData<Project>> GetProjectsByProjectGroupProjectProfitabilityVLAAsync(PaginationParameters<string> query, string projectGroup);

        // ProgrammeNewProject operations
        Task<bool> CheckProjectExistsAsync(string newProject);
        Task<bool> CheckProjectExistsInFarmFileAsync(string oldProject);
        Task ChangeProjectCodeAsync(string oldCode, string newCode);
        Task DeleteProjectAndChildrenAsync(string parentProject);

        // Delete guard checks
        Task<bool> HasPlannedTestsAsync(string parentProject);
        Task<bool> HasMonthlyOutputAsync(string parentProject);
        Task<bool> HasMonthlyTimeAsync(string parentProject);
        Task<bool> HasProjectInvoicesAsync(string parentProject);
        Task<bool> HasProjectSubcontractsAsync(string parentProject);

        // Program FK validation (derived from tI_tlkpProject / tU_tlkpProject triggers)
        Task<bool> CheckProgramExistsAsync(string programNo);

        // Resource Replan — staff data for a workgroup, paged/filtered/sorted
        Task<PagedData<ProjectStaffReplanView>> GetProjectStaffReplanAsync(PaginationParameters<string> query, string workgroup);
    }
}
