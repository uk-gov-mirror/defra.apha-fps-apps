using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IMonthlyOutputRepository
    {
        Task<PagedData<MonthlyOutputLog>> GetMonthlyOutputLogAsync(
            PaginationParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete);

        Task<bool> ExistsByTestCodeAndWorkGroupAsync(string testCode, string workGroup);

        // Live record operations
        Task<PagedData<MonthlyOutput>> SearchLiveAsync(
            PaginationParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month);

        Task<MonthlyOutput?> GetLiveByKeyAsync(string testCode, string buyer, double month, string workGroup);
        Task<MonthlyOutput> UpdateLiveAsync(MonthlyOutput monthlyOutput, string originalTestCode, string originalBuyer, double originalMonth, string originalWorkGroup);
        Task<bool> DeleteLiveAsync(string testCode, string buyer, double month, string workGroup);

        // Staging operations
        Task<PagedData<StagingMonthlyOutput>> SearchStagingAsync(
            PaginationParameters<string> query,
            string importedBy,
            bool? passed);

        Task<StagingMonthlyOutput?> GetStagingByIdAsync(int id, string importedBy);
        Task<StagingMonthlyOutput> CreateStagingAsync(StagingMonthlyOutput stagingMonthlyOutput);
        Task<StagingMonthlyOutput> UpdateStagingAsync(StagingMonthlyOutput stagingMonthlyOutput, string importedBy);
        Task<bool> DeleteStagingAsync(int id, string importedBy);
        Task<int> DeleteAllStagingByUserAsync(string importedBy);
        Task<int> DeleteFailedStagingByUserAsync(string importedBy);
        Task<int> ImportStagingAsync(IEnumerable<StagingMonthlyOutput> stagingRows);
        Task<int> RemoveZeroAndNullVolumeRecordsAsync(string importedBy);
        Task<List<StagingMonthlyOutput>> GetStagingRecordsForValidationAsync(string importedBy);
        Task UpdateStagingRecordsAsync(IEnumerable<StagingMonthlyOutput> records);
        Task<bool> HasFailedStagingAsync(string importedBy);
        Task<(int ProcessedCount, int ImportedCount, int FailedCount)> MakeLiveAsync(string importedBy);
        Task<bool> LiveRecordExistsAsync(string testCode, string buyer, double month, string workGroup);
    }
}
