using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IWorkGroupGradeService
    {
        Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkGroupGradeAsync(string profitCentre);
        Task<ApiResponseDto<bool>> DeleteWorkGroupGradeAsync(string wgGrade);
        Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetAllWorkgroupGradesPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<WorkgroupGradeDto>> GetByWgGradeAsync(string wgGrade);
        Task<ApiResponseDto<WorkgroupGradeDto>> CreateAsync(WorkgroupGradeDto dto);
        Task<ApiResponseDto<WorkgroupGradeDto>> UpdateAsync(string wgGrade, WorkgroupGradeDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(string wgGrade);
        Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync();
        Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkgroupGradesByWorkGroupAsync(string workGroup);
    }
}