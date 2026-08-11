using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IReviewItemService
    {
        Task<List<ReviewItemDto>> GetAllReviewItemsAsync();

        Task<PaginatedResult<ReviewItemDto>> GetPagedReviewItemsAsync(QueryParameters<string> query);

        Task<ReviewItemDto?> GetReviewItemByIdAsync(int itemId);

        Task<ReviewItemDto> CreateReviewItemAsync(ReviewItemDto dto);

        Task<ReviewItemDto> UpdateReviewItemAsync(ReviewItemDto dto);

        Task<bool> DeleteReviewItemAsync(int itemId);

        Task<bool> ReviewItemExistsAsync(int itemId);
    }
}
