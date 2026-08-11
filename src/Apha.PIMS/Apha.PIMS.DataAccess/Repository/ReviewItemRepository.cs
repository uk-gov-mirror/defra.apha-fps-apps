using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class ReviewItemRepository : BaseRepository, IReviewItemRepository
    {
        private readonly PimsDbContext _dbContext;

        public ReviewItemRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ReviewItem>> GetAllReviewItemsAsync()
        {
            return await _dbContext.ReviewItems
                .AsNoTracking()
                .OrderBy(r => r.ItemId)
                .ToListAsync();
        }

        public async Task<PagedData<ReviewItem>> GetPagedReviewItemsAsync(PaginationParameters<string> query)
        {
            var baseQuery = _dbContext.ReviewItems.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter)
                    ?? new Dictionary<string, string>();

                if (filters.TryGetValue("Itemid", out var itemIdFilter)
                    && int.TryParse(itemIdFilter, out var itemId))
                {
                    baseQuery = baseQuery.Where(r => r.ItemId == itemId);
                }

                if (filters.TryGetValue("Item", out var itemFilter)
                    && !string.IsNullOrWhiteSpace(itemFilter))
                {
                    var value = itemFilter.Trim();
                    baseQuery = baseQuery.Where(r => r.Item != null &&
                        EF.Functions.ILike(r.Item, $"%{value}%"));
                }
            }

            baseQuery = (query.SortBy, query.Descending) switch
            {
                ("Itemid", true) => baseQuery.OrderByDescending(r => r.ItemId),
                ("Itemid", false) => baseQuery.OrderBy(r => r.ItemId),
                ("Item", true) => baseQuery.OrderByDescending(r => r.Item),
                ("Item", false) => baseQuery.OrderBy(r => r.Item),
                (_, true) => baseQuery.OrderByDescending(r => r.ItemId),
                _ => baseQuery.OrderBy(r => r.ItemId)
            };

            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            return await ApplyPaging(baseQuery, page, pageSize);
        }

        public async Task<ReviewItem?> GetReviewItemByIdAsync(int itemId)
        {
            return await _dbContext.ReviewItems
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ItemId == itemId);
        }

        public async Task<ReviewItem> AddReviewItemAsync(ReviewItem entity)
        {
            _dbContext.ReviewItems.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<ReviewItem> UpdateReviewItemAsync(ReviewItem entity)
        {
            _dbContext.ReviewItems.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteReviewItemAsync(int itemId)
        {
            int rowsAffected = await _dbContext.ReviewItems
                .Where(r => r.ItemId == itemId)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> ReviewItemExistsAsync(int itemId)
        {
            return await _dbContext.ReviewItems
                .AnyAsync(r => r.ItemId == itemId);
        }
    }
}
