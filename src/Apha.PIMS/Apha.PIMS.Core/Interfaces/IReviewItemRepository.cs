using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IReviewItemRepository
    {
        Task<List<ReviewItem>> GetAllReviewItemsAsync();

        Task<PagedData<ReviewItem>> GetPagedReviewItemsAsync(PaginationParameters<string> query);

        Task<ReviewItem?> GetReviewItemByIdAsync(int itemId);

        Task<ReviewItem> AddReviewItemAsync(ReviewItem entity);

        Task<ReviewItem> UpdateReviewItemAsync(ReviewItem entity);

        Task<bool> DeleteReviewItemAsync(int itemId);

        Task<bool> ReviewItemExistsAsync(int itemId);
    }
}
