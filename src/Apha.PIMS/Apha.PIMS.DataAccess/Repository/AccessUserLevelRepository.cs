using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class AccessUserLevelRepository : BaseRepository, IAccessUserLevelRepository
    {
        private readonly PimsDbContext _dbContext;

        public AccessUserLevelRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<PagedData<AccessUserLevel>> GetPagedAccessUserLevelAllAsync(PaginationParameters<string> query)
        {
            var baseQuery = _dbContext.AccessUserLevels.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter)
                    ?? new Dictionary<string, string>();

                if (filters.TryGetValue("UserName", out var userNameFilter)
                    && !string.IsNullOrWhiteSpace(userNameFilter))
                {
                    var value = userNameFilter.Trim();
                    // Resolve matching NtLogins via subquery against AccessUsers
                    var matchingLogins = _dbContext.AccessUsers
                        .Where(u => u.UserName != null && EF.Functions.ILike(u.UserName, $"%{value}%"))
                        .Select(u => u.NtLogin);
                    baseQuery = baseQuery.Where(ul => matchingLogins.Contains(ul.NtLogin));
                }

                if (filters.TryGetValue("NtLogin", out var ntloginFilter)
                    && !string.IsNullOrWhiteSpace(ntloginFilter))
                {
                    var value = ntloginFilter.Trim();
                    baseQuery = baseQuery.Where(ul => EF.Functions.ILike(ul.NtLogin, $"%{value}%"));
                }

                if (filters.TryGetValue("SystemId", out var systemIdFilter)
                    && int.TryParse(systemIdFilter, out var systemIdVal))
                {
                    baseQuery = baseQuery.Where(ul => ul.SystemId == systemIdVal);
                }
            }

            baseQuery = (query.SortBy, query.Descending) switch
            {
                ("NtLogin", true)        => baseQuery.OrderByDescending(ul => ul.NtLogin),
                ("NtLogin", false)       => baseQuery.OrderBy(ul => ul.NtLogin),
                ("UserName", true)       => baseQuery.OrderByDescending(ul => ul.NtLogin),
                ("UserName", false)      => baseQuery.OrderBy(ul => ul.NtLogin),
                ("AccessLevelId", true)  => baseQuery.OrderByDescending(ul => ul.AccessLevelId),
                ("AccessLevelId", false) => baseQuery.OrderBy(ul => ul.AccessLevelId),
                ("SystemId", true)       => baseQuery.OrderByDescending(ul => ul.SystemId),
                ("SystemId", false)      => baseQuery.OrderBy(ul => ul.SystemId),
                (_, true)               => baseQuery.OrderByDescending(ul => ul.SystemId).ThenByDescending(ul => ul.NtLogin),
                _                       => baseQuery.OrderBy(ul => ul.SystemId).ThenBy(ul => ul.NtLogin)
            };

            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            return await ApplyPaging(baseQuery, page, pageSize);
        }
        public async Task<List<AccessUserLevel>> GetBySystemIdAsync(int systemid)
        {
            return await _dbContext.AccessUserLevels
                .AsNoTracking()
                .Where(ul => ul.SystemId == systemid)
                .OrderBy(ul => ul.NtLogin)
                .ThenBy(ul => ul.AccessLevelId)
                .ToListAsync();
        }
        public async Task<List<AccessUserLevel>> GetByUserAsync(int systemid, string ntlogin)
        {
            return await _dbContext.AccessUserLevels
                .AsNoTracking()
                .Where(ul => ul.SystemId == systemid && ul.NtLogin == ntlogin)
                .OrderBy(ul => ul.AccessLevelId)
                .ToListAsync();
        }
        public async Task<AccessUserLevel?> GetByIdAsync(int systemid, string ntlogin, int accesslevelid)
        {
            return await _dbContext.AccessUserLevels
                .AsNoTracking()
                .FirstOrDefaultAsync(ul => ul.SystemId == systemid
                                        && ul.NtLogin == ntlogin
                                        && ul.AccessLevelId == accesslevelid);
        }
        public async Task<AccessUserLevel> AddAsync(AccessUserLevel entity)
        {
            _dbContext.AccessUserLevels.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int systemId, string ntLogin, int accessLevelId)
        {
            int rowsAffected = await _dbContext.AccessUserLevels
                .Where(ul => ul.SystemId == systemId
                          && ul.NtLogin == ntLogin
                          && ul.AccessLevelId == accessLevelId)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }
        public async Task<bool> ExistsAsync(int systemId, string ntLogin, int accessLevelId)
        {
            return await _dbContext.AccessUserLevels
                .AnyAsync(ul => ul.SystemId == systemId
                             && ul.NtLogin == ntLogin
                             && ul.AccessLevelId == accessLevelId);
        }
    }
}
