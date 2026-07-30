using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IAdditionalCostRepository
    {
        Task<PagedData<AdditionalCost>> GetByJobCodeAsync(PaginationParameters<string> query, string jobCode);
        Task<decimal> GetTotalItemCostAsync(string jobCode);
        Task<List<AccountCategory>> GetAccountCategoriesAsync();
        Task<AdditionalCost?> GetByIdAsync(string jobCode, string account, string description);
        Task<AdditionalCost> AddAsync(AdditionalCost additionalCost);
        Task<AdditionalCost> UpdateAsync(AdditionalCost additionalCost, string originalAccount, string originalDescription);
        Task<bool> DeleteAsync(string jobCode, string account, string description);
    }
}
