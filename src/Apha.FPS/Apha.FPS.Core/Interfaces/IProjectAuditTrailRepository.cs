using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectAuditTrailRepository
    {
        Task<PagedData<ProjectLog>> GetProjectLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        Task<PagedData<StaffJobLog>> GetStaffJobLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        Task<PagedData<TestRequirementLog>> GetTestRequirementLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        Task<PagedData<AnimalRequestLog>> GetAnimalRequestLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);

        Task<PagedData<AdditionalCostLog>> GetAdditionalCostLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate);
    }
}
