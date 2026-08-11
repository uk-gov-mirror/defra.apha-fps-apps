using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactMonthlyTimeApiClient
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
        Task<ApiResponseDto<List<StagingMonthlyTimeDto>>> GetStagingAsync(QueryParameters<string> query, bool? passed);
        Task<ApiResponseDto<StagingMonthlyTimeDto>> GetStagingByIdAsync(int id);
        Task<ApiResponseDto<StagingMonthlyTimeDto>> CreateStagingAsync(StagingMonthlyTimeDto dto);
        Task<ApiResponseDto<StagingMonthlyTimeDto>> UpdateStagingAsync(int id, StagingMonthlyTimeDto dto);
        Task<ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>> BulkUpdateStagingNamesAsync(BulkUpdateStagingMonthlyTimeNamesDto dto);
        Task<ApiResponseDto<bool>> DeleteStagingAsync(int id);
        Task<ApiResponseDto<bool>> DeleteAllStagingByUserAsync();
        Task<ApiResponseDto<bool>> DeleteFailedStagingByUserAsync();
        Task<ApiResponseDto<MonthlyTimeImportResultDto>> ImportStagingAsync(MonthlyTimeImportReqDto request);
        Task<ApiResponseDto<MonthlyTimeValidateResultDto>> ValidateStagingAsync();
        Task<ApiResponseDto<MonthlyTimeMakeLiveResultDto>> MakeLiveAsync();

        Task<ApiResponseDto<List<MonthlyTimeLogDto>>> SearchAsync(
            QueryParameters<string> query,
            MonthlyTimeLogFilterDto filter);
    }
}
