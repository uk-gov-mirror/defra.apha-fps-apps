using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// Typed HTTP client interface for the Project Audit Trail feature.
    /// Binds to backend ProjectAuditTrailController at route /api/v1/projectaudittrail.
    /// </summary>
    public interface IFpsProjectAuditTrailApiClient
    {
        // project is required; fromDate and toDate are optional date range filters
        Task<ApiResponseDto<List<ProjectLogDto>>> GetProjectLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);

        // project is required; fromDate and toDate are optional date range filters
        Task<ApiResponseDto<List<StaffJobLogDto>>> GetStaffJobLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);

        // project is required; fromDate and toDate are optional date range filters
        Task<ApiResponseDto<List<TestRequirementLogDto>>> GetTestRequirementLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);

        // project is required; fromDate and toDate are optional date range filters
        Task<ApiResponseDto<List<AnimalRequestLogDto>>> GetAnimalRequestLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);

        // project is required; fromDate and toDate are optional date range filters
        Task<ApiResponseDto<List<AdditionalCostLogDto>>> GetAdditionalCostLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);
    }
}
