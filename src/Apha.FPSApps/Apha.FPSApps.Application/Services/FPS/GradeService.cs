using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service implementation for the Grade maintenance resource.
    /// Thin delegate — all calls forwarded to <see cref="IFpsApiClient.FpsGrade"/> with no business logic.
    /// </summary>
    public class GradeService : IGradeService
    {
        private readonly IFpsApiClient _fpsClient;

        public GradeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<GradeDto>>> GetAllPagedAsync(QueryParameters<string> query)
        {
            return await _fpsClient.FpsGrade.GetAllPagedAsync(query);
        }

        public async Task<ApiResponseDto<GradeDto>> GetByIdAsync(string gradeCode)
        {
            return await _fpsClient.FpsGrade.GetByIdAsync(gradeCode);
        }

        public async Task<ApiResponseDto<GradeDto>> CreateAsync(GradeDto dto)
        {
            return await _fpsClient.FpsGrade.CreateAsync(dto);
        }

        public async Task<ApiResponseDto<GradeDto>> UpdateAsync(string originalCode, GradeDto dto)
        {
            return await _fpsClient.FpsGrade.UpdateAsync(originalCode, dto);
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(string gradeCode)
        {
            return await _fpsClient.FpsGrade.DeleteAsync(gradeCode);
        }
    }
}
