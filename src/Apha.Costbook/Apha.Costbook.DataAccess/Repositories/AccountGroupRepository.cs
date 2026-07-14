using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.Costbook.DataAccess.Repositories
{
    
    public class AccountGroupRepository : RepositoryBase, IAccountGroupRepository
    {
        public AccountGroupRepository(CostbookDbContext context) : base(context) { }
       
        public async Task<List<AccountGroup>> GetAllAccountGroupAsync()
        {
            return await _context.AccountGroups
                .AsNoTracking()
                .OrderBy(a => a.Csg7group)
                .ToListAsync();
        }

        
        public async Task<PagedData<AccountGroup>> GetPaginatedAsync(PaginationParameters<string> query)
        {
            var baseQuery = _context.AccountGroups
                .AsNoTracking()
                 .AsQueryable();

            // Apply filtering (only CSG7 Group column required)
            baseQuery = ApplyAccountGroupFilter(baseQuery, query.Filter);

            // Apply sorting (defaults to Csg7group)
            baseQuery = (IQueryable<AccountGroup>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            return await ApplyPaging(baseQuery.OrderBy(a => a.Csg7group), query.Page, query.PageSize);
        }
       
        public async Task<AccountGroup?> GetByCsg7GroupAsync(string csg7Group)
        {
            return await _context.AccountGroups
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Csg7group == csg7Group);
        }

        public async Task<bool> ExistsAsync(string csg7Group)
        {
            return await _context.AccountGroups
                .AnyAsync(a => a.Csg7group == csg7Group);
        }

        public async Task<AccountGroup> AddAccountGroupAsync(AccountGroup accountGroup)
        {
            _context.AccountGroups.Add(accountGroup);
            await _context.SaveChangesAsync();
            return accountGroup;
        }

        
        public async Task<AccountGroup> UpdateAccountGroupAsync(AccountGroup accountGroup)
        {
            var existing = await _context.AccountGroups
                .FirstOrDefaultAsync(a => a.Csg7group == accountGroup.Csg7group);

            if (existing == null)
                throw new KeyNotFoundException($"AccountGroup with Csg7group '{accountGroup.Csg7group}' not found.");

            existing.Useinflation = accountGroup.Useinflation;

            await _context.SaveChangesAsync();
            return existing;
        }

        
        public async Task<bool> DeleteAccountGroupAsync(string csg7Group)
        {
            var exists = await _context.AccountGroups.AnyAsync(a => a.Csg7group == csg7Group);
            if (!exists)
                return false;

            await _context.AccountGroups
                .Where(a => a.Csg7group == csg7Group)
                .ExecuteDeleteAsync();

            return true;
        }

        // Filtering helper: supports a single filter value for the CSG7 Group column.
        private static IQueryable<AccountGroup> ApplyAccountGroupFilter(
            IQueryable<AccountGroup> query,
            string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            // Try common key variations - "Csg7Group" or "Csg7group"
            if (dict.TryGetValue("Csg7Group", out var val) || dict.TryGetValue("Csg7group", out val))
            {
                if (val is string s && !string.IsNullOrWhiteSpace(s))
                {
                    query = query.Where(x => EF.Functions.ILike(x.Csg7group!, $"%{s}%"));
                }
            }

            // Also allow a generic filter property name "Filter" used by some grids
            if (dict.TryGetValue("Filter", out var genericVal) && genericVal is string g && !string.IsNullOrWhiteSpace(g))
            {
                query = query.Where(x => EF.Functions.ILike(x.Csg7group!, $"%{g}%"));
            }

            return query;
        }

        // Sorting helper: supports sorting by Csg7group only (defaults to Csg7group)
        private static IQueryable ApplySorting(
            IQueryable<AccountGroup> query,
            string? sortBy,
            bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return descending ? query.OrderByDescending(x => x.Csg7group) : query.OrderBy(x => x.Csg7group);
            }

            var prop = sortBy.Trim().ToLowerInvariant();
            return prop switch
            {
                "csg7group" => ApplyOrder(query, q => q.Csg7group, descending),
                _ => descending ? query.OrderByDescending(x => x.Csg7group) : query.OrderBy(x => x.Csg7group),
            };
        }

        private static IQueryable ApplyOrder<TProperty>(
            IQueryable<AccountGroup> query,
            Expression<Func<AccountGroup, TProperty>> keySelector,
            bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
