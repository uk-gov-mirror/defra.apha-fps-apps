using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProgramService
    {
        Task<IEnumerable<ProgramDto>> GetAllProgramsAsync();
        Task<IEnumerable<ProgramDto>> GetAllProgramsForAllUsersAsync();
        Task<PaginatedResult<ProgramDto>> GetAllProgramsAsync(QueryParameters<string> query);
        Task<PaginatedResult<ProgramPlanCostDto>> GetProgramTimeSnapshotAsync(QueryParameters<string> query);
        Task<ProgramDto?> GetProgramByIdAsync(string programNo);
        Task<ProgramDto> AddProgramAsync(ProgramDto programDto);
        Task<ProgramDto> UpdateProgramAsync( ProgramDto programDto);
        Task<bool> DeleteProgramAsync(string programNo);
    }
}
