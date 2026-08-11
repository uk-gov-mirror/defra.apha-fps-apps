using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface ITestCapabilityRepository
    {
        Task<PagedData<TestCapability>> GetPagedByWorkGroupAsync(PaginationParameters<string> query, string? workGroup);
        Task<PagedData<TestCapability>> GetPagedByTestCodeAsync(PaginationParameters<string> query, string? testCode);
        Task<PagedData<TestCapability>> GetPagedTestCapabilityByPortfolioAsync(PaginationParameters<string> query, string? portfolio);
        Task<TestCapability?> GetByIdAsync(string testCode, string workGroup);
        Task<TestCapability?> HasRelatedTestCapabilitiesValidRecordsAsync(string testCode);
        Task<TestCapability> AddAsync(TestCapability entity);
        Task<TestCapability> UpdateAsync(TestCapability entity, string? originalWorkGroup = null);
        Task<bool> DeleteAsync(string testCode, string workGroup);
        Task<bool> ExistsAsync(string testCode, string portfolio);
        Task<List<TestCapability>> GetAllAsync();
        Task<PagedData<WgTestCapabilitiesWithDescription>> GetPagedWgTestCapabilitiesWithDescriptionAsync(PaginationParameters<string> query, string workGroup);

        // Plan CrossTab
        Task BuildTestPlanSummaryAsync();
        Task<CrossTabPagedResult> GetPagedTestPlanCrossTabAsync(PaginationParameters<string> query);
    }
}
