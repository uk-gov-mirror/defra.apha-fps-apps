using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class RadTrackProgRepository : BaseRepository, IRadTrackProgRepository
    {
        private readonly PimsDbContext _dbContext;

        public RadTrackProgRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<RadtrackProg>> GetAllRadTrackProgsAsync()
        {
            return await _dbContext.RadtrackProgs
                .AsNoTracking()
                .OrderBy(r => r.Program)
                .ToListAsync();
        }

        public async Task<PagedData<RadtrackProg>> GetPagedRadTrackProgsAsync(PaginationParameters<string> query)
        {
            var q = _dbContext.RadtrackProgs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                q = q.Where(r => EF.Functions.ILike(r.Program, $"%{query.Search}%") ||
                                 EF.Functions.ILike(r.Publicationprefix ?? string.Empty, $"%{query.Search}%"));
            }

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter ?? "{}")
                          ?? new Dictionary<string, string>();

            foreach (var filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Value))
                    continue;

                switch (filter.Key.ToLower())
                {
                    case "program":
                        q = q.Where(r => EF.Functions.ILike(r.Program, $"%{filter.Value}%"));
                        break;
                    case "publicationprefix":
                        q = q.Where(r => EF.Functions.ILike(r.Publicationprefix ?? string.Empty, $"%{filter.Value}%"));
                        break;
                }
            }

            q = (query.SortBy?.ToLower(), query.Descending) switch
            {
                ("program", true) => q.OrderByDescending(r => r.Program),
                ("program", false) => q.OrderBy(r => r.Program),
                ("publicationprefix", true) => q.OrderByDescending(r => r.Publicationprefix),
                ("publicationprefix", false) => q.OrderBy(r => r.Publicationprefix),
                (_, true) => q.OrderByDescending(r => r.Program),
                _ => q.OrderBy(r => r.Program)
            };

            return await ApplyPaging(q, query.Page, query.PageSize);
        }
        public async Task<RadtrackProg?> GetRadTrackProgByProgramAsync(string program)
        {
            return await _dbContext.RadtrackProgs
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Program == program);
        }
        public async Task<RadtrackProg> AddRadTrackProgAsync(RadtrackProg entity)
        {
            _dbContext.RadtrackProgs.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<RadtrackProg> UpdateRadTrackProgAsync(RadtrackProg entity)
        {
            _dbContext.RadtrackProgs.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteRadTrackProgAsync(string program)
        {
            int rowsAffected = await _dbContext.RadtrackProgs
                .Where(r => r.Program == program)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }
        public async Task<bool> RadTrackProgExistsAsync(string program)
        {
            return await _dbContext.RadtrackProgs
                .AnyAsync(r => r.Program == program);
        }

        // Returns distinct non-null Program values from MY_tlkpProject ordered alphabetically
        // SQL equivalent: SELECT Program FROM MY_tlkpProject GROUP BY Program HAVING Program IS NOT NULL ORDER BY Program
        public async Task<List<string>> GetAllProgramNamesAsync()
        {
            return await _dbContext.MyTlkpProjects
                .AsNoTracking()
                .Where(p => p.Program != null)
                .GroupBy(p => p.Program)
                .Select(g => g.Key!)
                .OrderBy(p => p)
                .ToListAsync();
        }
    }
}
