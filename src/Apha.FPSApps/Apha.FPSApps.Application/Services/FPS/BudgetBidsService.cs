using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class BudgetBidsService : IBudgetBidsService
    {
        private readonly IFpsApiClient _fpsClient;

        public BudgetBidsService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<BidViewDto>>> GetBidViewAsync(string workgroup)
        {
            return await _fpsClient.FpsBudgetBids.GetBidViewAsync(workgroup);
        }

        public async Task<ApiResponseDto<List<BidViewDto>>> GetBidViewPagedAsync(QueryParameters<string> query, string workgroup)
        {
            return await _fpsClient.FpsBudgetBids.GetBidViewPagedAsync(query, workgroup);
        }

        public async Task<ApiResponseDto<BidDto>> GetBidByIdAsync(string WorkGroupName, string account)
        {
            return await _fpsClient.FpsBudgetBids.GetBidByIdAsync(WorkGroupName, account);
        }

        public async Task<ApiResponseDto<BidDto>> CreateBidAsync(BidDto bid)
        {
            return await _fpsClient.FpsBudgetBids.CreateBidAsync(bid);
        }

        public async Task<ApiResponseDto<BidDto>> UpdateBidAsync(BidDto bid)
        {
            return await _fpsClient.FpsBudgetBids.UpdateBidAsync(bid);
        }

        public async Task<ApiResponseDto<bool>> DeleteBidAsync(BidDto bid)
        {
            return await _fpsClient.FpsBudgetBids.DeleteBidAsync(bid);
        }

        public async Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync()
        {
            return await _fpsClient.FpsBudgetBids.GetAccountCategoriesAsync();
        }

        public async Task<ApiResponseDto<List<GenericBidViewDto>>> GetGenericBidsPagedAsync(QueryParameters<string> query)
        {
            return await _fpsClient.FpsBudgetBids.GetGenericBidsPagedAsync(query);
        }
    }
}
