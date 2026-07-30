using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for Stage 2 Check Resource Allocation
    /// (frmResourceAllocation) read-only grid data.
    /// </summary>
    public interface IResourceAllocationService
    {
        /// <summary>Returns a paged, sorted and filtered set of staff allocation rows.</summary>
        Task<PaginatedResult<ResourceStaffAllocationDto>> GetPagedStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade, QueryParameters<string> query);

        /// <summary>Returns a paged, sorted and filtered set of distinct job detail rows.</summary>
        Task<PaginatedResult<ResourceStaffJobDetailDto>> GetPagedStaffJobDetailsByStaffIdAsync(string staffId, QueryParameters<string> query);
    }
}
