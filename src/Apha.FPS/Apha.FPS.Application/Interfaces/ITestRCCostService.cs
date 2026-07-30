using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for component charges per profit centre (TestRCCost) CRUD operations.
    /// Orchestrates repository calls and enforces FK guard checks from SP/VBA logic.
    /// Composite PK on fps.tbltestrccost: TestCode + ProfitCentre + FpsYear.
    /// </summary>
    public interface ITestRCCostService
    {
        Task<PaginatedResult<TestRCCostDto>> GetPagedByTestCodeAsync(QueryParameters<string> query, string testCode);

        Task<IEnumerable<TestRCCostDto>> GetByTestCodeAsync(string testCode);

        Task<TestRCCostDto?> GetByKeyAsync(string testCode, string profitCentre);

        Task<TestRCCostDto> CreateAsync(TestRCCostDto dto);

        Task<TestRCCostDto> UpdateAsync(string testCode, string profitCentre, TestRCCostDto dto);

        Task<bool> DeleteAsync(string testCode, string profitCentre);
    }
}
