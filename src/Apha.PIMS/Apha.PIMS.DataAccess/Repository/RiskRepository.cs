using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class RiskRepository : BaseRepository, IRiskRepository
    {
        private readonly PimsDbContext _dbContext;

        public RiskRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Risk>> GetAllRiskRatingsAsync()
        {
            return await _dbContext.Risks
                .AsNoTracking()
                .OrderBy(r => r.RiskId)
                .ToListAsync();
        }

        public async Task<PagedData<Risk>> GetPagedRiskRatingsAsync(PaginationParameters<string> query)
        {
            var baseQuery = _dbContext.Risks.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter)
                    ?? new Dictionary<string, string>();

                if (filters.TryGetValue("Riskid", out var riskIdFilter)
                    && int.TryParse(riskIdFilter, out var riskId))
                {
                    baseQuery = baseQuery.Where(r => r.RiskId == riskId);
                }

                if (filters.TryGetValue("Riskrating", out var riskratingFilter)
                    && !string.IsNullOrWhiteSpace(riskratingFilter))
                {
                    var value = riskratingFilter.Trim();
                    baseQuery = baseQuery.Where(r => EF.Functions.ILike(r.RiskRating, $"%{value}%"));
                }
            }

            baseQuery = (query.SortBy, query.Descending) switch
            {
                ("Riskid", true)      => baseQuery.OrderByDescending(r => r.RiskId),
                ("Riskid", false)     => baseQuery.OrderBy(r => r.RiskId),
                ("Riskrating", true)  => baseQuery.OrderByDescending(r => r.RiskRating),
                ("Riskrating", false) => baseQuery.OrderBy(r => r.RiskRating),
                (_, true)             => baseQuery.OrderByDescending(r => r.RiskId),
                _                     => baseQuery.OrderBy(r => r.RiskId)
            };

            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            return await ApplyPaging(baseQuery, page, pageSize);
        }

        public async Task<Risk?> GetRiskRatingByIdAsync(int riskId)
        {
            return await _dbContext.Risks
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RiskId == riskId);
        }

        public async Task<Risk> AddRiskRatingAsync(Risk entity)
        {
            _dbContext.Risks.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Risk> UpdateRiskRatingAsync(Risk entity)
        {
            _dbContext.Risks.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteRiskRatingAsync(int riskId)
        {
            int rowsAffected = await _dbContext.Risks
                .Where(r => r.RiskId == riskId)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> RiskRatingExistsAsync(int riskId)
        {
            return await _dbContext.Risks
                .AnyAsync(r => r.RiskId == riskId);
        }
    }
}
