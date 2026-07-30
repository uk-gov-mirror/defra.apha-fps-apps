using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// API client interface for the Resource Management Re-plan screen (frmRM_RePlan).
    /// </summary>
    public interface IFpsResourceMgmtReplanApiClient
    {
        /// <summary>Returns a paged list of re-plan grid rows for the given workgroup.</summary>
        Task<ApiResponseDto<List<ResourceMgmtReplanViewDto>>> GetRePlanGridAsync(string workGroup, QueryParameters<string> query);

        /// <summary>Returns a paged list of all-time staff jobs for the given job code and workgroup grade.</summary>
        Task<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>> GetStaffJobsAsync(string jobCode, string wgGrade, QueryParameters<string> query);

        /// <summary>Returns the currently staged re-plan rows for the given job code and workgroup grade.</summary>
        Task<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>> GetStagedRowsAsync(string jobCode, string wgGrade);

        /// <summary>Commits the staged re-plan rows for the given job code and workgroup grade.</summary>
        Task<ApiResponseDto<bool>> CommitReplanAsync(string jobCode, string wgGrade);
    }
}
