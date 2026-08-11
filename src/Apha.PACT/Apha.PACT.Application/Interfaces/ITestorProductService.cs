using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface ITestorProductService
    {
        Task<IEnumerable<TestorProductDto>> GetAllTestorProductsAsync();
        Task<PaginatedResult<TestorProductDto>> GetPagedTestOrProductsAsync(QueryParameters<string> query);
        Task<TestorProductDto?> GetTestorProductByIdAsync(string itemCode);
        Task<TestorProductDto> CreateTestorProductAsync(TestorProductDto dto);
        Task<TestorProductDto> UpdateTestorProductAsync(TestorProductDto dto);
        Task<bool> DeleteTestorProductAsync(string itemCode);
        Task<IEnumerable<string>> GetOwnersAsync();

        // TestPriceCheck (frmTestPriceCheck — qryTestPriceZero)
        Task<PaginatedResult<TestPriceCheckDto>> GetTestPriceCheckPagedAsync(
            QueryParameters<string> query,
            string priceFilter,
            string? owner);
        Task<TestPriceCheckDto?> GetTestPriceCheckByKeyAsync(string testCode, string jobCode);
        Task<bool> UpdateTestPriceCheckAsync(string testCode, string jobCode, TestPriceCheckDto dto);

        // TestFeePlan (Plan test-fee report)
        Task<PaginatedResult<TestFeePlanDto>> GetTestSnapshotPagedAsync(QueryParameters<string> query);
    }
}
