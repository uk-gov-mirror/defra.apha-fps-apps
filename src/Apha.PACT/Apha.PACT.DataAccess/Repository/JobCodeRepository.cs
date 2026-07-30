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
    public class JobCodeRepository : BaseRepository, IJobCodeRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;

        public JobCodeRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        public async Task<IEnumerable<JobCode>> GetJobCodesAsync()
        {
            return await _context.JobCodes
                .AsNoTracking()
                .OrderBy(j => j.JobCodeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobCode>> GetJobCodesByProjectAsync(string parentProject)
        {
            return await _context.JobCodes
                .AsNoTracking()
                .Where(j => j.ParentProject == parentProject)
                .OrderBy(j => j.JobCodeId)
                .ToListAsync();
        }

        public async Task<PagedData<JobCode>> GetPagedJobCodesAsync(
            PaginationParameters<string> query, string? parentProject)
        {
            var queryJobcodes = _context.JobCodes.AsNoTracking().AsQueryable();

            if(!string.IsNullOrEmpty(parentProject))
            {
                queryJobcodes = queryJobcodes.Where(j => j.ParentProject == parentProject);
            }

            // Apply filtering
            queryJobcodes = ApplyJobCodeFilter(queryJobcodes, query.Filter);

            // Apply sorting
            queryJobcodes = (IQueryable<JobCode>)ApplySorting(queryJobcodes, query.SortBy, query.Descending);

            // Apply paging
            return await ApplyPaging(queryJobcodes, query.Page, query.PageSize);
        }

        public async Task<JobCode?> GetJobCodeByIdAsync(string jobCodeId)
        {
            return await _context.JobCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(j => !string.IsNullOrEmpty(j.JobCodeId) && !string.IsNullOrEmpty(jobCodeId) 
                && j.JobCodeId.ToLower() == jobCodeId.ToLower());
        }

        public async Task<IEnumerable<string>> GetTypesAsync()
        {
            return await _context.JobCodes
                .AsNoTracking()
                .Where(j => j.Type != null)
                .Select(j => j.Type!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        public async Task<JobCode> CreateJobCodeAsync(JobCode jobCode)
        {
            jobCode.FpsYear = _fpsRequestContext.FpsYear;
            await _context.JobCodes.AddAsync(jobCode);
            await _context.SaveChangesAsync();
            return jobCode;
        }

        public async Task<JobCode> UpdateJobCodeAsync(JobCode jobCode)
        {
            jobCode.FpsYear = _fpsRequestContext.FpsYear;
            _context.Entry(jobCode).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return jobCode;
        }

        public async Task<bool> DeleteJobCodeAsync(string jobCodeId)
        {
            var jobCode = await _context.JobCodes
                .FirstOrDefaultAsync(j => j.JobCodeId == jobCodeId && j.FpsYear == _fpsRequestContext.FpsYear);
            if (jobCode == null) return false;
            _context.JobCodes.Remove(jobCode);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<JobCodeZtLookup>> GetZtJobCodesAsync()
        {
            var baseQuery = (from jc in _context.ProjectViews
                             where jc.Program != null && EF.Functions.ILike(jc.Program.ToLower(), "zt_prog")
                             && jc.UserEmail != null && EF.Functions.ILike(jc.UserEmail, _fpsRequestContext.UserEmailId)
                             select new JobCodeZtLookup
                             {
                                 JobCode = jc.ParentProject,
                                 Description = jc.ProjectTitle
                             }).Distinct().AsQueryable();

            return await baseQuery.AsNoTracking().ToListAsync();
        }

        private static IQueryable<JobCode> ApplyJobCodeFilter(IQueryable<JobCode> queryJobCode, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return queryJobCode;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return queryJobCode;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("JobCodeId", out var jobCodeId) && jobCodeId != null)
                queryJobCode = queryJobCode.Where(x => EF.Functions.ILike(x.JobCodeId, $"%{jobCodeId}%"));

            if (dict.TryGetValue("ParentProject", out var parentProject) && parentProject != null)
                queryJobCode = queryJobCode.Where(x => EF.Functions.ILike(x.ParentProject!, $"%{parentProject}%"));

            if (dict.TryGetValue("JobCodeWorkGroup", out var workGroup) && workGroup != null)
                queryJobCode = queryJobCode.Where(x => EF.Functions.ILike(x.JobCodeWorkGroup!, $"%{workGroup}%"));

            if (dict.TryGetValue("Type", out var type) && type != null)
                queryJobCode = queryJobCode.Where(x => EF.Functions.ILike(x.Type!, $"%{type}%"));

            if (dict.TryGetValue("JobCodeName", out var jobCodeName) && jobCodeName != null)
                queryJobCode = queryJobCode.Where(x => EF.Functions.ILike(x.JobCodeName!, $"%{jobCodeName}%"));

            return queryJobCode;
        }

        private static IQueryable ApplySorting(IQueryable<JobCode> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(e => e.JobCodeId);
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<JobCode> query, string property, bool descending)
        {
            return property switch
            {
                "jobcodeid" => ApplyOrder(query, i => i.JobCodeId, descending),
                "parentproject" => ApplyOrder(query, i => i.ParentProject, descending),
                "jobcodeworkgroup" => ApplyOrder(query, i => i.JobCodeWorkGroup, descending),
                "type" => ApplyOrder(query, i => i.Type, descending),
                "jobcodename" => ApplyOrder(query, i => i.JobCodeName, descending),
                _ => query.OrderBy(e => e.JobCodeId)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<JobCode> query, Expression<Func<JobCode, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
