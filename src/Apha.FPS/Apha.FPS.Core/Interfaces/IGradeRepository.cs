using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for async CRUD and paged query operations on <see cref="Grade"/>.
    /// Implementations must respect the FpsYear query filter applied by FpsDbContext.
    /// </summary>
    public interface IGradeRepository
    {
        /// <summary>Returns a paged, optionally filtered and sorted list of grades for the active FPS year.</summary>
        Task<PagedData<Grade>> GetAllPagedAsync(PaginationParameters<string> query);

        /// <summary>Returns a single grade by its GradeCode, or null if not found in the active FPS year.</summary>
        Task<Grade?> GetByIdAsync(string gradeCode);

        /// <summary>Inserts a new grade record and returns the persisted entity.</summary>
        Task<Grade> CreateAsync(Grade grade);

        /// <summary>Updates an existing grade identified by <paramref name="originalCode"/> and returns the updated entity.</summary>
        Task<Grade> UpdateAsync(string originalCode, Grade grade);

        /// <summary>Deletes the grade with the given GradeCode. Returns true if deleted, false if not found.</summary>
        Task<bool> DeleteAsync(string gradeCode);
    }
}
