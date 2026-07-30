using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for component charges per profit centre (TestRCCost) CRUD.
    /// Scoped to the fsubTestRCPrice component charges tab use case.
    /// Composite PK on fps.tbltestrccost: TestCode + ProfitCentre + FpsYear.
    /// No infrastructure-specific code — Core layer only.
    /// </summary>
    public interface ITestRCCostRepository
    {
        Task<PagedData<TestRCCost>> GetPagedByTestCodeAsync(PaginationParameters<string> query, string testCode);

        Task<IEnumerable<TestRCCost>> GetByTestCodeAsync(string testCode);

        Task<TestRCCost?> GetByKeyAsync(string testCode, string profitCentre);

        Task<bool> ExistsAsync(string testCode, string profitCentre);

        Task<TestRCCost> AddAsync(TestRCCost testRCCost);

        Task<TestRCCost> UpdateAsync(TestRCCost testRCCost);

        Task<bool> DeleteAsync(string testCode, string profitCentre);
    }
}
