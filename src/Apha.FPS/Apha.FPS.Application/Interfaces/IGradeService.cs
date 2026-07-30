using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for Grade maintenance business logic.
    /// All operations respect the FpsYear query filter applied by FpsDbContext.
    /// </summary>
    public interface IGradeService
    {
        /// <summary>
        /// Returns a paginated, optionally filtered and sorted list of grades for the active FPS year.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <returns>Paginated result of GradeDtos.</returns>
        Task<PaginatedResult<GradeDto>> GetAllPagedAsync(QueryParameters<string> query);

        /// <summary>
        /// Returns a single grade by its GradeCode, or null if not found in the active FPS year.
        /// </summary>
        /// <param name="gradeCode">Grade code identifier.</param>
        /// <returns>GradeDto if found; null otherwise.</returns>
        Task<GradeDto?> GetByIdAsync(string gradeCode);

        /// <summary>
        /// Creates a new grade record after validation.
        /// Throws <see cref="ArgumentException"/> if GradeCode is null/empty.
        /// Throws <see cref="InvalidOperationException"/> if a grade with the same GradeCode already exists.
        /// </summary>
        /// <param name="gradeDto">Grade data to create.</param>
        /// <returns>Created GradeDto.</returns>
        Task<GradeDto> CreateAsync(GradeDto gradeDto);

        /// <summary>
        /// Updates an existing grade identified by <paramref name="originalCode"/>.
        /// Throws <see cref="ArgumentException"/> if originalCode or GradeCode is null/empty.
        /// Throws <see cref="KeyNotFoundException"/> if the grade does not exist.
        /// Throws <see cref="InvalidOperationException"/> if the new GradeCode conflicts with an existing grade.
        /// </summary>
        /// <param name="originalCode">Original GradeCode to identify the record.</param>
        /// <param name="gradeDto">Grade data to update (may contain new GradeCode).</param>
        /// <returns>Updated GradeDto.</returns>
        Task<GradeDto> UpdateAsync(string originalCode, GradeDto gradeDto);

        /// <summary>
        /// Deletes the grade with the given GradeCode.
        /// Throws <see cref="ArgumentException"/> if gradeCode is null/empty.
        /// Throws <see cref="KeyNotFoundException"/> if the grade does not exist.
        /// Throws <see cref="InvalidOperationException"/> if the grade is referenced by Division Grade, RC Grade (ProfitCentreGrade), or WG Grade (WorkgroupGrade) records.
        /// </summary>
        /// <param name="gradeCode">GradeCode of the grade to delete.</param>
        /// <returns>True if deleted; false if not found.</returns>
        Task<bool> DeleteAsync(string gradeCode);
    }
}
