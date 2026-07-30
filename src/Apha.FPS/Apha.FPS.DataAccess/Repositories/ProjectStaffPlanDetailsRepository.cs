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
    public class ProjectStaffPlanDetailsRepository : BaseRepository, IProjectStaffPlanDetailsRepository
    {
        public ProjectStaffPlanDetailsRepository(FpsDbContext context) : base(context) { }

        public async Task<PagedData<ProjectStaffPlanDetailsView>> GetPagedAsync(PaginationParameters<string> query)
        {
            // FpsYear is applied automatically by the global query filter on the view.
            var baseQuery = _context.ProjectStaffPlanDetailsViews.AsNoTracking();

            baseQuery = ApplyFilter(baseQuery, query.Filter);
            baseQuery = (IQueryable<ProjectStaffPlanDetailsView>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        private static IQueryable<ProjectStaffPlanDetailsView> ApplyFilter(IQueryable<ProjectStaffPlanDetailsView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            query = ApplyLike(query, dict, "ProfitCentre", (q, v) => q.Where(x => EF.Functions.ILike(x.ProfitCentre!, $"%{v}%")));
            query = ApplyLike(query, dict, "Program", (q, v) => q.Where(x => EF.Functions.ILike(x.Program!, $"%{v}%")));
            query = ApplyLike(query, dict, "Name", (q, v) => q.Where(x => EF.Functions.ILike(x.Name!, $"%{v}%")));
            query = ApplyLike(query, dict, "Manager", (q, v) => q.Where(x => EF.Functions.ILike(x.Manager!, $"%{v}%")));
            query = ApplyLike(query, dict, "ProjectStatus", (q, v) => q.Where(x => EF.Functions.ILike(x.ProjectStatus!, $"%{v}%")));
            query = ApplyLike(query, dict, "WorkGroup", (q, v) => q.Where(x => EF.Functions.ILike(x.WorkGroup!, $"%{v}%")));
            query = ApplyLike(query, dict, "GradeCode", (q, v) => q.Where(x => EF.Functions.ILike(x.GradeCode!, $"%{v}%")));

            return query;
        }

        private static IQueryable<ProjectStaffPlanDetailsView> ApplyLike(
            IQueryable<ProjectStaffPlanDetailsView> query,
            IDictionary<string, object> dict,
            string key,
            Func<IQueryable<ProjectStaffPlanDetailsView>, object, IQueryable<ProjectStaffPlanDetailsView>> apply)
        {
            if (dict.TryGetValue(key, out var value) && value != null)
                return apply(query, value);

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<ProjectStaffPlanDetailsView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.ProfitCentre).ThenBy(x => x.WorkGroup).ThenBy(x => x.Program);
            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<ProjectStaffPlanDetailsView> query, string property, bool descending)
        {
            return property switch
            {
                "program"       => ApplyOrder(query, x => x.Program,       descending),
                "name"          => ApplyOrder(query, x => x.Name,          descending),
                "manager"       => ApplyOrder(query, x => x.Manager,       descending),
                "projectstatus" => ApplyOrder(query, x => x.ProjectStatus, descending),
                "profitcentre"  => ApplyOrder(query, x => x.ProfitCentre,  descending),
                "workgroup"     => ApplyOrder(query, x => x.WorkGroup,     descending),
                "gradecode"     => ApplyOrder(query, x => x.GradeCode,     descending),
                "plannedhours"  => ApplyOrder(query, x => x.PlannedHours,  descending),
                "cost"          => ApplyOrder(query, x => x.Cost,          descending),
                _               => query.OrderBy(x => x.ProfitCentre).ThenBy(x => x.WorkGroup).ThenBy(x => x.Program)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<ProjectStaffPlanDetailsView> query,
            Expression<Func<ProjectStaffPlanDetailsView, T>> keySelector, bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
