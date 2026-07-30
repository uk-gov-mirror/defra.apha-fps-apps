using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for project-specific component charges (TestRequirementRCCost) CRUD operations.
    /// Orchestrates repository calls and enforces FK guard checks from SP/VBA logic.
    /// Composite PK on fps.tbltestrequirementrccost: TestCode + Buyer + ProfitCentre + FpsYear.
    /// </summary>
    public interface ITestRequirementRCCostService
    {
        Task<PaginatedResult<TestRequirementRCCostDto>> GetPagedByTestCodeAsync(QueryParameters<string> query, string testCode);

        Task<IEnumerable<TestRequirementRCCostDto>> GetByTestCodeAsync(string testCode);

        Task<TestRequirementRCCostDto?> GetByKeyAsync(string testCode, string buyer, string profitCentre);

        Task<TestRequirementRCCostDto> CreateAsync(TestRequirementRCCostDto dto);

        Task<TestRequirementRCCostDto> UpdateAsync(string testCode, string buyer, string profitCentre, TestRequirementRCCostDto dto);

        Task<bool> DeleteAsync(string testCode, string buyer, string profitCentre);
    }
}
