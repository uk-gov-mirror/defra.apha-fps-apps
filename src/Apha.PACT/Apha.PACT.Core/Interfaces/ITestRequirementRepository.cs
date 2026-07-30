using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface ITestRequirementRepository
    {
        Task<PagedData<TestRequirement>> GetPagedByTestCodeAsync(PaginationParameters<string> query, string testCode);
        Task<PagedData<TestSupplierView>> GetPagedBySupplierTestCodeAsync(
         PaginationParameters<string> query, string testCode, bool showRejected);
        Task<PagedData<TestRequirementDetail>> GetPagedWithDetailsAsync(PaginationParameters<string> query, string testCode);
        Task<PagedData<TestRequirementDetail>> GetPagedByProjectAsync(PaginationParameters<string> query, string parentProject);
        Task<TestRequirement?> GetByIdAsync(string testCode, string buyer);
        Task<TestRequirementDetail?> GetDetailByIdAsync(string testCode, string buyer);
        Task<TestRequirementDetail?> GetPricingAsync(string testCode, string? projectCode);
        Task<IEnumerable<TestRequirementDetail>> GetAllForExportAsync(string testCode, string? filterJson);
        Task<bool> ExistsAsync(string testCode, string buyer);
        Task<bool> ExistsByTestBuyerCodeAsync(string testBuyerCode);
        Task<bool> ExistsByTestCodeAndBuyerInMonthlyOutputAsync(string testCode, string buyer);
        Task<TestRequirement> AddAsync(TestRequirement entity);
        Task<TestRequirement> UpdateAsync(TestRequirement entity);
        Task<bool> DeleteAsync(string testCode, string buyer);

        // TestReqBreakdown (fps.vtestreqbreakdown)
        Task<PagedData<TestReqBreakdownView>> GetPlannedTestsByWorkgroupAsync(PaginationParameters<string> query);
        Task<PagedData<TestActualBreakdownView>> GetActualsTestsWithPlannedDataByWorkgroupAsync(PaginationParameters<string> query);
    }
}
