using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProgramRepository
    {
        Task<IEnumerable<Program>> GetAllProgramsAsync();
        Task<IEnumerable<Program>> GetAllProgramsForAllUsers();
        Task<PagedData<Program>> GetAllProgramsAsync(PaginationParameters<string> query);
        Task<PagedData<ProgramPlanCostView>> GetProgramTimeSnapshotAsync(PaginationParameters<string> query);
        Task<Program?> GetProgramByIdAsync(string id);       
        Task<Program> AddProgramAsync(Program entity);
        Task<Program> UpdateProgramAsync(Program entity, string originalProgramNo);
        Task<bool> DeleteProgramAsync(string id);
        Task<bool> HasLinkedProjectsAsync(string programNo);
    }
}
