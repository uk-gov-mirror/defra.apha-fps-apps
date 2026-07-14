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
    public class CapsStaffRepository : RepositoryBase, ICapsStaffRepository
    {
        public CapsStaffRepository(CostbookDbContext context) : base(context) { }

        public async Task<List<Staff>> GetAllStaffAsync()
        {
            return await _context.Staffs
                .AsNoTracking()
                .OrderBy(c => c.Mnumber)
                .ToListAsync();
        }

        public async Task<PagedData<Staff>> GetPaginatedAsync(PaginationParameters<string> queryFilter)
        {
            var query = _context.Staffs
                .AsNoTracking()
                .AsQueryable();

            // Apply filtering
            query = ApplyCapsStaffFilter(query, queryFilter.Filter);

            // Apply free-text search across all searchable columns
            if (!string.IsNullOrWhiteSpace(queryFilter.Search))
            {
                var search = queryFilter.Search.ToLower();
                query = query.Where(c =>
                    c.Mnumber.ToLower().Contains(search) ||
                    c.Name.ToLower().Contains(search) ||
                    (c.Dt2number != null && c.Dt2number.ToLower().Contains(search)));
            }

            // Apply sorting (defaults to Mnumber)
            query = (IQueryable<Staff>)ApplySorting(query, queryFilter.SortBy, queryFilter.Descending);

            return await ApplyPaging(query.OrderBy(c => c.Mnumber), queryFilter.Page, queryFilter.PageSize);
        }

        public async Task<Staff?> GetByMNumberAsync(string mNumber)
        {
            return await _context.Staffs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Mnumber == mNumber);
        }

        public async Task<bool> ExistsAsync(string mNumber)
        {
            return await _context.Staffs.AnyAsync(c => c.Mnumber == mNumber);
        }

        public async Task<Staff> AddStaffAsync(Staff capsStaff)
        {
            _context.Staffs.Add(capsStaff);
            await _context.SaveChangesAsync();
            return capsStaff;
        }

        public async Task<Staff> UpdateStaffAsync(Staff capsStaff)
        {
            var existing = await _context.Staffs
                .FirstOrDefaultAsync(c => c.Mnumber == capsStaff.Mnumber);

            if (existing == null)
                throw new KeyNotFoundException($"Staff member '{capsStaff.Mnumber}' not found.");

            existing.Name      = capsStaff.Name;
            existing.Dt2number = capsStaff.Dt2number;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteStaffAsync(string mNumber)
        {
            var exists = await _context.Staffs.AnyAsync(c => c.Mnumber == mNumber);
            if (!exists) return false;

            await _context.Staffs
                .Where(c => c.Mnumber == mNumber)
                .ExecuteDeleteAsync();

            return true;
        }

        // Filtering helper: supports Mnumber and Name column filters.
        private static IQueryable<Staff> ApplyCapsStaffFilter(
            IQueryable<Staff> query,
            string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("MNumber", out var mnVal) || dict.TryGetValue("Mnumber", out mnVal))
            {
                if (mnVal is string mn && !string.IsNullOrWhiteSpace(mn))
                {
                    query = query.Where(x => EF.Functions.ILike(x.Mnumber!, $"%{mn}%"));
                }
            }

            if (dict.TryGetValue("Name", out var nameVal) || dict.TryGetValue("name", out nameVal))
            {
                if (nameVal is string name && !string.IsNullOrWhiteSpace(name))
                {
                    query = query.Where(x => EF.Functions.ILike(x.Name!, $"%{name}%"));
                }
            }

            if (dict.TryGetValue("Filter", out var genericVal) && genericVal is string g && !string.IsNullOrWhiteSpace(g))
            {
                query = query.Where(x =>
                    EF.Functions.ILike(x.Mnumber!, $"%{g}%") ||
                    EF.Functions.ILike(x.Name!, $"%{g}%") ||
                    (x.Dt2number != null && EF.Functions.ILike(x.Dt2number, $"%{g}%")));
            }

            return query;
        }

        // Sorting helper: supports sorting by Mnumber and Name (defaults to Mnumber)
        private static IQueryable ApplySorting(
            IQueryable<Staff> query,
            string? sortBy,
            bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return descending ? query.OrderByDescending(x => x.Mnumber) : query.OrderBy(x => x.Mnumber);
            }

            var prop = sortBy.Trim().ToLowerInvariant();
            return prop switch
            {
                "name" => ApplyOrder(query, q => q.Name, descending),
                "mnumber" => ApplyOrder(query, q => q.Mnumber, descending),
                _ => descending ? query.OrderByDescending(x => x.Mnumber) : query.OrderBy(x => x.Mnumber),
            };
        }

        private static IQueryable ApplyOrder<TProperty>(
            IQueryable<Staff> query,
            Expression<Func<Staff, TProperty>> keySelector,
            bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}

