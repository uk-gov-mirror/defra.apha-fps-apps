using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsReviewItemApiClient
    {
        Task<ApiResponseDto<List<ReviewItemDto>>> GetAllReviewItemsAsync();
     
        Task<ApiResponseDto<PaginatedResult<ReviewItemDto>>> GetPagedReviewItemsAsync(QueryParameters<string> query);

       
        Task<ApiResponseDto<ReviewItemDto>> GetReviewItemByIdAsync(int itemId);
        //
        Task<ApiResponseDto<ReviewItemDto>> CreateReviewItemAsync(ReviewItemDto dto);
        Task<ApiResponseDto<ReviewItemDto>> UpdateReviewItemAsync(int itemId, ReviewItemDto dto);
        Task<ApiResponseDto<bool>> DeleteReviewItemAsync(int itemId);
    }
}
