using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IBudgetBidsService
    {
        Task<ApiResponseDto<List<BidViewDto>>> GetBidViewAsync(string workgroup);
        Task<ApiResponseDto<List<BidViewDto>>> GetBidViewPagedAsync(QueryParameters<string> query, string workgroup);
        Task<ApiResponseDto<BidDto>> GetBidByIdAsync(string WorkGroupName, string account);
        Task<ApiResponseDto<BidDto>> CreateBidAsync(BidDto bid);
        Task<ApiResponseDto<BidDto>> UpdateBidAsync(BidDto bid);
        Task<ApiResponseDto<bool>> DeleteBidAsync(BidDto bid);
        Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync();
        Task<ApiResponseDto<List<GenericBidViewDto>>> GetGenericBidsPagedAsync(QueryParameters<string> query);
    }
}
