using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface ITestorProductRepository
    {
        Task<IEnumerable<TestorProduct>> GetAllTestorProductsAsync();
        Task<PagedData<TestorProduct>> GetPagedTestOrProductsAsync(PaginationParameters<string> parameters);
        Task<TestorProduct?> GetTestOrProductByIdAsync(string itemCode);
        Task<TestorProduct> CreateTestOrProductAsync(TestorProduct entity);
        Task<TestorProduct> UpdateTestOrProductAsync(TestorProduct entity);
        Task<bool> DeleteTestOrProductAsync(string itemCode);
        Task<IEnumerable<string>> GetOwnersAsync();
        Task<Dictionary<string, string?>> GetDescriptionsByCodesAsync(IEnumerable<string> itemCodes);
        Task<Dictionary<string, decimal?>> GetUnitPricesByCodesAsync(IEnumerable<string> itemCodes);
        Task<bool> UpdateUnitPriceByCodeAsync(string itemCode, decimal? unitPrice);

        // TestPriceCheck (frmTestPriceCheck — qryTestPriceZero)
        Task<PagedData<TestPriceCheckView>> GetTestPriceCheckPagedAsync(
            PaginationParameters<string> query,
            string priceFilter,
            string? owner);
        Task<TestPriceCheckView?> GetTestPriceCheckByKeyAsync(string testCode, string jobCode);
        Task<bool> UpdateTestPriceCheckAsync(string testCode, string jobCode, short isDefraProject, decimal? testPrice, decimal? defraUnitPrice);

        // TestSnapshot (Plan test-fee report)
        Task<PagedData<TestFeePlanView>> GetTestSnapshotPagedAsync(PaginationParameters<string> query);
    }
}

