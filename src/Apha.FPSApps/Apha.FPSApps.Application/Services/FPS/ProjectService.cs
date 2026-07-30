using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProjectService : IProjectService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProjectService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetAllPactProjectsAsync()
            => await _fpsClient.FpsProject.GetAllPactProjectsAsync();

        public async Task<ApiResponseDto<List<ProjectDto>>> GetAllProjectsAsync()
            => await _fpsClient.FpsProject.GetAllProjectsAsync();

        public async Task<ApiResponseDto<List<ProjectDto>>> GetAllProjectsForAllUsersAsync()
            => await _fpsClient.FpsProject.GetAllProjectsForAllUsersAsync();

        public async Task<ApiResponseDto<List<ProjectDto>>> GetPagedProjectsAsync(QueryParameters<string> query)
            => await _fpsClient.FpsProject.GetPagedProjectsAsync(query);

        public async Task<ApiResponseDto<List<ProjectDto>>> GetPagedProjectsByUserAsync(QueryParameters<string> query)
            => await _fpsClient.FpsProject.GetPagedProjectsByUserAsync(query);

        public async Task<ApiResponseDto<List<ProjectDto>>> GetPagedPactProjectsAsync(QueryParameters<string> query)
            => await _fpsClient.FpsProject.GetPagedPactProjectsAsync(query);

        public async Task<ApiResponseDto<List<ProjectDto>>> GetPagedPactProjectsByProgramAsync(QueryParameters<string> query, string programNo)
            => await _fpsClient.FpsProject.GetPagedPactProjectsByProgramAsync(query, programNo);

        public async Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(string parentProject)
            => await _fpsClient.FpsProject.GetProjectByIdAsync(parentProject);

        public async Task<ApiResponseDto<ProjectDto>> CreateProjectAsync(ProjectDto project)
            => await _fpsClient.FpsProject.CreateProjectAsync(project);

        public async Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(ProjectDto project)
            => await _fpsClient.FpsProject.UpdateProjectAsync(project);

        public Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(string parentProject, ProjectDto project)
            => _fpsClient.FpsProject.UpdateProjectAsync(parentProject, project);

        public async Task<ApiResponseDto<ProjectDto>> UpdatePactProjectAsync(ProjectDto project)
            => await _fpsClient.FpsProject.UpdatePactProjectAsync(project);

        public async Task<ApiResponseDto<ProjectDto>> UpdatePactPortfolioAsync(ProjectDto project)
            => await _fpsClient.FpsProject.UpdatePactPortfolioAsync(project);

        public async Task<ApiResponseDto<ProjectDto>> UpdateFpsPortfolioAsync(ProjectDto project)
            => await _fpsClient.FpsProject.UpdateFpsPortfolioAsync(project);

        public async Task<ApiResponseDto<bool>> DeleteProjectAsync(string parentProject)
            => await _fpsClient.FpsProject.DeleteProjectAsync(parentProject);

        public async Task<ApiResponseDto<List<StatusDto>>> GetAllStatusesAsync()
            => await _fpsClient.FpsLookup.GetAllStatusesAsync();

        public async Task<ApiResponseDto<List<DiseaseDto>>> GetAllDiseasesAsync()
            => await _fpsClient.FpsLookup.GetAllDiseasesAsync();

        public async Task<ApiResponseDto<List<CustomerDto>>> GetAllCustomersAsync()
            => await _fpsClient.FpsLookup.GetAllCustomersAsync();

        public async Task<ApiResponseDto<List<ContractDto>>> GetAllContractsAsync()
            => await _fpsClient.FpsLookup.GetAllContractsAsync();

        public async Task<ApiResponseDto<List<ContractDto>>> GetContractsByUserAsync()
            => await _fpsClient.FpsProject.GetContractsByUserAsync();

        public async Task<ApiResponseDto<List<ContractDto>>> GetAllPactContractsAsync()
            => await _fpsClient.FpsLookup.GetAllPactContractsAsync();

        public async Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProgramAsync(QueryParameters<string> query, string programNo)
            => await _fpsClient.FpsProject.GetProjectsByProgramAsync(query, programNo);

        public async Task<ApiResponseDto<List<ProjectDto>>> GetProjectLookupAsync()
        {
            var response = await _fpsClient.FpsProject.GetAllProjectsAsync();
            if (!response.Success || response.Data == null)
                return response;

            var lookup = response.Data
                .OrderBy(p => p.ParentProject)
                .Select(p => new ProjectDto { ParentProject = p.ParentProject, Program = p.Program, ProjectGroup = p.ProjectGroup })
                .ToList();

            return ApiResponseDto<List<ProjectDto>>.SuccessResponse(lookup);
        }

        public async Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProjectGroupAsync(QueryParameters<string> query, string projectGroup)
            => await _fpsClient.FpsProjectGroup.GetProjectsByProjectGroupAsync(query, projectGroup);

        public async Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProgramProjectProfitabilityVLAAsync(QueryParameters<string> query, string programNo)
            => await _fpsClient.FpsProject.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo);

        public async Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProjectGroupProjectProfitabilityVLAAsync(QueryParameters<string> query, string projectGroup)
            => await _fpsClient.FpsProjectGroup.GetProjectsByProjectGroupProjectProfitabilityVLAAsync(query, projectGroup);

        public async Task<ApiResponseDto<List<ProjectGroupDto>>> GetAllProjectGroupsAsync()
            => await _fpsClient.FpsLookup.GetAllProjectGroupsAsync();

        // Merged from ProgrammeNewProjectService
        public Task<ApiResponseDto<ProjectDto>> GetProgrammeNewProjectByIdAsync(string parentProject)
            => _fpsClient.FpsProject.GetProjectByIdAsync(parentProject);

        public Task<ApiResponseDto<bool>> DeleteProjectAndChildrenAsync(string parentProject)
            => _fpsClient.FpsProject.DeleteProjectAndChildrenAsync(parentProject);

        public Task<ApiResponseDto<bool>> ChangeProjectCodeAsync(string oldCode, string newCode)
            => _fpsClient.FpsProject.ChangeProjectCodeAsync(oldCode, newCode);

        public Task<ApiResponseDto<bool>> CheckProjectExistsAsync(string code)
            => _fpsClient.FpsProject.CheckProjectExistsAsync(code);

        public Task<ApiResponseDto<List<ManagerDto>>> GetManagersAsync()
            => _fpsClient.FpsProject.GetManagersAsync();

        public Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetCostCentresAsync()
            => _fpsClient.FpsProject.GetCostCentresAsync();

        public Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsAsync()
            => _fpsClient.FpsProjectGroup.GetAllProjectGroupsAsync();

        public Task<ApiResponseDto<List<ProjectGroupDto>>> GetProjectGroupsByUserAsync()
            => _fpsClient.FpsProjectGroup.GetProjectGroupsByUserAsync();

        public Task<ApiResponseDto<List<AccountCodeDto>>> GetAccountCodesAsync()
            => _fpsClient.FpsProject.GetAccountCodesAsync();

        public Task<ApiResponseDto<List<SubAccountDto>>> GetSubAccountsAsync()
            => _fpsClient.FpsProject.GetSubAccountsAsync();

        public Task<ApiResponseDto<List<ProjectProfitabilityDto>>> GetProjectProfitabilityAsync(
            QueryParameters<string> query, string programNo, string workTypeFilter)
            => _fpsClient.FpsProject.GetProjectProfitabilityAsync(query, programNo, workTypeFilter);

        public Task<ApiResponseDto<List<ProjectProfitabilityDto>>> GetProjectGroupProfitabilityAsync(
            QueryParameters<string> query, string projectGroup, string workTypeFilter)
            => _fpsClient.FpsProject.GetProjectGroupProfitabilityAsync(query, projectGroup, workTypeFilter);

        // _fpsClient.FpsProject.GetProjectProfitabilityVlaAsync() — NO business logic.
        // All five params passed through unchanged; four filter params default to null when
        // not supplied by the MVC controller (optional filters on the VLA DataGrid page).
        public Task<ApiResponseDto<List<ProjectProfitabilityVlaDto>>> GetProjectProfitabilityVlaAsync(
            QueryParameters<string> query,
            string? projectStatus = null,
            string? programNo = null,
            string? manager = null,
            string? customer = null)
            => _fpsClient.FpsProject.GetProjectProfitabilityVlaAsync(query, projectStatus, programNo, manager, customer);

        public Task<ApiResponseDto<List<ProjectStaffReplanDto>>> GetProjectGroupStaffReplanAsync(QueryParameters<string> query, string workgroup)
            => _fpsClient.FpsProject.GetProjectGroupStaffReplanAsync(query, workgroup);

    }
}
