using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProgramService : IProgramService
    {
        private readonly IFpsApiClient _fpsApiClient;

        public ProgramService(IFpsApiClient fpsApiClient)
        {
            _fpsApiClient = fpsApiClient;
        }

        public async Task<ApiResponseDto<IEnumerable<ProgramDto>>> GetAllProgramsAsync()
        {
            return await _fpsApiClient.FpsProgram.GetAllProgramsAsync();
        }

        public async Task<ApiResponseDto<IEnumerable<ProgramDto>>> GetAllProgramsForAllUsersAsync()
        {
            return await _fpsApiClient.FpsProgram.GetAllProgramsForAllUsersAsync();
        }

        public async Task<ApiResponseDto<List<ProgramDto>>> GetAllProgramsAsync(QueryParameters<string> query)
        {
            return await _fpsApiClient.FpsProgram.GetAllProgramsAsync(query);
        }

        public async Task<ApiResponseDto<List<ProgramPlanCostViewDto>>> GetProgramTimeSnapshotAsync(QueryParameters<string> query)
        {
            return await _fpsApiClient.FpsProgram.GetProgramTimeSnapshotAsync(query);
        }

        public async Task<ApiResponseDto<ProgramDto?>> GetProgramByIdAsync(string programNo)
        {
            return await _fpsApiClient.FpsProgram.GetProgramByIdAsync(programNo);
        }
        public async Task<ApiResponseDto<ProgramDto>> AddProgramAsync(ProgramDto programDto)
        {
            return await _fpsApiClient.FpsProgram.AddProgramAsync(programDto);
        }

        public async Task<ApiResponseDto<ProgramDto>> UpdateProgramAsync(ProgramDto programDto)
        {
            return await _fpsApiClient.FpsProgram.UpdateProgramAsync(programDto);
        }

        public async Task<ApiResponseDto<bool>> DeleteProgramAsync(string programNo)
        {
            return await _fpsApiClient.FpsProgram.DeleteProgramAsync(programNo);
        }
    }
}