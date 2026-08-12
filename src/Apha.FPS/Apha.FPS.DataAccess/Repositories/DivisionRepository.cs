using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Repository implementation for Division entity data access.
    /// </summary>
    public class DivisionRepository : BaseRepository, IDivisionRepository
    {
        public DivisionRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<List<Division>> GetAllDivisionsAsync()
        {
            return await _context.Divisions
                .AsNoTracking()
                .OrderBy(d => d.DivName)
                .ToListAsync();
        }

        public async Task<PagedData<Division>> GetAllDivisionsPagedAsync(PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var divisionsQuery = _context.Divisions
                .AsNoTracking()
                .AsQueryable();

            // Apply filtering
            divisionsQuery = ApplyDivisionFilter(divisionsQuery, query.Filter);

            // Apply sorting
            divisionsQuery = ApplySorting(divisionsQuery, query.SortBy, query.Descending);

            var divisions = await divisionsQuery.ToListAsync();

            return ApplyPaging(divisions, query.Page, query.PageSize);
        }

        public async Task<Division?> GetDivisionByNameAsync(string divName)
        {
            if (string.IsNullOrWhiteSpace(divName))
            {
                return null;
            }

            return await _context.Divisions
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DivName == divName);
        }

        public async Task<Division> CreateDivisionAsync(Division division)
        {
            ArgumentNullException.ThrowIfNull(division);

            _context.Divisions.Add(division);
            await _context.SaveChangesAsync();
            return division;
        }

        public async Task<Division> UpdateDivisionAsync(string originalDivName, Division division)
        {
            ArgumentNullException.ThrowIfNull(division);

            if (string.IsNullOrWhiteSpace(originalDivName))
            {
                throw new ArgumentException("Original division name is required.", nameof(originalDivName));
            }

            // Check if primary key (DivName) is being changed
            if (!originalDivName.Equals(division.DivName, StringComparison.OrdinalIgnoreCase))
            {
                // Primary key is changing - use delete and insert pattern
                // First, get the existing record
                var existingDivision = await _context.Divisions
                    .FirstOrDefaultAsync(d => d.DivName == originalDivName);

                if (existingDivision == null)
                {
                    throw new InvalidOperationException($"Division '{originalDivName}' not found.");
                }

                // Delete the old record
                _context.Divisions.Remove(existingDivision);
                await _context.SaveChangesAsync();

                // Insert with new primary key value
                _context.Divisions.Add(division);
                await _context.SaveChangesAsync();

                return division;
            }
            else
            {
                // Primary key is NOT changing - use normal update
                var existingDivision = await _context.Divisions
                    .FirstOrDefaultAsync(d => d.DivName == originalDivName);

                if (existingDivision == null)
                {
                    throw new InvalidOperationException($"Division '{originalDivName}' not found.");
                }

                // Update properties manually to ensure tracking
                existingDivision.DivisionId = division.DivisionId;
                existingDivision.AgencyId = division.AgencyId;
                existingDivision.CentOverhead = division.CentOverhead;

                await _context.SaveChangesAsync();
                return existingDivision;
            }
        }

        public async Task<bool> DeleteDivisionAsync(string divName)
        {
            if (string.IsNullOrWhiteSpace(divName))
            {
                return false;
            }

            var division = await _context.Divisions
                .FirstOrDefaultAsync(d => d.DivName == divName);

            if (division == null)
            {
                return false;
            }

            _context.Divisions.Remove(division);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DivisionExistsAsync(string divName)
        {
            if (string.IsNullOrWhiteSpace(divName))
            {
                return false;
            }

            return await _context.Divisions
                .AsNoTracking()
                .AnyAsync(d => EF.Functions.ILike(d.DivName, divName));
        }

        public async Task<List<string>> GetDivisionForeignKeyReferencesAsync(string divName)
        {
            var referencedTables = new List<string>();

            if (string.IsNullOrWhiteSpace(divName))
            {
                return referencedTables;
            }

            // Check ProfitCentre table (division field references divname)
            // Table: fps.tblkpprofitcentre
            var profitCentreExists = await _context.Set<ProfitCentre>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(pc => pc.Division == divName);

            if (profitCentreExists)
            {
                referencedTables.Add("tblkpprofitcentre");
            }

            // Check DivisionGrade table (division field references divname)
            // Table: fps.divisiongrade
            var divisionGradeExists = await _context.Set<DivisionGrade>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(dg => dg.Division == divName);

            if (divisionGradeExists)
            {
                referencedTables.Add("divisiongrade");
            }

            return referencedTables;
        }

        private static IQueryable<Division> ApplyDivisionFilter(IQueryable<Division> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("DivisionId", out var divisionId) && divisionId != null
                && int.TryParse(divisionId.ToString(), out var divisionIdValue))
                query = query.Where(d => d.DivisionId == divisionIdValue);

            if (dict.TryGetValue("AgencyId", out var agencyId) && agencyId != null
                && int.TryParse(agencyId.ToString(), out var agencyIdValue))
                query = query.Where(d => d.AgencyId == agencyIdValue);

            if (dict.TryGetValue("DivName", out var divName) && divName != null)
                query = query.Where(d => EF.Functions.ILike(d.DivName, $"%{divName}%"));

            return query;
        }

        private static IQueryable<Division> ApplySorting(IQueryable<Division> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(d => d.DivName);
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable<Division> ApplySortingByProperty(IQueryable<Division> query, string property, bool descending)
        {
            return property switch
            {
                "divisionid" => descending ? query.OrderByDescending(d => d.DivisionId) : query.OrderBy(d => d.DivisionId),
                "agencyid" => descending ? query.OrderByDescending(d => d.AgencyId) : query.OrderBy(d => d.AgencyId),
                "divname" => descending ? query.OrderByDescending(d => d.DivName) : query.OrderBy(d => d.DivName),
                "centoverhead" => descending ? query.OrderByDescending(d => d.CentOverhead) : query.OrderBy(d => d.CentOverhead),
                _ => query.OrderBy(d => d.DivName)
            };
        }
    }
}
