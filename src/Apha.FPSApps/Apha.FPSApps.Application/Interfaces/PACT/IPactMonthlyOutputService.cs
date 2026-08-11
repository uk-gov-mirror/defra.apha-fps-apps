using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Microsoft.AspNetCore.Http;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface IPactMonthlyOutputService
    {
        Task<ApiResponseDto<List<MonthlyOutputLogDto>>> SearchAsync(
            QueryParameters<string> query,
            MonthlyOutputLogFilterDto filter);

        Task<ApiResponseDto<List<PactMonthlyOutputDto>>> GetLiveAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month);

        Task<ApiResponseDto<PactMonthlyOutputDto>> GetLiveByKeyAsync(string testCode, string buyer, double month, string workGroup);
        Task<ApiResponseDto<PactMonthlyOutputDto>> UpdateLiveAsync(PactMonthlyOutputDto dto);
        Task<ApiResponseDto<List<ValidationFieldErrorDto>>> ValidateLiveAsync(PactMonthlyOutputDto dto);
        Task<ApiResponseDto<List<StagingMonthlyOutputDto>>> GetStagingAsync(QueryParameters<string> query, bool? passed);
        Task<ApiResponseDto<StagingMonthlyOutputDto>> GetStagingByIdAsync(int id);
        Task<ApiResponseDto<StagingMonthlyOutputDto>> CreateStagingAsync(StagingMonthlyOutputDto dto);
        Task<ApiResponseDto<StagingMonthlyOutputDto>> UpdateStagingAsync(int id, StagingMonthlyOutputDto dto);
        Task<ApiResponseDto<bool>> DeleteStagingAsync(int id);
        Task<ApiResponseDto<bool>> DeleteAllStagingByUserAsync();
        Task<ApiResponseDto<bool>> DeleteFailedStagingByUserAsync();
        Task<ApiResponseDto<MonthlyOutputImportResultDto>> ImportMonthlyOutputAsync(IFormFile file, short importType);
        Task<ApiResponseDto<MonthlyOutputValidateResultDto>> ValidateStagingAsync();
        Task<ApiResponseDto<MonthlyOutputMakeLiveResultDto>> MakeLiveAsync();
    }
}

