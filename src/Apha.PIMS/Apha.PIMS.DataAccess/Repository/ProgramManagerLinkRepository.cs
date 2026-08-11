using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class ProgramManagerLinkRepository : BaseRepository, IProgramManagerLinkRepository
    {
        private readonly PimsDbContext _dbContext;

        public ProgramManagerLinkRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ProgramManagerLink>> GetAllProgramManagerLinksAsync()
        {
            return await _dbContext.ProgramManagerLinks
                .AsNoTracking()
                .OrderBy(l => l.Program)
                .ThenBy(l => l.Manager)
                .ToListAsync();
        }

        public async Task<PagedData<ProgramManagerLink>> GetPagedByManagerAsync(PaginationParameters<string> query, string manager)
        {
            var baseQuery = _dbContext.ProgramManagerLinks
                .AsNoTracking()
                .Where(l => l.Manager == manager);

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter)
                    ?? new Dictionary<string, string>();

                if (filters.TryGetValue("Program", out var programFilter)
                    && !string.IsNullOrWhiteSpace(programFilter))
                {
                    var value = programFilter.Trim();
                    baseQuery = baseQuery.Where(l => EF.Functions.ILike(l.Program, $"%{value}%"));
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
                ("Program", true) => baseQuery.OrderByDescending(l => l.Program).ThenBy(l => l.Manager),
                ("Program", false) => baseQuery.OrderBy(l => l.Program).ThenBy(l => l.Manager),
                ("Manager", true) => baseQuery.OrderByDescending(l => l.Manager).ThenBy(l => l.Program),
                ("Manager", false) => baseQuery.OrderBy(l => l.Manager).ThenBy(l => l.Program),
                (_, true) => baseQuery.OrderByDescending(l => l.Program).ThenBy(l => l.Manager),
                _ => baseQuery.OrderBy(l => l.Program).ThenBy(l => l.Manager)
            };

            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            return await ApplyPaging(baseQuery, page, pageSize);
        }

        public async Task<List<ProgramManagerLink>> GetByProgramAsync(string program)
        {
            return await _dbContext.ProgramManagerLinks
                .AsNoTracking()
                .Where(l => l.Program == program)
                .OrderBy(l => l.Manager)
                .ToListAsync();
        }

        public async Task<List<ProgramManagerLink>> GetByManagerAsync(string manager)
        {
            return await _dbContext.ProgramManagerLinks
                .AsNoTracking()
                .Where(l => l.Manager == manager)
                .OrderBy(l => l.Program)
                .ToListAsync();
        }

        public async Task<ProgramManagerLink?> GetProgramManagerLinkByIdAsync(string program, string manager)
        {
            return await _dbContext.ProgramManagerLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Program == program && l.Manager == manager);
        }

        public async Task<ProgramManagerLink> AddProgramManagerLinkAsync(ProgramManagerLink entity)
        {
            _dbContext.ProgramManagerLinks.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteProgramManagerLinkAsync(string program, string manager)
        {
            int rowsAffected = await _dbContext.ProgramManagerLinks
                .Where(l => l.Program == program && l.Manager == manager)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> ProgramManagerLinkExistsAsync(string program, string manager)
        {
            return await _dbContext.ProgramManagerLinks
                .AnyAsync(l => l.Program == program && l.Manager == manager);
        }

        // Translates: SELECT DISTINCTROW ProgramNo, Max(Year) AS LatestYear FROM MY_tlkpProgram GROUP BY ProgramNo
        public async Task<List<ProgramLookup>> GetProgramsAsync()
        {
            return await _dbContext.MyTlkpProjects
                .AsNoTracking()
                .Where(p => p.Program != null)
                .GroupBy(p => p.Program!)
                .Select(g => new ProgramLookup
                {
                    ProgramNo = g.Key,
                    LatestYear = g.Max(p => p.Year)
                })
                .OrderBy(x => x.ProgramNo)
                .ToListAsync();
        }
    }
}
