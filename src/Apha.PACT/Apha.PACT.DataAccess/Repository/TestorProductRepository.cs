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
    public class TestorProductRepository : BaseRepository, ITestorProductRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;
        private readonly FpsDbContext _dbContext;

        public TestorProductRepository(FpsDbContext dbContext, IFpsRequestContext fpsRequestContext) : base(dbContext)
        {
            _fpsRequestContext = fpsRequestContext;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<TestorProduct>> GetAllTestorProductsAsync()
        {
            return await _dbContext.TestorProducts
                .AsNoTracking()
                .OrderBy(t => t.ItemCode)
                .ToListAsync();
        }

        public async Task<PagedData<TestorProduct>> GetPagedTestOrProductsAsync(PaginationParameters<string> parameters)
        {
            var query = _context.TestorProducts.AsNoTracking().AsQueryable();

            query = ApplyTestOrProductFilter(query, parameters.Filter);
            query = ApplySorting(query, parameters.SortBy, parameters.Descending);

            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
        }

        public async Task<TestorProduct?> GetTestOrProductByIdAsync(string itemCode)
        {
            return await _context.TestorProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ItemCode.ToLower() == itemCode.ToLower());
        }

        public async Task<TestorProduct> CreateTestOrProductAsync(TestorProduct entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            await _context.TestorProducts.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<TestorProduct> UpdateTestOrProductAsync(TestorProduct entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteTestOrProductAsync(string itemCode)
        {
            var entity = await _context.TestorProducts
                .FirstOrDefaultAsync(t => t.ItemCode == itemCode && t.FpsYear == _fpsRequestContext.FpsYear);
            if (entity == null) return false;
            _context.TestorProducts.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<string>> GetOwnersAsync()
        {
            return await _context.TestorProducts
                .AsNoTracking()
                .Where(t => t.Owner != null)
                .Select(t => t.Owner!)
                .Distinct()
                .OrderBy(o => o)
                .ToListAsync();
        }

        public async Task<Dictionary<string, string?>> GetDescriptionsByCodesAsync(IEnumerable<string> itemCodes)
        {
            var codes = itemCodes.ToList();
            return await _context.TestorProducts
                .AsNoTracking()
                .Where(t => codes.Contains(t.ItemCode))
                .ToDictionaryAsync(t => t.ItemCode, t => t.ItemDescription);
        }

        private static IQueryable<TestorProduct> ApplyTestOrProductFilter(IQueryable<TestorProduct> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return query;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return query;
            }

            var dict = (IDictionary<string, object>)filterModel;

            query = ApplyILikeFilter(dict, "ItemCode", query, (q, v) => q.Where(x => EF.Functions.ILike(x.ItemCode, v)));
            query = ApplyILikeFilter(dict, "ItemDescription", query, (q, v) => q.Where(x => x.ItemDescription != null && EF.Functions.ILike(x.ItemDescription, v)));
            query = ApplyILikeFilter(dict, "ShortDescription", query, (q, v) => q.Where(x => x.ShortDescription != null && EF.Functions.ILike(x.ShortDescription, v)));
            query = ApplyILikeFilter(dict, "Owner", query, (q, v) => q.Where(x => x.Owner != null && EF.Functions.ILike(x.Owner, v)));
            query = ApplyILikeFilter(dict, "TestManager", query, (q, v) => q.Where(x => x.TestManager != null && EF.Functions.ILike(x.TestManager, v)));
            query = ApplyILikeFilter(dict, "JobStatus", query, (q, v) => q.Where(x => x.JobStatus != null && EF.Functions.ILike(x.JobStatus, v)));

            return query;
        }

        private static IQueryable<TestorProduct> ApplyILikeFilter(
            IDictionary<string, object> dict,
            string key,
            IQueryable<TestorProduct> query,
            Func<IQueryable<TestorProduct>, string, IQueryable<TestorProduct>> applyWhere)
        {
            if (dict.TryGetValue(key, out var value) && value != null)
                query = applyWhere(query, $"%{value}%");
            return query;
        }

        private static IQueryable<TestorProduct> ApplySorting(IQueryable<TestorProduct> query, string? sortBy, bool descending)
        {
            var sortMap = new Dictionary<string, Expression<Func<TestorProduct, object?>>>
            {
                ["itemcode"] = e => e.ItemCode,
                ["itemdescription"] = e => e.ItemDescription,
                ["shortdescription"] = e => e.ShortDescription,
                ["owner"] = e => e.Owner,
                ["testmanager"] = e => e.TestManager,
                ["jobstatus"] = e => e.JobStatus,
                ["unitpricevla"] = e => e.UnitPriceVla,
                ["defraunitprice"] = e => e.DefraUnitPrice,
            };

            var key = sortBy?.ToLower() ?? string.Empty;
            if (!sortMap.TryGetValue(key, out var keySelector))
                keySelector = e => e.ItemCode;

            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        // ── TestPriceCheck (frmTestPriceCheck — qryTestPriceZero) ──────────────────────────────

        public async Task<PagedData<TestPriceCheckView>> GetTestPriceCheckPagedAsync(
            PaginationParameters<string> query,
            string priceFilter,
            string? owner)
        {
            // Step 1 — IQueryable: build base join query.
            var baseQuery = BuildTestPriceCheckBaseQuery();            

            baseQuery = ApplyTestPriceCheckFilter(baseQuery, query.Filter);

            // Step 3 — SQL-side owner dropdown filter (exact match)
            if (!string.IsNullOrWhiteSpace(owner))
                baseQuery = baseQuery.Where(x => x.Owner == owner);

            // Step 4 — SQL-side price filter
            // NormalPrice expression: CASE WHEN IsDefraProject != 0 THEN DefraUnitPrice ELSE UnitPriceVla END
            baseQuery = priceFilter switch
            {
                "zero" => baseQuery.Where(x => x.TestPrice == 0m),
                "non-standard" => baseQuery.Where(x =>
                    x.TestPrice != 0m &&
                    x.TestPrice != (x.IsDefraProject != 0 ? x.DefraUnitPrice : x.UnitPriceVla)),
                _ => baseQuery
            };

            // Step 5 — SQL-side sorting
            var sorted = ApplyTestPriceCheckSorting(baseQuery, query.SortBy, query.Descending);           

            // Step 6 — SQL-side paging (COUNT + LIMIT/OFFSET)
            var paged = await ApplyPaging(sorted, query.Page, query.PageSize);

            // Step 7 — Compute derived fields on paged subset only
            foreach (var row in paged.Data)
            {
                row.NormalPrice = row.IsDefraProject != 0 ? row.DefraUnitPrice : row.UnitPriceVla;
                row.IsZeroPrice = row.TestPrice == 0m;
                row.IsNotStandard = row.TestPrice != row.NormalPrice;
            }

            return paged;
        }

        public async Task<TestPriceCheckView?> GetTestPriceCheckByKeyAsync(string testCode, string jobCode)
        {
            var row = await BuildTestPriceCheckBaseQuery()
                .FirstOrDefaultAsync(x => x.TestCode == testCode && x.JobCode == jobCode);

            if (row == null) return null;

            row.NormalPrice   = row.IsDefraProject != 0 ? row.DefraUnitPrice : row.UnitPriceVla;
            row.IsZeroPrice   = row.TestPrice == 0m;
            row.IsNotStandard = row.TestPrice != row.NormalPrice;
            return row;
        }

        public async Task<bool> UpdateTestPriceCheckAsync(
            string testCode, string jobCode, short isDefraProject, decimal? testPrice, decimal? defraUnitPrice)
        {
            await _context.Projects
                .Where(p => p.ParentProject == jobCode)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsDefraProject, isDefraProject));

            await _context.TestRequirements
                .Where(r => r.TestCode == testCode && r.Buyer == jobCode)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.UnitPrice, testPrice));

            await _context.TestorProducts
                .Where(tp => tp.ItemCode == testCode)
                .ExecuteUpdateAsync(s => s.SetProperty(tp => tp.DefraUnitPrice, defraUnitPrice));

            return true;
        }

        private IQueryable<TestPriceCheckView> BuildTestPriceCheckBaseQuery()
        {
            return (from tr in _context.TestRequirements
                    join p  in _context.ProjectViews   on tr.Buyer    equals p.ParentProject
                    join tp in _context.TestorProducts on tr.TestCode equals tp.ItemCode
                    where(EF.Functions.ILike(p.UserEmail!, _fpsRequestContext.UserEmailId))
                    select new TestPriceCheckView
                    {
                        TestCode       = tr.TestCode,
                        JobCode        = tr.Buyer,
                        NoTests        = tr.NoRequired,
                        TestPrice      = tr.UnitPrice,
                        UnitPriceVla   = tp.UnitPriceVla,
                        DefraUnitPrice = tp.DefraUnitPrice,
                        Program        = p.Program,
                        Manager        = p.Manager,
                        Owner          = tp.Owner,
                        IsDefraProject = p.IsDefraProject ?? 0,
                        FpsYear        = tr.FpsYear
                    }).Distinct().AsNoTracking();
        }

        private static IQueryable<TestPriceCheckView> ApplyTestPriceCheckSorting(
            IQueryable<TestPriceCheckView> source, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "jobcode"        => ApplyOrder(source, x => x.JobCode,       descending),
                "manager"        => ApplyOrder(source, x => x.Manager,       descending),
                "program"        => ApplyOrder(source, x => x.Program,       descending),
                "notests"        => ApplyOrder(source, x => x.NoTests,       descending),
                "testprice"      => ApplyOrder(source, x => x.TestPrice,     descending),
                "normalprice"    => ApplyOrder(source, x => x.IsDefraProject != 0 ? x.DefraUnitPrice : x.UnitPriceVla, descending),
                "unitpricevla"   => ApplyOrder(source, x => x.UnitPriceVla,  descending),
                "defraunitprice" => ApplyOrder(source, x => x.DefraUnitPrice, descending),
                "owner"          => ApplyOrder(source, x => x.Owner,         descending),
                _                => ApplyOrder(source, x => x.TestCode,      descending),
            };
        }

        private static IQueryable<T> ApplyOrder<T, TKey>(
            IQueryable<T> source, Expression<Func<T, TKey>> keySelector, bool descending)
            => descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);

        private static IQueryable<TestPriceCheckView> ApplyTestPriceCheckFilter(
            IQueryable<TestPriceCheckView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("TestCode", out var tc) && tc != null)
                query = query.Where(x => EF.Functions.ILike(x.TestCode, $"%{tc}%"));
            if (dict.TryGetValue("JobCode", out var jc) && jc != null)
                query = query.Where(x => EF.Functions.ILike(x.JobCode, $"%{jc}%"));
            if (dict.TryGetValue("Owner", out var ow) && ow != null)
                query = query.Where(x => x.Owner != null && EF.Functions.ILike(x.Owner, $"%{ow}%"));
            if (dict.TryGetValue("Program", out var pg) && pg != null)
                query = query.Where(x => x.Program != null && EF.Functions.ILike(x.Program, $"%{pg}%"));
            if (dict.TryGetValue("Manager", out var mg) && mg != null)
                query = query.Where(x => x.Manager != null && EF.Functions.ILike(x.Manager, $"%{mg}%"));

            return query;
        }

    }
}
