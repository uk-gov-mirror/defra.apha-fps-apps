using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    /// <summary>
    /// Frontend service interface for the Grade maintenance resource.
    /// Mirrors the five async methods on <see cref="Apha.FPSApps.Application.Interfaces.FpsApiClients.IFpsGradeApiClient"/>.
    /// Injected into <c>GradeMaintenanceController</c> in the FPS area.
    /// </summary>
    public interface IGradeService
    {
        /// <summary>
        /// Returns a paginated, optionally filtered and sorted list of grades for the active FPS year.
        /// </summary>
        Task<ApiResponseDto<List<GradeDto>>> GetAllPagedAsync(QueryParameters<string> query);

        /// <summary>
        /// Returns a single grade by its GradeCode for the active FPS year.
        /// </summary>
        Task<ApiResponseDto<GradeDto>> GetByIdAsync(string gradeCode);

        /// <summary>
        /// Creates a new grade record.
        /// </summary>
        Task<ApiResponseDto<GradeDto>> CreateAsync(GradeDto dto);

        /// <summary>
        /// Updates an existing grade record identified by <paramref name="originalCode"/>.
        /// The <paramref name="dto"/> may carry a new GradeCode value to trigger a rename.
        /// </summary>
        Task<ApiResponseDto<GradeDto>> UpdateAsync(string originalCode, GradeDto dto);

        /// <summary>
        /// Deletes the grade with the given GradeCode in the active FPS year.
        /// </summary>
        Task<ApiResponseDto<bool>> DeleteAsync(string gradeCode);
    }
}
