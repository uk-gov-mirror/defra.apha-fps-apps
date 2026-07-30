using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for the Stage 2 Check Resource Allocation (frmResourceAllocation)
    /// read-only grids.
    /// </summary>
    public interface IResourceAllocationRepository
    {
        /// <summary>
        /// Returns a paged, sorted and filtered set of staff allocation rows for the given workgroup grade.
        /// </summary>
        Task<PagedData<ResourceStaffGeneralSummaryRow>> GetPagedStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade, PaginationParameters<string> query);
        /// <summary>
        /// Returns a paged, sorted and filtered set of distinct job detail rows for the given staff member.
        /// </summary>
        Task<PagedData<ResourceStaffJobDetailRow>> GetPagedStaffJobDetailsByStaffIdAsync(string staffId, PaginationParameters<string> query);
    }
}