using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;using Apha.FPSApps.Application.Pagination;
using Microsoft.AspNetCore.Http;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface IPactMonthlyTimeService
    {
        Task<ApiResponseDto<List<MonthlyTimeDto>>> GetLiveAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? timeCode,
            string? pactStaffId,
            string? parentProject,
            double? month);

        Task<ApiResponseDto<MonthlyTimeDto>> GetLiveByKeyAsync(string pactStaffId, string timeCode, double month, string parentProject);
        Task<ApiResponseDto<MonthlyTimeDto>> UpdateLiveAsync(MonthlyTimeDto dto);
        Task<ApiResponseDto<List<ValidationFieldErrorDto>>> ValidateLiveAsync(MonthlyTimeDto dto);
        Task<ApiResponseDto<List<StagingMonthlyTimeDto>>> GetStagingAsync(QueryParameters<string> query, bool? passed);
        Task<ApiResponseDto<StagingMonthlyTimeDto>> GetStagingByIdAsync(int id);
        Task<ApiResponseDto<StagingMonthlyTimeDto>> CreateStagingAsync(StagingMonthlyTimeDto dto);
        Task<ApiResponseDto<StagingMonthlyTimeDto>> UpdateStagingAsync(int id, StagingMonthlyTimeDto dto);
        Task<ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>> BulkUpdateStagingNamesAsync(BulkUpdateStagingMonthlyTimeNamesDto dto);
        Task<ApiResponseDto<bool>> DeleteStagingAsync(int id);
        Task<ApiResponseDto<bool>> DeleteAllStagingByUserAsync();
        Task<ApiResponseDto<bool>> DeleteFailedStagingByUserAsync();
        Task<ApiResponseDto<MonthlyTimeImportResultDto>> ImportMonthlyTimeAsync(IFormFile file, short importType);
        Task<ApiResponseDto<MonthlyTimeValidateResultDto>> ValidateStagingAsync();
        Task<ApiResponseDto<MonthlyTimeMakeLiveResultDto>> MakeLiveAsync();

        Task<ApiResponseDto<List<MonthlyTimeLogDto>>> SearchAsync(
            QueryParameters<string> query,
            MonthlyTimeLogFilterDto filter);
    }
}
