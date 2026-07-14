using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service implementation for Project Audit Trail.
    /// Thin delegate — every method forwards to IFpsApiClient.FpsProjectAuditTrail.
    /// Contains NO business logic; business logic lives exclusively in the backend service.
    /// </summary>
    public class ProjectAuditTrailService : IProjectAuditTrailService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProjectAuditTrailService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<ProjectLogDto>>> GetProjectLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            return await _fpsClient.FpsProjectAuditTrail.GetProjectLogsAsync(query, project, fromDate, toDate);
        }

        public async Task<ApiResponseDto<List<StaffJobLogDto>>> GetStaffJobLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            return await _fpsClient.FpsProjectAuditTrail.GetStaffJobLogsAsync(query, project, fromDate, toDate);
        }

        public async Task<ApiResponseDto<List<TestRequirementLogDto>>> GetTestRequirementLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            return await _fpsClient.FpsProjectAuditTrail.GetTestRequirementLogsAsync(query, project, fromDate, toDate);
        }

        public async Task<ApiResponseDto<List<AnimalRequestLogDto>>> GetAnimalRequestLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            return await _fpsClient.FpsProjectAuditTrail.GetAnimalRequestLogsAsync(query, project, fromDate, toDate);
        }

        public async Task<ApiResponseDto<List<AdditionalCostLogDto>>> GetAdditionalCostLogsAsync(
            QueryParameters<string> query,
            string project,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            return await _fpsClient.FpsProjectAuditTrail.GetAdditionalCostLogsAsync(query, project, fromDate, toDate);
        }
    }
}
