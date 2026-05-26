using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class MonthlyTimeRepository : BaseRepository, IMonthlyTimeRepository
    {
        public MonthlyTimeRepository(FpsDbContext context) : base(context) { }

        public async Task<bool> HasMonthlyTimeEntriesAsync(string workGroup, string timeCode, string parentProject)
        {
            return await _context.MonthlyTimes
                .AsNoTracking()
                .AnyAsync(m => m.WorkGroup == workGroup && m.TimeCode == timeCode && m.ParentProject == parentProject);
        }

        public async Task<IEnumerable<MonthlyTime>> GetMonthlyTimeByTimeCodeAndProjectAsync(string timeCode, string workGroup, string parentProject)
        {
            return await _context.MonthlyTimes
                .AsNoTracking()
                .Where(m => m.TimeCode == timeCode && m.WorkGroup == workGroup && m.ParentProject == parentProject
                            && m.FpsYear == _context.FilterFpsYear)
                .OrderBy(m => m.Month)
                .ToListAsync();
        }

        public async Task<PagedData<MonthlyTime>> GetPagedMonthlyTimeAsync(PaginationParameters<string> parameters, string? timeCode, string? workGroup, string? parentProject)
        {
            var query = _context.MonthlyTimes
                .AsNoTracking()
                .Where(m => m.FpsYear == _context.FilterFpsYear);

            if (!string.IsNullOrWhiteSpace(timeCode))
                query = query.Where(m => m.TimeCode == timeCode);
            if (!string.IsNullOrWhiteSpace(workGroup))
                query = query.Where(m => m.WorkGroup == workGroup);
            if (!string.IsNullOrWhiteSpace(parentProject))
                query = query.Where(m => m.ParentProject == parentProject);

            var ordered = query.OrderBy(m => m.PactStaffId).ThenBy(m => m.Month);
            return ApplyPaging(await ordered.ToListAsync(), parameters.Page, parameters.PageSize);
        }

        public async Task<MonthlyTime?> GetMonthlyTimeByIdAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            return await _context.MonthlyTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.PactStaffId == pactStaffId && m.TimeCode == timeCode
                                          && m.Month == month && m.ParentProject == parentProject
                                          && m.FpsYear == _context.FilterFpsYear);
        }

        public async Task<MonthlyTime> CreateMonthlyTimeAsync(MonthlyTime entity)
        {
            entity.FpsYear = _context.FilterFpsYear;
            _context.MonthlyTimes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<MonthlyTime> UpdateMonthlyTimeAsync(MonthlyTime entity)
        {
            _context.MonthlyTimes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteMonthlyTimeAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            var entity = await _context.MonthlyTimes
                .FirstOrDefaultAsync(m => m.PactStaffId == pactStaffId && m.TimeCode == timeCode
                                          && m.Month == month && m.ParentProject == parentProject
                                          && m.FpsYear == _context.FilterFpsYear);
            if (entity == null) return false;
            _context.MonthlyTimes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
