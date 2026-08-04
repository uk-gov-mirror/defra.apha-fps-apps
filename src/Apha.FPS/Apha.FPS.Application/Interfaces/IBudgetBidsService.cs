using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IBudgetBidsService
    {
        Task<List<BidViewDto>> GetBidViewAsync(string workgroup);
        Task<PaginatedResult<BidViewDto>> GetBidViewPagedAsync(QueryParameters<string> query, string workgroup);
        Task<BidDto?> GetBidByIdAsync(string WorkGroupName, string account);
        Task<BidDto> AddBidAsync(BidDto bid);
        Task<BidDto> UpdateBidAsync(BidDto bid);
        Task<bool> DeleteBidAsync(string WorkGroupName, string account);
        Task<List<AccountCategoryDto>> GetAccountCategoriesAsync();
        Task<PaginatedResult<GenericBidViewDto>> GetGenericBidsPagedAsync(QueryParameters<string> query);
    }
}
