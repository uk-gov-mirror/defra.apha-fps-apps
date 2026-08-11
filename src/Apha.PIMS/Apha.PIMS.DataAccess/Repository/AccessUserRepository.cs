using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class AccessUserRepository : BaseRepository, IAccessUserRepository
    {
        private readonly PimsDbContext _dbContext;

        public AccessUserRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedData<AccessUser>> GetPagedAsync(PaginationParameters<string> query)
        {
            var baseQuery = _dbContext.AccessUsers.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter)
                    ?? new Dictionary<string, string>();

                if (filters.TryGetValue("NtLogin", out var ntLoginFilter)
                    && !string.IsNullOrWhiteSpace(ntLoginFilter))
                {
                    var value = ntLoginFilter.Trim();
                    baseQuery = baseQuery.Where(u => EF.Functions.ILike(u.NtLogin, $"%{value}%"));
                }

                if (filters.TryGetValue("UserName", out var userNameFilter)
                    && !string.IsNullOrWhiteSpace(userNameFilter))
                {
                    var value = userNameFilter.Trim();
                    baseQuery = baseQuery.Where(u => u.UserName != null && EF.Functions.ILike(u.UserName, $"%{value}%"));
                }

                if (filters.TryGetValue("UserEmail", out var userEmailFilter)
                    && !string.IsNullOrWhiteSpace(userEmailFilter))
                {
                    var value = userEmailFilter.Trim();
                    baseQuery = baseQuery.Where(u => u.UserEmail != null && EF.Functions.ILike(u.UserEmail, $"%{value}%"));
                }
            }

            baseQuery = (query.SortBy, query.Descending) switch
            {
                ("NtLogin", true)   => baseQuery.OrderByDescending(u => u.NtLogin),
                ("NtLogin", false)  => baseQuery.OrderBy(u => u.NtLogin),
                ("UserName", true)  => baseQuery.OrderByDescending(u => u.UserName),
                ("UserName", false) => baseQuery.OrderBy(u => u.UserName),
                ("UserEmail", true)  => baseQuery.OrderByDescending(u => u.UserEmail),
                ("UserEmail", false) => baseQuery.OrderBy(u => u.UserEmail),
                ("SystemId", true)  => baseQuery.OrderByDescending(u => u.SystemId).ThenByDescending(u => u.NtLogin),
                ("SystemId", false) => baseQuery.OrderBy(u => u.SystemId).ThenBy(u => u.NtLogin),
                (_, true)           => baseQuery.OrderByDescending(u => u.SystemId).ThenByDescending(u => u.NtLogin),
                _                   => baseQuery.OrderBy(u => u.SystemId).ThenBy(u => u.NtLogin)
            };

            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            return await ApplyPaging(baseQuery, page, pageSize);
        }

        public async Task<List<AccessUser>> GetAllAsync()
        {
            return await _dbContext.AccessUsers
                .AsNoTracking()
                .OrderBy(u => u.SystemId)
                .ThenBy(u => u.NtLogin)
                .ToListAsync();
        }
        public async Task<List<AccessUser>> GetBySystemIdAsync(int systemid)
        {
            return await _dbContext.AccessUsers
                .AsNoTracking()
                .Where(u => u.SystemId == systemid)
                .OrderBy(u => u.NtLogin)
                .ToListAsync();
        }
        public async Task<List<AccessUser>> GetByNtLoginAsync(string ntlogin)
        {
            return await _dbContext.AccessUsers
                .AsNoTracking()
                .Where(u => u.NtLogin == ntlogin)
                .OrderBy(u => u.SystemId)
                .ToListAsync();
        }
        public async Task<AccessUser?> GetByIdAsync(int systemid, string ntlogin)
        {
            return await _dbContext.AccessUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.SystemId == systemid && u.NtLogin == ntlogin);
        }
        public async Task<AccessUser> AddAsync(AccessUser entity)
        {
            _dbContext.AccessUsers.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<AccessUser> UpdateAsync(AccessUser entity)
        {
            _dbContext.AccessUsers.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int systemid, string ntlogin)
        {
            int rowsAffected = await _dbContext.AccessUsers
                .Where(u => u.SystemId == systemid && u.NtLogin == ntlogin)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }
        public async Task<bool> ExistsAsync(int systemid, string ntlogin)
        {
            return await _dbContext.AccessUsers
                .AnyAsync(u => u.SystemId == systemid && u.NtLogin == ntlogin);
        }
    }
}
