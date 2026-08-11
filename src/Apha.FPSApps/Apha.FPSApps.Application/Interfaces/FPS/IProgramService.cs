using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IProgramService
    {
        Task<ApiResponseDto<IEnumerable<ProgramDto>>> GetAllProgramsAsync();
        Task<ApiResponseDto<IEnumerable<ProgramDto>>> GetAllProgramsForAllUsersAsync();
        Task<ApiResponseDto<List<ProgramDto>>> GetAllProgramsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<ProgramPlanCostViewDto>>> GetProgramTimeSnapshotAsync(QueryParameters<string> query);
        Task<ApiResponseDto<ProgramDto?>> GetProgramByIdAsync(string programNo);      
        Task<ApiResponseDto<ProgramDto>> AddProgramAsync(ProgramDto programDto);
        Task<ApiResponseDto<ProgramDto>> UpdateProgramAsync(ProgramDto programDto);
        Task<ApiResponseDto<bool>> DeleteProgramAsync(string programNo);
    }
}
