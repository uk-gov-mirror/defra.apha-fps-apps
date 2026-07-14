using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.Costbook.DataAccess.Repositories
{
    
    public class FpsAccountCategoryRepository : RepositoryBase, IFpsAccountCategoryRepository
    {
        public FpsAccountCategoryRepository(CostbookDbContext context) : base(context) { }       

        // matching VBA query: WHERE ProjectSpecific = True (Access -1 = boolean true)
        public async Task<List<FpsAccountCategory>> GetAllForMaintenanceAsync()
        {
            return await _context.FpsAccountCategories
                .AsNoTracking()
                .Where(a => a.ProjectSpecific == -1)
                .OrderBy(a => a.AccShortName)
                .ToListAsync();
        }

        
        public async Task<PagedData<FpsAccountCategory>> GetPaginatedAsync(PaginationParameters<string> queryFilter)
        {
            var queryAccountCategories = _context.FpsAccountCategories
                .AsNoTracking()
                .Where(a => a.ProjectSpecific == -1)
                .AsQueryable();

            // Apply filtering
            queryAccountCategories = ApplyAccountCategoryFilter(queryAccountCategories, queryFilter.Filter);

            // Apply sorting
            queryAccountCategories = (IQueryable<FpsAccountCategory>)ApplySorting(
                queryAccountCategories,
                queryFilter.SortBy,
                queryFilter.Descending);

            // Execute query
            return await ApplyPaging(
                queryAccountCategories,
                queryFilter.Page,
                queryFilter.PageSize);
        }

        
        public async Task<FpsAccountCategory?> GetByAccShortNameAsync(string accShortName)
        {
            return await _context.FpsAccountCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AccShortName == accShortName);
        }

  
        public async Task<bool> ExistsAsync(string accShortName)
        {
            return await _context.FpsAccountCategories
                .AnyAsync(a => a.AccShortName == accShortName);
        }

       
        public async Task<FpsAccountCategory> AddAsync(FpsAccountCategory accountCategory)
        {
            _context.FpsAccountCategories.Add(accountCategory);
            await _context.SaveChangesAsync();
            return accountCategory;
        }

        
        public async Task<FpsAccountCategory> UpdateAsync(FpsAccountCategory accountCategory)
        {
            var existing = await _context.FpsAccountCategories
                .FirstOrDefaultAsync(a => a.AccShortName == accountCategory.AccShortName);

            if (existing == null)
                throw new KeyNotFoundException($"FpsAccountCategory with AccShortName '{accountCategory.AccShortName}' not found.");

            existing.AccountDescription = accountCategory.AccountDescription;
            existing.AccountType = accountCategory.AccountType;
            existing.ConstituentAccountCodes = accountCategory.ConstituentAccountCodes;
            existing.Csg7Group = accountCategory.Csg7Group;
            existing.ProjectSpecific = accountCategory.ProjectSpecific;
            existing.RcSpecific = accountCategory.RcSpecific;

            await _context.SaveChangesAsync();
            return existing;
        }

        
        public async Task<bool> UpdateCsg7GroupAsync(string accShortName, string? csg7Group)
        {
            var rowsAffected = await _context.FpsAccountCategories
                .Where(a => a.AccShortName == accShortName)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.Csg7Group, csg7Group));

            return rowsAffected > 0;
        }

        
        public async Task<bool> DeleteAsync(string accShortName)
        {
            var exists = await _context.FpsAccountCategories.AnyAsync(a => a.AccShortName == accShortName);
            if (!exists)
                return false;

            await _context.FpsAccountCategories
                .Where(a => a.AccShortName == accShortName)
                .ExecuteDeleteAsync();

            return true;
        }


        private static IQueryable<FpsAccountCategory> ApplyAccountCategoryFilter(
    IQueryable<FpsAccountCategory> queryAccountCategories,
    string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return queryAccountCategories;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);

            if (filterModel == null)
            {
                return queryAccountCategories;
            }

            var dict = (IDictionary<string, object>)filterModel;

            // Account Short Name
            if (dict.TryGetValue("AccShortName", out var accShortName) &&
                accShortName is string accShortNameValue &&
                !string.IsNullOrWhiteSpace(accShortNameValue))
            {
                queryAccountCategories = queryAccountCategories.Where(x =>
                    EF.Functions.ILike(x.AccShortName, $"%{accShortNameValue}%"));
            }

            // Description (supports both Description and AccountDescription)
            object? description = null;

            if ((dict.TryGetValue("Description", out description) ||
                 dict.TryGetValue("AccountDescription", out description)) &&
                description is string descriptionValue &&
                !string.IsNullOrWhiteSpace(descriptionValue))
            {
                queryAccountCategories = queryAccountCategories.Where(x =>
                    EF.Functions.ILike(x.AccountDescription!, $"%{descriptionValue}%"));
            }

            // CSG7 Group
            if (dict.TryGetValue("Csg7Group", out var csg7Group) &&
                csg7Group is string csg7GroupValue &&
                !string.IsNullOrWhiteSpace(csg7GroupValue))
            {
                queryAccountCategories = queryAccountCategories.Where(x =>
                    EF.Functions.ILike(x.Csg7Group!, $"%{csg7GroupValue}%"));
            }

            return queryAccountCategories;
        }

        private static IQueryable ApplySorting(
    IQueryable<FpsAccountCategory> query,
    string? sortBy,
    bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return query.OrderBy(x => x.AccShortName);
            }

            return ApplySortingByProperty(query, sortBy.Trim().ToLowerInvariant(), descending);
        }
        private static IQueryable ApplySortingByProperty(
      IQueryable<FpsAccountCategory> query,
      string property,
      bool descending)
        {
            return property switch
            {
                "accshortname" =>
                    ApplyOrder(query, p => p.AccShortName, descending),

                "description" =>
                    ApplyOrder(query, p => p.AccountDescription, descending),

                "accountdescription" =>
                    ApplyOrder(query, p => p.AccountDescription, descending),

                "csg7group" =>
                    ApplyOrder(query, p => p.Csg7Group, descending),

                _ =>
                    query.OrderBy(p => p.AccShortName)
            };
        }
        // Order application helper similar to ProjectRepository ApplyOrder
        private static IQueryable ApplyOrder<T>(
            IQueryable<FpsAccountCategory> query,
            Expression<Func<FpsAccountCategory, T>> keySelector,
            bool descending)
        {
            return descending
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);
        }
    }
}
