using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.PACT.DataAccess.Repository
{
    public class TimeCodeValidRepository : BaseRepository, ITimeCodeValidRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;

        public TimeCodeValidRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        public async Task<IEnumerable<TimeCodeValid>> GetByJobCodeAsync(string jobCode, string parentProject)
        {
            return await _context.TimeCodeValids
                .AsNoTracking()
                .Where(t => t.JobCode == jobCode && t.ParentProject == parentProject)
                .OrderBy(t => t.WorkGroup)
                .ToListAsync();
        }

        public async Task<List<TimeCodeValid>> GetTimeCodeValidsAsync()
        {
            return await _context.TimeCodeValids
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeCodeValid>> GetTimeCodeValidsByWorkGroupAsync(string workGroup)
        {
            return await _context.TimeCodeValids
                .AsNoTracking()
                .Where(t => t.WorkGroup == workGroup)
                .OrderBy(t => t.TimeCode)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync(string workGroup, string timeCode)
        {
            return await _context.TimeCodeValids
                .AsNoTracking()
                .Where(t => t.WorkGroup == workGroup && t.TimeCode == timeCode)
                .Select(t => t.ParentProject)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        public async Task<PagedData<TimeCodeValid>> GetPagedTimeCodesAsync(
            PaginationParameters<string> query, string? jobCode, string? parentProject)
        {
            var queryTimeCode = _context.TimeCodeValids.AsNoTracking().AsQueryable();
            if (!string.IsNullOrEmpty(jobCode))
            {
                queryTimeCode = queryTimeCode.Where(t => t.JobCode == jobCode);
            }

            if (!string.IsNullOrEmpty(parentProject))
            {
                queryTimeCode = queryTimeCode.Where(t => t.ParentProject == parentProject);
            }

            // Apply filtering
            queryTimeCode = ApplyTimeCodeFilter(queryTimeCode, query.Filter);

            // Apply sorting
            queryTimeCode = (IQueryable<TimeCodeValid>)ApplySorting(queryTimeCode, query.SortBy, query.Descending);

            // Apply paging
            return await ApplyPaging(queryTimeCode, query.Page, query.PageSize);
        }

        public async Task<PagedData<TimeCodeValid>> GetPagedByProjectAndTestCodeAsync(
            PaginationParameters<string> query, string parentProject, string testCode)
        {
            var queryTimeCode = _context.TimeCodeValids
                .AsNoTracking()
                .Where(t => t.ParentProject == parentProject && t.TestCode == testCode);

            // Apply filtering
            queryTimeCode = ApplyTimeCodeFilter(queryTimeCode, query.Filter);

            // Apply sorting
            queryTimeCode = (IQueryable<TimeCodeValid>)ApplySorting(queryTimeCode, query.SortBy, query.Descending);

            // Apply paging
            return await ApplyPaging(queryTimeCode, query.Page, query.PageSize);
        }

        public async Task<TimeCodeValid?> GetTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
        {
            return await _context.TimeCodeValids
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.WorkGroup.ToLower() == workGroup.ToLower() &&
                    t.TimeCode.ToLower() == timeCode.ToLower() &&
                    t.ParentProject.ToLower() == parentProject.ToLower());
        }

        public async Task<TimeCodeValid> CreateTimeCodeValidAsync(TimeCodeValid timeCodeValid)
        {
            timeCodeValid.FpsYear = _fpsRequestContext.FpsYear;
            await _context.TimeCodeValids.AddAsync(timeCodeValid);
            await _context.SaveChangesAsync();
            return timeCodeValid;
        }

        public async Task<TimeCodeValid> UpdateTimeCodeValidAsync(TimeCodeValid timeCodeValid)
        {
            timeCodeValid.FpsYear = _fpsRequestContext.FpsYear;
            _context.Entry(timeCodeValid).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return timeCodeValid;
        }

        public async Task<bool> DeleteTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
        {
            var entity = await _context.TimeCodeValids
                .FirstOrDefaultAsync(t =>
                    t.WorkGroup == workGroup &&
                    t.TimeCode == timeCode &&
                    t.ParentProject == parentProject &&
                    t.FpsYear == _fpsRequestContext.FpsYear);
            if (entity == null) return false;
            _context.TimeCodeValids.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllByJobCodeAsync(string jobCode, string parentProject)
        {
            var entities = await _context.TimeCodeValids
                .Where(t => t.JobCode == jobCode &&
                            t.ParentProject == parentProject &&
                            t.FpsYear == _fpsRequestContext.FpsYear)
                .ToListAsync();
            if (entities.Count > 0)
            {
                _context.TimeCodeValids.RemoveRange(entities);
                await _context.SaveChangesAsync();
            }            
            return true;
        }

        public async Task<IEnumerable<TimeCodeValid>> CopyWorkGroupAsync(
            string sourceJobCode, string targetJobCode, string parentProject)
        {
            var sourceEntries = await _context.TimeCodeValids
                .AsNoTracking()
                .Where(t => t.JobCode == sourceJobCode && t.ParentProject == parentProject)
                .ToListAsync();

            var existingWorkGroups = await _context.TimeCodeValids
                .AsNoTracking()
                .Where(t => t.JobCode == targetJobCode && t.ParentProject == parentProject)
                .Select(t => t.WorkGroup)
                .ToHashSetAsync();

            var copies = sourceEntries
                .Where(s => !existingWorkGroups.Contains(s.WorkGroup))
                .Select(s => new TimeCodeValid
                {
                    TimeCode = targetJobCode,
                    WorkGroup = s.WorkGroup,
                    ParentProject = parentProject,
                    JobCode = targetJobCode,
                    Active = s.Active,
                    FpsYear = _fpsRequestContext.FpsYear
                }).ToList();

            if (copies.Count > 0)
            {
                await _context.TimeCodeValids.AddRangeAsync(copies);
                await _context.SaveChangesAsync();
            }

            return copies;
        }

        public async Task<bool> DeleteBulkAsync(IEnumerable<(string WorkGroup, string TimeCode)> items, string parentProject)
        {
            var itemList = items.ToList();
            var workGroups = itemList.Select(i => i.WorkGroup).ToList();
            var timeCodes = itemList.Select(i => i.TimeCode).ToList();

            var entities = await _context.TimeCodeValids
                .Where(t => t.ParentProject == parentProject &&
                            t.FpsYear == _fpsRequestContext.FpsYear &&
                            workGroups.Contains(t.WorkGroup) &&
                            timeCodes.Contains(t.TimeCode))
                .ToListAsync();

            // Filter to exact (WorkGroup, TimeCode) pairs
            var toDelete = entities
                .Where(e => itemList.Any(i => i.WorkGroup == e.WorkGroup && i.TimeCode == e.TimeCode))
                .ToList();

            if (toDelete.Count != 0)
            {
                _context.TimeCodeValids.RemoveRange(toDelete);
                await _context.SaveChangesAsync();
            }
            return true;
        }

        public async Task<IEnumerable<TimeCodeValid>> CopySelectedWorkGroupsAsync(
            IEnumerable<string> workGroups, string sourceJobCode, string targetJobCode, string parentProject)
        {
            var workGroupList = workGroups.ToList();
            var sourceEntries = await _context.TimeCodeValids
                .AsNoTracking()
                .Where(t => t.JobCode == sourceJobCode &&
                            t.ParentProject == parentProject &&
                            workGroupList.Contains(t.WorkGroup))
                .ToListAsync();

            var existingWorkGroups = await _context.TimeCodeValids
                .AsNoTracking()
                .Where(t => t.JobCode == targetJobCode && t.ParentProject == parentProject)
                .Select(t => t.WorkGroup)
                .ToHashSetAsync();

            var copies = sourceEntries
                .Where(s => !existingWorkGroups.Contains(s.WorkGroup))
                .Select(s => new TimeCodeValid
                {
                    TimeCode = targetJobCode,
                    WorkGroup = s.WorkGroup,
                    ParentProject = parentProject,
                    JobCode = targetJobCode,
                    Active = s.Active,
                    FpsYear = _fpsRequestContext.FpsYear
                }).ToList();

            if (copies.Count > 0)
            {
                await _context.TimeCodeValids.AddRangeAsync(copies);
                await _context.SaveChangesAsync();
            }

            return copies;
        }

        private static IQueryable<TimeCodeValid> ApplyTimeCodeFilter(IQueryable<TimeCodeValid> queryTimeCode, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return queryTimeCode;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return queryTimeCode;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("TimeCode", out var timeCode) && timeCode != null)
                queryTimeCode = queryTimeCode.Where(x => EF.Functions.ILike(x.TimeCode, $"%{timeCode}%"));

            if (dict.TryGetValue("ParentProject", out var parentProject) && parentProject != null)
                queryTimeCode = queryTimeCode.Where(x => x.ParentProject != null && EF.Functions.ILike(x.ParentProject, $"%{parentProject}%"));

            if (dict.TryGetValue("Project", out var project) && project != null)
                queryTimeCode = queryTimeCode.Where(x => x.ParentProject != null && EF.Functions.ILike(x.ParentProject, $"%{project}%"));

            if (dict.TryGetValue("WorkGroup", out var workGroup) && workGroup != null)
                queryTimeCode = queryTimeCode.Where(x => x.WorkGroup != null && EF.Functions.ILike(x.WorkGroup, $"%{workGroup}%"));

            if (dict.TryGetValue("JobCode", out var jobCode) && jobCode != null)
                queryTimeCode = queryTimeCode.Where(x => x.JobCode != null && EF.Functions.ILike(x.JobCode, $"%{jobCode}%"));

            if (dict.TryGetValue("TestCode", out var testCode) && testCode != null)
                queryTimeCode = queryTimeCode.Where(x => x.TestCode != null && EF.Functions.ILike(x.TestCode, $"%{testCode}%"));

            if (dict.TryGetValue("Portfolio", out var portfolio) && portfolio != null)
                queryTimeCode = queryTimeCode.Where(x => x.Portfolio != null && EF.Functions.ILike(x.Portfolio, $"%{portfolio}%"));

            return queryTimeCode;
        }

        private static IQueryable ApplySorting(IQueryable<TimeCodeValid> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(e => e.TimeCode);
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<TimeCodeValid> query, string property, bool descending)
        {
            return property switch
            {
                "timecode" => ApplyOrder(query, i => i.TimeCode, descending),
                "workgroup" => ApplyOrder(query, i => i.WorkGroup, descending),
                "parentproject" => ApplyOrder(query, i => i.ParentProject, descending),
                "jobcode" => ApplyOrder(query, i => i.JobCode, descending),
                "active" => ApplyOrder(query, i => i.Active, descending),
                "testcode" => ApplyOrder(query, i => i.TestCode, descending),
                "portfolio" => ApplyOrder(query, i => i.Portfolio, descending),
                "project" => ApplyOrder(query, i => i.ParentProject, descending),
                _ => query.OrderBy(e => e.TimeCode)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<TimeCodeValid> query, Expression<Func<TimeCodeValid, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        public async Task<bool> HasRelatedTimeCodeValidRecordsAsync(string jobCode)
        {
            return await _context.TimeCodeValids
                .AsNoTracking()
                .AnyAsync(t => t.JobCode == jobCode);
        }
    }
}
