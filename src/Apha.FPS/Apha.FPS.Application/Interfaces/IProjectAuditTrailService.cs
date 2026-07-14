using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProjectAuditTrailService
    {
        Task<PaginatedResult<ProjectLogDto>> GetProjectLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        Task<PaginatedResult<StaffJobLogDto>> GetStaffJobLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        Task<PaginatedResult<TestRequirementLogDto>> GetTestRequirementLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        Task<PaginatedResult<AnimalRequestLogDto>> GetAnimalRequestLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        Task<PaginatedResult<AdditionalCostLogDto>> GetAdditionalCostLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);
    }
}
