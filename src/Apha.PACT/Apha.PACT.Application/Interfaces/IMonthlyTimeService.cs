using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IMonthlyTimeService
    {
        Task<PaginatedResult<MonthlyTimeDto>> SearchLiveAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? timeCode,
            string? pactStaffId,
            string? parentProject,
            double? month);

        Task<MonthlyTimeDto?> GetLiveByKeyAsync(string pactStaffId, string timeCode, double month, string parentProject);
        Task<MonthlyTimeDto> UpdateLiveAsync(MonthlyTimeDto monthlyTime);
        Task<bool> DeleteLiveAsync(string pactStaffId, string timeCode, double month, string parentProject);

        Task<PaginatedResult<StagingMonthlyTimeDto>> SearchStagingAsync(
            QueryParameters<string> query,
            string importedBy,
            bool? passed);

        Task<StagingMonthlyTimeDto?> GetStagingByIdAsync(int id, string importedBy);
        Task<StagingMonthlyTimeDto> CreateStagingAsync(StagingMonthlyTimeDto stagingMonthlyTime, string importedBy);
        Task<StagingMonthlyTimeDto> UpdateStagingAsync(StagingMonthlyTimeDto stagingMonthlyTime, string importedBy);
        Task<BulkUpdateStagingMonthlyTimeNamesResultDto> BulkUpdateStagingNamesAsync(BulkUpdateStagingMonthlyTimeNamesDto request, string importedBy);
        Task<bool> DeleteStagingAsync(int id, string importedBy);
        Task<int> DeleteAllStagingByUserAsync(string importedBy);
        Task<int> DeleteFailedStagingByUserAsync(string importedBy);
        Task<MonthlyTimeImportResultDto> ImportStagingAsync(MonthlyTimeImportDto request, string importedBy);
        Task<MonthlyTimeValidateResultDto> ValidateStagingAsync(string importedBy);
        Task<MonthlyTimeMakeLiveResultDto> MakeLiveAsync(string importedBy);

        Task<PaginatedResult<MonthlyTimeLogDto>> SearchAsync(
            QueryParameters<string> query,
            MonthlyTimeLogFilterDto monthlyTimeLogFilter);
    }
}
