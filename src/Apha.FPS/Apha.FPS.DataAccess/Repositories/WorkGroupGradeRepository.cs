using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Repository implementation for WorkgroupGrade CRUD and lookup data access.
    /// </summary>
    public class WorkGroupGradeRepository : BaseRepository, IWorkGroupGradeRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public WorkGroupGradeRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<PagedData<WorkgroupGrade>> GetAllWorkgroupGradesPagedAsync(
            PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var baseQuery = _dbContext.WorkgroupGrades
                .AsNoTracking()
                .AsQueryable();

            baseQuery = ApplyWorkgroupGradeFilter(baseQuery, query.Filter);
            baseQuery = (IQueryable<WorkgroupGrade>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<WorkgroupGrade?> GetByWgGradeAsync(string wgGrade)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                return null;

            return await _dbContext.WorkgroupGrades
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.WgGrade == wgGrade);
        }

        public async Task<WorkgroupGrade> CreateAsync(WorkgroupGrade entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            entity.FpsYear = _requestContext.FpsYear;
            _dbContext.WorkgroupGrades.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<WorkgroupGrade> UpdateAsync(WorkgroupGrade entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var existing = await _dbContext.WorkgroupGrades
                .FirstOrDefaultAsync(e => e.WgGrade == entity.WgGrade);

            if (existing is null)
                throw new KeyNotFoundException($"WorkgroupGrade '{entity.WgGrade}' not found.");

            existing.ProfitCentreGrade = entity.ProfitCentreGrade;
            existing.GradeCode = entity.GradeCode;
            existing.Workgroup = entity.Workgroup;
            existing.FpsYear = _requestContext.FpsYear;

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(string wgGrade)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                return false;

            var deleted = await _dbContext.WorkgroupGrades
                .Where(e => e.WgGrade == wgGrade)
                .ExecuteDeleteAsync();

            return deleted > 0;
        }

        public async Task<List<string>> GetAllGradeCodesAsync()
        {
            return await _dbContext.WorkgroupGrades
                .AsNoTracking()
                .Select(e => e.GradeCode)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<bool> ExistsForGradeCodeAsync(string gradeCode)
        {
            if (string.IsNullOrWhiteSpace(gradeCode))
                return false;

            return await _dbContext.WorkgroupGrades
                .AsNoTracking()
                .AnyAsync(wgg => wgg.GradeCode == gradeCode);
        }

        // ─── Sorting ──────────────────────────────────────────────────────────────

        private static IQueryable ApplySorting(IQueryable<WorkgroupGrade> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query;

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<WorkgroupGrade> query, string property, bool descending)
        {
            return property switch
            {
                "wggrade" => ApplyOrder(query, e => e.WgGrade, descending),
                "profitcentregrade" => ApplyOrder(query, e => e.ProfitCentreGrade, descending),
                "gradecode" => ApplyOrder(query, e => e.GradeCode, descending),
                "workgroup" => ApplyOrder(query, e => e.Workgroup, descending),
                _ => query
            };
        }

        private static IQueryable ApplyOrder<T>(
            IQueryable<WorkgroupGrade> query,
            Expression<Func<WorkgroupGrade, T>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        // ─── Filtering ────────────────────────────────────────────────────────────

        private static IQueryable<WorkgroupGrade> ApplyWorkgroupGradeFilter(
            IQueryable<WorkgroupGrade> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter.Trim() == "{}")
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("WgGrade", out var wgGrade) && wgGrade != null)
                query = query.Where(e => e.WgGrade.Contains(wgGrade.ToString()!));

            if (dict.TryGetValue("ProfitCentreGrade", out var pcGrade) && pcGrade != null)
                query = query.Where(e => e.ProfitCentreGrade.Contains(pcGrade.ToString()!));

            if (dict.TryGetValue("GradeCode", out var gradeCode) && gradeCode != null)
                query = query.Where(e => e.GradeCode.Contains(gradeCode.ToString()!));

            if (dict.TryGetValue("Workgroup", out var workgroup) && workgroup != null)
                query = query.Where(e => e.Workgroup.Contains(workgroup.ToString()!));

            return query;
        }

        public async Task<List<WorkgroupGrade>> GetWorkgroupGradesByWorkGroupAsync(
            string workGroup)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workGroup);

            return await _dbContext.WorkgroupGrades
                .AsNoTracking()
                .Where(wg => wg.Workgroup == workGroup)
                .Distinct()
                .OrderBy(wg => wg.WgGrade)
                .ToListAsync();
        }

        // Existing methods for backward compatibility
        public async Task<PagedData<WorkGroupGradeView>> GetWorkGroupGradesAsync(
            PaginationParameters<string> query,
            string profitCentreGrade)
        {
            var all = await _dbContext.WorkGroupGradeViews
                .AsNoTracking()
                .Where(x => x.ProfitCentreGrade == profitCentreGrade
                         && x.UserEmail != null && x.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Distinct()
                .OrderBy(x => x.WgGrade)
                .ToListAsync();

            return ApplyPaging(all, query.Page, query.PageSize);
        }

        public async Task<bool> DeleteWorkGroupGradeAsync(string wgGrade)
        {
            var entity = await _dbContext.WorkgroupGrades
                .FirstOrDefaultAsync(x => x.WgGrade == wgGrade);
            if (entity == null)
                return false;

            _dbContext.WorkgroupGrades.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
