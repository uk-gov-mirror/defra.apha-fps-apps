using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class ProfitCentreManagerLinkRepository : BaseRepository, IProfitCentreManagerLinkRepository
    {
        private readonly PimsDbContext _dbContext;

        public ProfitCentreManagerLinkRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ProfitCentreManagerLink>> GetAllProfitCentreManagerLinksAsync()
        {
            return await _dbContext.ProfitCentreManagerLinks
                .AsNoTracking()
                .OrderBy(l => l.ProfitCentre)
                .ThenBy(l => l.Manager)
                .ToListAsync();
        }

        public async Task<PagedData<ProfitCentreManagerLink>> GetPagedByManagerAsync(PaginationParameters<string> query, string manager)
        {
            var baseQuery = _dbContext.ProfitCentreManagerLinks
                .AsNoTracking()
                .Where(l => l.Manager == manager);

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter)
                    ?? new Dictionary<string, string>();

                if (filters.TryGetValue("ProfitCentre", out var profitcentreFilter)
                    && !string.IsNullOrWhiteSpace(profitcentreFilter))
                {
                    var value = profitcentreFilter.Trim();
                    baseQuery = baseQuery.Where(l => EF.Functions.ILike(l.ProfitCentre, $"%{value}%"));
                }

                if (filters.TryGetValue("Manager", out var managerFilter)
                    && !string.IsNullOrWhiteSpace(managerFilter))
                {
                    var value = managerFilter.Trim();
                    baseQuery = baseQuery.Where(l => EF.Functions.ILike(l.Manager, $"%{value}%"));
                }
            }

            baseQuery = (query.SortBy, query.Descending) switch
            {
                ("ProfitCentre", true) => baseQuery.OrderByDescending(l => l.ProfitCentre).ThenBy(l => l.Manager),
                ("ProfitCentre", false) => baseQuery.OrderBy(l => l.ProfitCentre).ThenBy(l => l.Manager),
                ("Manager", true) => baseQuery.OrderByDescending(l => l.Manager).ThenBy(l => l.ProfitCentre),
                ("Manager", false) => baseQuery.OrderBy(l => l.Manager).ThenBy(l => l.ProfitCentre),
                (_, true) => baseQuery.OrderByDescending(l => l.ProfitCentre).ThenBy(l => l.Manager),
                _ => baseQuery.OrderBy(l => l.ProfitCentre).ThenBy(l => l.Manager)
            };

            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            return await ApplyPaging(baseQuery, page, pageSize);
        }
        public async Task<List<ProfitCentreLookup>> GetProfitCentresAsync()
        {
            return await _dbContext.MyTblProfitCentres
                .AsNoTracking()
                .GroupBy(x => x.ProfitCentre)
                .Select(g => new ProfitCentreLookup
                {
                    ProfitCentre = g.Key,
                    LatestYear = g.Max(x => x.Year)
                })
                .OrderBy(x => x.ProfitCentre)
                .ToListAsync();
        }
        public async Task<List<ProfitCentreManagerLink>> GetByProfitCentreAsync(string profitCentre)
        {
            return await _dbContext.ProfitCentreManagerLinks
                .AsNoTracking()
                .Where(l => l.ProfitCentre == profitCentre)
                .OrderBy(l => l.Manager)
                .ToListAsync();
        }

        public async Task<List<ProfitCentreManagerLink>> GetByManagerAsync(string manager)
        {
            return await _dbContext.ProfitCentreManagerLinks
                .AsNoTracking()
                .Where(l => l.Manager == manager)
                .OrderBy(l => l.ProfitCentre)
                .ToListAsync();
        }
        public async Task<ProfitCentreManagerLink?> GetProfitCentreManagerLinkByIdAsync(string profitCentre, string manager)
        {
            return await _dbContext.ProfitCentreManagerLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.ProfitCentre == profitCentre && l.Manager == manager);
        }
        public async Task<ProfitCentreManagerLink> AddProfitCentreManagerLinkAsync(ProfitCentreManagerLink entity)
        {
            _dbContext.ProfitCentreManagerLinks.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteProfitCentreManagerLinkAsync(string profitCentre, string manager)
        {
            int rowsAffected = await _dbContext.ProfitCentreManagerLinks
                .Where(l => l.ProfitCentre == profitCentre && l.Manager == manager)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }
        public async Task<bool> ProfitCentreManagerLinkExistsAsync(string profitCentre, string manager)
        {
            return await _dbContext.ProfitCentreManagerLinks
                .AnyAsync(l => l.ProfitCentre == profitCentre && l.Manager == manager);
        }
    }
}
