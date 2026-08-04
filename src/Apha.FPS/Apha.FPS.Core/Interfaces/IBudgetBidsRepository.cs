using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IBudgetBidsRepository
    {
        Task<List<BidView>> GetBidViewAsync(string workgroup);
        Task<PagedData<BidView>> GetBidViewPagedAsync(PaginationParameters<string> query, string workgroup);
        Task<Bid?> GetBidByIdAsync(string WorkGroupName, string account);
        Task<bool> HasRelatedPurchasesAsync(string WorkGroupName, string account);
        Task<Bid> AddBidAsync(Bid bid);
        Task<Bid> UpdateBidAsync(Bid bid);
        Task<bool> DeleteBidAsync(string WorkGroupName, string account);
        Task<List<AccountCategory>> GetAccountCategoriesAsync();
        Task<PagedData<GenericBidView>> GetGenericBidsPagedAsync(PaginationParameters<string> query);
    }
}
