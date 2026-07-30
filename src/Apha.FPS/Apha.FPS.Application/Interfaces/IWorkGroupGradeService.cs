using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for WorkgroupGrade CRUD and lookup operations.
    /// </summary>
    public interface IWorkGroupGradeService
    {
        /// <summary>Returns a paginated list of WorkgroupGrade records.</summary>
        Task<PaginatedResult<WorkgroupGradeDto>> GetAllWorkgroupGradesPagedAsync(QueryParameters<string> query);

        /// <summary>Returns a single WorkgroupGrade by WgGrade code.</summary>
        Task<WorkgroupGradeDto?> GetByWgGradeAsync(string wgGrade);

        /// <summary>Creates a new WorkgroupGrade record.</summary>
        Task<WorkgroupGradeDto> CreateAsync(WorkgroupGradeDto dto);

        /// <summary>Updates an existing WorkgroupGrade record.</summary>
        Task<WorkgroupGradeDto> UpdateAsync(WorkgroupGradeDto dto);

        /// <summary>Deletes a WorkgroupGrade record by WgGrade code.</summary>
        Task<bool> DeleteAsync(string wgGrade);

        /// <summary>Returns all Grade codes for dropdown population.</summary>
        Task<List<string>> GetAllGradeCodesAsync();

        /// <summary>Returns distinct WorkgroupGrade records for a given workgroup, ordered by WGGrade.</summary>
        Task<List<WorkgroupGradeDto>> GetWorkgroupGradesByWorkGroupAsync(string workGroup);

        // Existing methods for backward compatibility
        Task<PaginatedResult<WorkgroupGradeDto>> GetWorkGroupGradeAsync(QueryParameters<string> query, string profitCentreGrade);
        Task<bool> DeleteWorkGroupGradeAsync(string wgGrade);
    }
}
