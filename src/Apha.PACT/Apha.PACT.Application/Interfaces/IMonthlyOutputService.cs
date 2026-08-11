using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IMonthlyOutputService
    {
        Task<PaginatedResult<MonthlyOutputLogDto>> GetMonthlyOutputLogAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete);

        // Live
        Task<PaginatedResult<MonthlyOutputDto>> SearchLiveAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month);

        Task<MonthlyOutputDto?> GetLiveByKeyAsync(string testCode, string buyer, double month, string workGroup);
        Task<MonthlyOutputDto> UpdateLiveAsync(MonthlyOutputDto monthlyOutput);
        Task<bool> DeleteLiveAsync(string testCode, string buyer, double month, string workGroup);

        // Staging
        Task<PaginatedResult<StagingMonthlyOutputDto>> SearchStagingAsync(
            QueryParameters<string> query,
            string importedBy,
            bool? passed);

        Task<StagingMonthlyOutputDto?> GetStagingByIdAsync(int id, string importedBy);
        Task<StagingMonthlyOutputDto> CreateStagingAsync(StagingMonthlyOutputDto stagingMonthlyOutput, string importedBy);
        Task<StagingMonthlyOutputDto> UpdateStagingAsync(StagingMonthlyOutputDto stagingMonthlyOutput, string importedBy);
        Task<bool> DeleteStagingAsync(int id, string importedBy);
        Task<int> DeleteAllStagingByUserAsync(string importedBy);
        Task<int> DeleteFailedStagingByUserAsync(string importedBy);

        // Import / Validate / Make Live
        Task<MonthlyOutputImportResultDto> ImportStagingAsync(MonthlyOutputImportDto request, string importedBy);
        Task<MonthlyOutputValidateResultDto> ValidateStagingAsync(string importedBy);
        Task<MonthlyOutputMakeLiveResultDto> MakeLiveAsync(string importedBy);
    }
}
