using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class WorkGroupGradeService : IWorkGroupGradeService
    {
        private readonly IFpsApiClient _fpsClient;

        public WorkGroupGradeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkGroupGradeAsync(string profitCentre)
        {
            return await _fpsClient.FpsWorkGroupGrade.GetWorkGroupGradeAsync(new QueryParameters<string>(), profitCentre);
        }

        public async Task<ApiResponseDto<bool>> DeleteWorkGroupGradeAsync(string wgGrade)
        {
            return await _fpsClient.FpsWorkGroupGrade.DeleteWorkGroupGradeAsync(wgGrade);
        }

        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetAllWorkgroupGradesPagedAsync(QueryParameters<string> query)
            => await _fpsClient.FpsWorkGroupGrade.GetAllWorkgroupGradesPagedAsync(query);

        public async Task<ApiResponseDto<WorkgroupGradeDto>> GetByWgGradeAsync(string wgGrade)
            => await _fpsClient.FpsWorkGroupGrade.GetByWgGradeAsync(wgGrade);

        public async Task<ApiResponseDto<WorkgroupGradeDto>> CreateAsync(WorkgroupGradeDto dto)
            => await _fpsClient.FpsWorkGroupGrade.CreateAsync(dto);

        public async Task<ApiResponseDto<WorkgroupGradeDto>> UpdateAsync(string wgGrade, WorkgroupGradeDto dto)
            => await _fpsClient.FpsWorkGroupGrade.UpdateAsync(wgGrade, dto);

        public async Task<ApiResponseDto<bool>> DeleteAsync(string wgGrade)
            => await _fpsClient.FpsWorkGroupGrade.DeleteAsync(wgGrade);

        public async Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync()
            => await _fpsClient.FpsWorkGroupGrade.GetAllGradeCodesAsync();

        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkgroupGradesByWorkGroupAsync(string workGroup)
            => await _fpsClient.FpsWorkGroupGrade.GetWorkgroupGradesByWorkGroupAsync(workGroup);
    }
}
