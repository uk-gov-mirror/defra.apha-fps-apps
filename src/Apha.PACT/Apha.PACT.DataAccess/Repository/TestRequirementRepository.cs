using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PACT.DataAccess.Repository
{
    public class TestRequirementRepository : BaseRepository, ITestRequirementRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;

        public TestRequirementRepository(
            FpsDbContext context,
            IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        public async Task<PagedData<TestRequirement>> GetPagedByTestCodeAsync(
            PaginationParameters<string> query, string testCode)
        {
            var baseQuery = _context.TestRequirements
                .AsNoTracking()
                .Where(t => t.TestCode == testCode)
                .AsQueryable();

            baseQuery = ApplyTestReqmtFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                baseQuery = query.Descending
                    ? baseQuery.OrderByDescending(e => EF.Property<object>(e, query.SortBy))
                    : baseQuery.OrderBy(e => EF.Property<object>(e, query.SortBy));
            else
                baseQuery = baseQuery.OrderBy(t => t.Buyer);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<PagedData<TestSupplierView>> GetPagedBySupplierTestCodeAsync(
            PaginationParameters<string> query, string testCode, bool showRejected)
        {
            // TestCost (money * int) cannot be computed in SQL on PostgreSQL due to
            // money-type casting restrictions; it is calculated client-side after fetch.
            var baseQuery = (from tr in _context.TestRequirements
                             join p in _context.Projects on tr.Buyer equals p.ParentProject
                             where tr.TestCode == testCode
                             && (showRejected || tr.Active == 1)
                             select new TestSupplierView
                             {
                                 TestCode = tr.TestCode,
                                 Buyer = tr.Buyer,
                                 ProjectManager = p.Manager,
                                 NoRequired = tr.NoRequired,
                                 UnitPrice = tr.UnitPrice,
                                 TestCost = null,
                                 ProjectStatus = p.ProjectStatus
                             }).AsQueryable();

            baseQuery = ApplySupplierFilter(baseQuery, query.Filter);

            // TestCost sort is deferred to client-side below; all other sorts run in DB.
            bool sortByTestCost = string.Equals(query.SortBy, nameof(TestSupplierView.TestCost),
                StringComparison.Ordinal);

            if (!sortByTestCost)
                baseQuery = ApplySupplierDbSort(baseQuery, query.SortBy, query.Descending);

            var rows = await baseQuery.ToListAsync();

            // Compute TestCost client-side to avoid PostgreSQL money-type cast error.
            foreach (var row in rows)
            {
                row.TestCost = row.NoRequired.HasValue && row.UnitPrice.HasValue
                    ? (decimal)row.NoRequired.Value * row.UnitPrice.Value
                    : null;
            }

            // Apply TestCost sort in memory now that the computed value is available.
            IEnumerable<TestSupplierView> sortedByTestCost = query.Descending
                ? rows.OrderByDescending(t => t.TestCost)
                : rows.OrderBy(t => t.TestCost);
            IEnumerable<TestSupplierView> result = sortByTestCost ? sortedByTestCost : rows;

            return ApplyPagingInMemory(result.ToList(), query.Page, query.PageSize);
        }

        public async Task<PagedData<TestRequirementDetail>> GetPagedWithDetailsAsync(
            PaginationParameters<string> query, string testCode)
        {
            var baseQuery = (from t in _context.TestRequirements
                             join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode
                             join p in _context.Projects on t.Buyer equals p.ParentProject
                             where t.TestCode == testCode
                             select new TestRequirementDetail
                             {
                                 TestCode = t.TestCode,
                                 ItemDescription = tp.ItemDescription,
                                 Buyer = t.Buyer,
                                 UnitPrice = t.UnitPrice,
                                 NoRequired = t.NoRequired,
                                 ProjectBuyerCode = t.ProjectBuyerCode,
                                 TestBuyerCode = t.TestBuyerCode,
                                 DateCreated = t.DateCreated,
                                 Active = t.Active,
                                 FpsYear = t.FpsYear,
                                 IsDefraProject = p.IsDefraProject,
                                 RecUnitPrice = p.IsDefraProject == 0 ? tp.UnitPriceVla : (decimal?)tp.DefraUnitPrice
                             }).AsQueryable();

            baseQuery = ApplyTestReqmtDetailFilter(baseQuery, query.Filter);

            baseQuery = (!string.IsNullOrWhiteSpace(query.SortBy), query.Descending) switch
            {
                (true, true) => query.SortBy switch
                {
                    nameof(TestRequirementDetail.Buyer) => baseQuery.OrderByDescending(t => t.Buyer),
                    nameof(TestRequirementDetail.UnitPrice) => baseQuery.OrderByDescending(t => t.UnitPrice),
                    nameof(TestRequirementDetail.NoRequired) => baseQuery.OrderByDescending(t => t.NoRequired),
                    nameof(TestRequirementDetail.Active) => baseQuery.OrderByDescending(t => t.Active),
                    nameof(TestRequirementDetail.ProjectBuyerCode) => baseQuery.OrderByDescending(t => t.ProjectBuyerCode),
                    nameof(TestRequirementDetail.IsDefraProject) => baseQuery.OrderByDescending(t => t.IsDefraProject),
                    nameof(TestRequirementDetail.RecUnitPrice) => baseQuery.OrderByDescending(t => t.RecUnitPrice),
                    _ => baseQuery.OrderByDescending(t => t.TestCode)
                },
                (true, false) => query.SortBy switch
                {
                    nameof(TestRequirementDetail.Buyer) => baseQuery.OrderBy(t => t.Buyer),
                    nameof(TestRequirementDetail.UnitPrice) => baseQuery.OrderBy(t => t.UnitPrice),
                    nameof(TestRequirementDetail.NoRequired) => baseQuery.OrderBy(t => t.NoRequired),
                    nameof(TestRequirementDetail.Active) => baseQuery.OrderBy(t => t.Active),
                    nameof(TestRequirementDetail.ProjectBuyerCode) => baseQuery.OrderBy(t => t.ProjectBuyerCode),
                    nameof(TestRequirementDetail.IsDefraProject) => baseQuery.OrderBy(t => t.IsDefraProject),
                    nameof(TestRequirementDetail.RecUnitPrice) => baseQuery.OrderBy(t => t.RecUnitPrice),
                    _ => baseQuery.OrderBy(t => t.TestCode)
                },
                _ => baseQuery.OrderBy(t => t.TestCode)
            };

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<PagedData<TestRequirementDetail>> GetPagedByProjectAsync(
            PaginationParameters<string> query, string parentProject)
        {
            var baseQuery = (from t in _context.TestRequirements
                             join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode
                             join p in _context.Projects on t.Buyer equals p.ParentProject
                             where t.Buyer != null && parentProject != null && t.Buyer.ToLower() == parentProject.ToLower()
                             select new TestRequirementDetail
                             {
                                 TestCode = t.TestCode,
                                 ItemDescription = tp.ItemDescription,
                                 Buyer = t.Buyer,
                                 UnitPrice = t.UnitPrice,
                                 NoRequired = t.NoRequired,
                                 ProjectBuyerCode = t.ProjectBuyerCode,
                                 TestBuyerCode = t.TestBuyerCode,
                                 DateCreated = t.DateCreated,
                                 Active = t.Active,
                                 FpsYear = t.FpsYear,
                                 IsDefraProject = p.IsDefraProject,
                                 RecUnitPrice = p.IsDefraProject == 0 ? tp.UnitPriceVla : (decimal?)tp.DefraUnitPrice
                             }).AsQueryable();

            baseQuery = ApplyTestReqmtDetailFilter(baseQuery, query.Filter);

            baseQuery = (!string.IsNullOrWhiteSpace(query.SortBy), query.Descending) switch
            {
                (true, true) => query.SortBy switch
                {
                    nameof(TestRequirementDetail.TestCode) => baseQuery.OrderByDescending(t => t.TestCode),
                    nameof(TestRequirementDetail.UnitPrice) => baseQuery.OrderByDescending(t => t.UnitPrice),
                    nameof(TestRequirementDetail.NoRequired) => baseQuery.OrderByDescending(t => t.NoRequired),
                    nameof(TestRequirementDetail.Active) => baseQuery.OrderByDescending(t => t.Active),
                    nameof(TestRequirementDetail.ProjectBuyerCode) => baseQuery.OrderByDescending(t => t.ProjectBuyerCode),
                    nameof(TestRequirementDetail.IsDefraProject) => baseQuery.OrderByDescending(t => t.IsDefraProject),
                    nameof(TestRequirementDetail.RecUnitPrice) => baseQuery.OrderByDescending(t => t.RecUnitPrice),
                    _ => baseQuery.OrderByDescending(t => t.TestCode)
                },
                (true, false) => query.SortBy switch
                {
                    nameof(TestRequirementDetail.TestCode) => baseQuery.OrderBy(t => t.TestCode),
                    nameof(TestRequirementDetail.UnitPrice) => baseQuery.OrderBy(t => t.UnitPrice),
                    nameof(TestRequirementDetail.NoRequired) => baseQuery.OrderBy(t => t.NoRequired),
                    nameof(TestRequirementDetail.Active) => baseQuery.OrderBy(t => t.Active),
                    nameof(TestRequirementDetail.ProjectBuyerCode) => baseQuery.OrderBy(t => t.ProjectBuyerCode),
                    nameof(TestRequirementDetail.IsDefraProject) => baseQuery.OrderBy(t => t.IsDefraProject),
                    nameof(TestRequirementDetail.RecUnitPrice) => baseQuery.OrderBy(t => t.RecUnitPrice),
                    _ => baseQuery.OrderBy(t => t.TestCode)
                },
                _ => baseQuery.OrderBy(t => t.TestCode)
            };

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<TestRequirementDetail>> GetAllForExportAsync(string testCode, string? filterJson)
        {
            var query = (from t in _context.TestRequirements
                         join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode
                         join p in _context.Projects on t.Buyer equals p.ParentProject
                         where t.TestCode == testCode
                         select new TestRequirementDetail
                         {
                             TestCode = t.TestCode,
                             ItemDescription = tp.ItemDescription,
                             Buyer = t.Buyer,
                             UnitPrice = t.UnitPrice,
                             NoRequired = t.NoRequired,
                             ProjectBuyerCode = t.ProjectBuyerCode,
                             TestBuyerCode = t.TestBuyerCode,
                             DateCreated = t.DateCreated,
                             Active = t.Active,
                             FpsYear = t.FpsYear,
                             IsDefraProject = p.IsDefraProject,
                             RecUnitPrice = p.IsDefraProject == 0 ? tp.UnitPriceVla : (decimal?)tp.DefraUnitPrice
                         }).AsQueryable();

            query = ApplyTestReqmtDetailFilter(query, filterJson);

            return await query.OrderBy(t => t.Buyer).ToListAsync();
        }

        public async Task<TestRequirement?> GetByIdAsync(string testCode, string buyer)
        {
            return await _context.TestRequirements
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TestCode == testCode && t.Buyer == buyer);
        }

        public async Task<TestRequirementDetail?> GetDetailByIdAsync(string testCode, string buyer)
        {
            return await (from t in _context.TestRequirements
                          join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode
                          join p in _context.Projects on t.Buyer equals p.ParentProject
                          where t.TestCode == testCode && t.Buyer == buyer
                          select new TestRequirementDetail
                          {
                              TestCode = t.TestCode,
                              ItemDescription = tp.ItemDescription,
                              Buyer = t.Buyer,
                              UnitPrice = t.UnitPrice,
                              NoRequired = t.NoRequired,
                              ProjectBuyerCode = t.ProjectBuyerCode,
                              TestBuyerCode = t.TestBuyerCode,
                              DateCreated = t.DateCreated,
                              Active = t.Active,
                              FpsYear = t.FpsYear,
                              IsDefraProject = p.IsDefraProject,
                              RecUnitPrice = p.IsDefraProject == 0 ? tp.UnitPriceVla : (decimal?)tp.DefraUnitPrice
                          }).FirstOrDefaultAsync();
        }

        // ── Actuals Tests with Planned Data (moved from TestActualBreakdownRepository) ──────

        public async Task<PagedData<TestActualBreakdownView>> GetActualsTestsWithPlannedDataByWorkgroupAsync(
            PaginationParameters<string> query)
        {
            var baseQuery = _context.TestActualBreakdownViews.AsNoTracking();
            baseQuery = ApplyActualBreakdownFilter(baseQuery, query.Filter);
            var sorted = ApplyActualBreakdownSorting(baseQuery, query.SortBy, query.Descending);
            return await ApplyPaging(sorted, query.Page, query.PageSize);
        }

        private static IQueryable<TestActualBreakdownView> ApplyActualBreakdownSorting(
            IQueryable<TestActualBreakdownView> source, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "testcode"         => ApplyOrder(source, x => x.TestCode,         descending),
                "shortdescription" => ApplyOrder(source, x => x.ShortDescription, descending),
                "program"          => ApplyOrder(source, x => x.Program,          descending),
                "buyer"            => ApplyOrder(source, x => x.Buyer,            descending),
                "portfolio"        => ApplyOrder(source, x => x.Portfolio,        descending),
                "workgroup"        => ApplyOrder(source, x => x.WorkGroup,        descending),
                "month"            => ApplyOrder(source, x => x.Month,            descending),
                "pcprice"          => ApplyOrder(source, x => x.PCPrice,          descending),
                "pccost"           => ApplyOrder(source, x => x.PCCost,           descending),
                "profitcentre"     => ApplyOrder(source, x => x.ProfitCentre,     descending),
                _                  => ApplyOrder(source, x => x.TestCode,         descending)
            };
        }

        private static IQueryable<TestActualBreakdownView> ApplyActualBreakdownFilter(
            IQueryable<TestActualBreakdownView> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filter);
            if (filters is null)
                return query;

            if (filters.TryGetValue("TestCode", out var testCode) && !string.IsNullOrWhiteSpace(testCode))
                query = query.Where(x => x.TestCode != null && EF.Functions.ILike(x.TestCode, $"%{testCode}%"));
            if (filters.TryGetValue("ShortDescription", out var shortDesc) && !string.IsNullOrWhiteSpace(shortDesc))
                query = query.Where(x => x.ShortDescription != null && EF.Functions.ILike(x.ShortDescription, $"%{shortDesc}%"));
            if (filters.TryGetValue("Program", out var program) && !string.IsNullOrWhiteSpace(program))
                query = query.Where(x => x.Program != null && EF.Functions.ILike(x.Program, $"%{program}%"));
            if (filters.TryGetValue("Buyer", out var buyer) && !string.IsNullOrWhiteSpace(buyer))
                query = query.Where(x => x.Buyer != null && EF.Functions.ILike(x.Buyer, $"%{buyer}%"));
            if (filters.TryGetValue("Portfolio", out var portfolio) && !string.IsNullOrWhiteSpace(portfolio))
                query = query.Where(x => x.Portfolio != null && EF.Functions.ILike(x.Portfolio, $"%{portfolio}%"));
            if (filters.TryGetValue("WorkGroup", out var workGroup) && !string.IsNullOrWhiteSpace(workGroup))
                query = query.Where(x => x.WorkGroup != null && EF.Functions.ILike(x.WorkGroup, $"%{workGroup}%"));
            if (filters.TryGetValue("ProfitCentre", out var profitCentre) && !string.IsNullOrWhiteSpace(profitCentre))
                query = query.Where(x => x.ProfitCentre != null && EF.Functions.ILike(x.ProfitCentre, $"%{profitCentre}%"));

            return query;
        }

        public async Task<TestRequirementDetail?> GetPricingAsync(string testCode, string? projectCode)
        {
            var tp = await _context.TestorProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ItemCode == testCode);

            if (tp is null) return null;

            // TestCode only — return DefraUnitPrice with no project context
            if (string.IsNullOrWhiteSpace(projectCode))
            {
                return new TestRequirementDetail
                {
                    TestCode = testCode,
                    RecUnitPrice = tp.DefraUnitPrice
                };
            }

            // TestCode + ProjectCode — apply IsDefraProject logic
            var p = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ParentProject == projectCode);

            if (p is null) return null;

            return new TestRequirementDetail
            {
                TestCode = testCode,
                Buyer = projectCode,
                IsDefraProject = p.IsDefraProject,
                RecUnitPrice = p.IsDefraProject == 0 ? tp.UnitPriceVla : (decimal?)tp.DefraUnitPrice
            };
        }

        public async Task<bool> ExistsAsync(string testCode, string buyer)
        {
            return await _context.TestRequirements
                .AsNoTracking()
                .AnyAsync(p => p.TestCode == testCode && p.Buyer == buyer);
        }

        public async Task<bool> ExistsByTestBuyerCodeAsync(string testBuyerCode)
        {
            return await _context.TestRequirements
                .AsNoTracking()
                .AnyAsync(r => r.TestBuyerCode == testBuyerCode);
        }

        public async Task<bool> ExistsByTestCodeAndBuyerInMonthlyOutputAsync(string testCode, string buyer)
        {
            return await _context.MonthlyOutputs
                .AsNoTracking()
                .AnyAsync(m => m.TestCode == testCode && m.Buyer == buyer);
        }

        public async Task<TestRequirement> AddAsync(TestRequirement entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            entity.DateCreated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await _context.TestRequirements.AddAsync(entity);
            await _context.SaveChangesAsync();
            await WriteAuditLogAsync(entity, "I");
            return entity;
        }

        public async Task<TestRequirement> UpdateAsync(TestRequirement entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await WriteAuditLogAsync(entity, "U");
            return entity;
        }

        public async Task<bool> DeleteAsync(string testCode, string buyer)
        {
            var entity = await _context.TestRequirements
                .FirstOrDefaultAsync(t =>
                    t.TestCode == testCode &&
                    t.Buyer == buyer &&
                    t.FpsYear == _fpsRequestContext.FpsYear);

            if (entity is null) return false;

            await WriteAuditLogAsync(entity, "D");
            _context.TestRequirements.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static IQueryable<TestSupplierView> ApplySupplierDbSort(
            IQueryable<TestSupplierView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderBy(t => t.Buyer);

            return (sortBy, descending) switch
            {
                (nameof(TestSupplierView.Buyer), true) => query.OrderByDescending(t => t.Buyer),
                (nameof(TestSupplierView.ProjectManager), true) => query.OrderByDescending(t => t.ProjectManager),
                (nameof(TestSupplierView.UnitPrice), true) => query.OrderByDescending(t => t.UnitPrice),
                (nameof(TestSupplierView.NoRequired), true) => query.OrderByDescending(t => t.NoRequired),
                (nameof(TestSupplierView.ProjectStatus), true) => query.OrderByDescending(t => t.ProjectStatus),
                (nameof(TestSupplierView.Buyer), false) => query.OrderBy(t => t.Buyer),
                (nameof(TestSupplierView.ProjectManager), false) => query.OrderBy(t => t.ProjectManager),
                (nameof(TestSupplierView.UnitPrice), false) => query.OrderBy(t => t.UnitPrice),
                (nameof(TestSupplierView.NoRequired), false) => query.OrderBy(t => t.NoRequired),
                (nameof(TestSupplierView.ProjectStatus), false) => query.OrderBy(t => t.ProjectStatus),
                _ => descending ? query.OrderByDescending(t => t.Buyer) : query.OrderBy(t => t.Buyer)
            };
        }

        private static IQueryable<TestSupplierView> ApplySupplierFilter(
            IQueryable<TestSupplierView> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue(nameof(TestSupplierView.Buyer), out string? buyer)
                && !string.IsNullOrWhiteSpace(buyer))
                query = query.Where(t => EF.Functions.ILike(t.Buyer, $"%{buyer}%"));

            if (filters.TryGetValue(nameof(TestSupplierView.TestCode), out string? testCode)
                && !string.IsNullOrWhiteSpace(testCode))
                query = query.Where(t => EF.Functions.ILike(t.TestCode, $"%{testCode}%"));

            if (filters.TryGetValue(nameof(TestSupplierView.ProjectManager), out string? projectManager)
                && !string.IsNullOrWhiteSpace(projectManager))
                query = query.Where(t => t.ProjectManager != null
                    && EF.Functions.ILike(t.ProjectManager, $"%{projectManager}%"));

            if (filters.TryGetValue(nameof(TestSupplierView.ProjectStatus), out string? status)
                && !string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.ProjectStatus != null
                    && EF.Functions.ILike(t.ProjectStatus, $"%{status}%"));

            return query;
        }

        private static IQueryable<TestRequirement> ApplyTestReqmtFilter(
            IQueryable<TestRequirement> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue("Buyer", out string? buyer) && !string.IsNullOrWhiteSpace(buyer))
                query = query.Where(t => EF.Functions.ILike(t.Buyer, $"%{buyer}%"));

            if (filters.TryGetValue("ProjectBuyerCode", out string? projectCode) && !string.IsNullOrWhiteSpace(projectCode))
                query = query.Where(t => t.ProjectBuyerCode != null && EF.Functions.ILike(t.ProjectBuyerCode, $"%{projectCode}%"));

            return query;
        }

        private static IQueryable<TestRequirementDetail> ApplyTestReqmtDetailFilter(
            IQueryable<TestRequirementDetail> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;
            
            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue("TestCode", out string? testcode) && !string.IsNullOrWhiteSpace(testcode))
                query = query.Where(t => EF.Functions.ILike(t.TestCode, $"%{testcode}%"));

            if (filters.TryGetValue("ItemDescription", out string? itemdescription) && !string.IsNullOrWhiteSpace(itemdescription))
                query = query.Where(t => t.ItemDescription != null && EF.Functions.ILike(t.ItemDescription, $"%{itemdescription}%"));

            if (filters.TryGetValue("Buyer", out string? buyer) && !string.IsNullOrWhiteSpace(buyer))
                query = query.Where(t => EF.Functions.ILike(t.Buyer, $"%{buyer}%"));

            if (filters.TryGetValue("ProjectBuyerCode", out string? projectCode) && !string.IsNullOrWhiteSpace(projectCode))
                query = query.Where(t => t.ProjectBuyerCode != null && EF.Functions.ILike(t.ProjectBuyerCode, $"%{projectCode}%"));

            return query;
        }

        // ── UITrig: INSERT/UPDATE → 'I'  |  DTrig: DELETE → 'D' ─────────────
        private async Task WriteAuditLogAsync(TestRequirement entity, string insertDelete)
        {
            var log = new TestRequirementLog
            {
                TestCode      = entity.TestCode,
                Buyer         = entity.Buyer,
                UnitPrice     = entity.UnitPrice.HasValue ? (double?)decimal.ToDouble(entity.UnitPrice.Value) : null,
                NoRequired    = entity.NoRequired.HasValue ? (int?)Convert.ToInt32(entity.NoRequired.Value) : null,
                DateTime      = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UserId        = _fpsRequestContext.UserEmailId,
                InsertDelete  = insertDelete,
                FpsYear       = _fpsRequestContext.FpsYear
            };

            // UITrig also captures ProjectBuyerCode, TestBuyerCode and Active
            if (insertDelete == "I")
            {
                log.ProjectBuyerCode = entity.ProjectBuyerCode;
                log.TestBuyerCode    = entity.TestBuyerCode;
                log.Active           = entity.Active;
            }

                    await _context.TestRequirementLogs.AddAsync(log);
                        await _context.SaveChangesAsync();
                    }

                    // ── TestReqBreakdown (fps.vtestreqbreakdown) ──────────────────────────────

                    public async Task<PagedData<TestReqBreakdownView>> GetPlannedTestsByWorkgroupAsync(PaginationParameters<string> query)
                    {
                        var baseQuery = _context.TestReqBreakdownViews.AsNoTracking();

                        baseQuery = ApplyPlannedTestsByWorkgroupFilter(baseQuery, query.Filter);
                        var sorted = ApplyPlannedTestsByWorkgroupSorting(baseQuery, query.SortBy, query.Descending);

                        return await ApplyPaging(sorted, query.Page, query.PageSize);
                    }

                    private static IQueryable<TestReqBreakdownView> ApplyPlannedTestsByWorkgroupSorting(
                        IQueryable<TestReqBreakdownView> source, string? sortBy, bool descending)
                    {
                        return sortBy?.ToLower() switch
                        {
                            "testcode"         => ApplyOrder(source, x => x.TestCode,         descending),
                            "shortdescription" => ApplyOrder(source, x => x.ShortDescription, descending),
                            "program"          => ApplyOrder(source, x => x.Program,          descending),
                            "project"          => ApplyOrder(source, x => x.Project,          descending),
                            "pc"               => ApplyOrder(source, x => x.Pc,               descending),
                            "workg"            => ApplyOrder(source, x => x.WorkG,            descending),
                            "wgprice"          => ApplyOrder(source, x => x.WgPrice,          descending),
                            "totalcost"        => ApplyOrder(source, x => x.TotalCost,        descending),
                            _                  => ApplyOrder(source, x => x.TestCode,         descending)
                        };
                    }

                    private static IQueryable<TestReqBreakdownView> ApplyPlannedTestsByWorkgroupFilter(
                        IQueryable<TestReqBreakdownView> query, string? filter)
                    {
                        if (string.IsNullOrWhiteSpace(filter))
                            return query;

                        var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filter);
                        if (filters is null)
                            return query;

                        query = ApplyBreakdownTextFilters(query, filters);
                        query = ApplyBreakdownCodeFilters(query, filters);

                        return query;
                    }

                    private static IQueryable<TestReqBreakdownView> ApplyBreakdownTextFilters(
                        IQueryable<TestReqBreakdownView> query, Dictionary<string, string> filters)
                    {
                        if (filters.TryGetValue("TestCode", out var testCode) && !string.IsNullOrWhiteSpace(testCode))
                            query = query.Where(x => EF.Functions.ILike(x.TestCode, $"%{testCode}%"));
                        if (filters.TryGetValue("ShortDescription", out var shortDesc) && !string.IsNullOrWhiteSpace(shortDesc))
                            query = query.Where(x => x.ShortDescription != null && EF.Functions.ILike(x.ShortDescription, $"%{shortDesc}%"));
                        if (filters.TryGetValue("Program", out var program) && !string.IsNullOrWhiteSpace(program))
                            query = query.Where(x => x.Program != null && EF.Functions.ILike(x.Program, $"%{program}%"));
                        return query;
                    }

                    private static IQueryable<TestReqBreakdownView> ApplyBreakdownCodeFilters(
                        IQueryable<TestReqBreakdownView> query, Dictionary<string, string> filters)
                    {
                        if (filters.TryGetValue("Project", out var project) && !string.IsNullOrWhiteSpace(project))
                            query = query.Where(x => EF.Functions.ILike(x.Project, $"%{project}%"));
                        if (filters.TryGetValue("PC", out var pc) && !string.IsNullOrWhiteSpace(pc))
                            query = query.Where(x => x.Pc != null && EF.Functions.ILike(x.Pc, $"%{pc}%"));
                        if (filters.TryGetValue("WorkG", out var workG) && !string.IsNullOrWhiteSpace(workG))
                            query = query.Where(x => x.WorkG != null && EF.Functions.ILike(x.WorkG, $"%{workG}%"));
                        return query;
                    }

                    private static IQueryable<T> ApplyOrder<T, TKey>(
                        IQueryable<T> source, System.Linq.Expressions.Expression<Func<T, TKey>> keySelector, bool descending)
                        => descending ? source.OrderByDescending(keySelector) : source.OrderBy(keySelector);
                }
            }
