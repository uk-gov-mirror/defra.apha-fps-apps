using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using static System.Net.WebRequestMethods;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsProjectApiClient
    {
        Task<ApiResponseDto<List<ProjectDto>>> GetAllProjectsAsync();
        Task<ApiResponseDto<List<ProjectDto>>> GetAllProjectsForAllUsersAsync();
        Task<ApiResponseDto<List<ProjectDto>>> GetAllPactProjectsAsync();
        Task<ApiResponseDto<List<ProjectDto>>> GetPagedProjectsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<ProjectSpecificQueryDto>>> GetPagedProjectSpecificQueryAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<ProjectDto>>> GetPagedProjectsByUserAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<ProjectDto>>> GetPagedPactProjectsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<ProjectDto>>> GetPagedPactProjectsByProgramAsync(QueryParameters<string> query, string programNo);
        Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(string parentProject);
        Task<ApiResponseDto<ProjectDto>> CreateProjectAsync(ProjectDto project);
        Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(ProjectDto project);
        Task<ApiResponseDto<ProjectDto>> UpdateProjectAsync(string parentProject, ProjectDto project);
        Task<ApiResponseDto<ProjectDto>> UpdatePactProjectAsync(ProjectDto project);
        Task<ApiResponseDto<ProjectDto>> UpdatePactPortfolioAsync(ProjectDto project);
        Task<ApiResponseDto<ProjectDto>> UpdateFpsPortfolioAsync(ProjectDto project);
        Task<ApiResponseDto<bool>> DeleteProjectAsync(string parentProject);
        Task<ApiResponseDto<bool>> DeleteProjectAndChildrenAsync(string parentProject);
        Task<ApiResponseDto<bool>> ChangeProjectCodeAsync(string oldCode, string newCode);
        Task<ApiResponseDto<bool>> CheckProjectExistsAsync(string code);
        Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProgramAsync(QueryParameters<string> query, string programNo);
        Task<ApiResponseDto<List<ProjectDto>>> GetProjectsByProgramProjectProfitabilityVLAAsync(QueryParameters<string> query, string programNo);
        Task<ApiResponseDto<List<ManagerDto>>> GetManagersAsync();
        Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetCostCentresAsync();
        Task<ApiResponseDto<List<ContractDto>>> GetContractsByUserAsync();
        Task<ApiResponseDto<List<AccountCodeDto>>> GetAccountCodesAsync();
        Task<ApiResponseDto<List<SubAccountDto>>> GetSubAccountsAsync();
        Task<ApiResponseDto<List<ProjectProfitabilityDto>>> GetProjectProfitabilityAsync(QueryParameters<string> query, string programNo, string workTypeFilter);
        Task<ApiResponseDto<List<ProjectProfitabilityDto>>> GetProjectGroupProfitabilityAsync(QueryParameters<string> query, string projectGroup, string workTypeFilter);

        // All four filter params are optional; each maps to a filter dropdown on the VLA page
        // (filterProjectStatus, filterProgram, filterManager, filterCustomer in projectprofitability_vla.js).
        // QueryParameters<string> carries page + pageSize for server-side DataGrid pagination.
        Task<ApiResponseDto<List<ProjectProfitabilityVlaDto>>> GetProjectProfitabilityVlaAsync(
            QueryParameters<string> query,
            string? projectStatus = null,
            string? programNo = null,
            string? manager = null,
            string? customer = null);

        Task<ApiResponseDto<List<ProjectStaffReplanDto>>> GetProjectGroupStaffReplanAsync(QueryParameters<string> query, string workgroup);

        Task<ApiResponseDto<List<ProjectExceptionalCostViewDto>>> GetProjectExceptionalCostsPagedAsync(QueryParameters<string> query);
    }
}
