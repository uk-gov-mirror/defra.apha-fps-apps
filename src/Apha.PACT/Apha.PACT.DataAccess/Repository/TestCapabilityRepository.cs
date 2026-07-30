using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PACT.DataAccess.Repository
{
    public class TestCapabilityRepository : BaseRepository, ITestCapabilityRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;

        public TestCapabilityRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        public async Task<PagedData<TestCapability>> GetPagedByWorkGroupAsync(
            PaginationParameters<string> query, string? workGroup)
        {
            var baseQuery = _context.TestCapabilities.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(workGroup))
                baseQuery = baseQuery.Where(t => t.WorkGroup == workGroup);

            baseQuery = ApplyTestCapabilityFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                baseQuery = query.Descending
                    ? baseQuery.OrderByDescending(e => EF.Property<object>(e, query.SortBy))
                    : baseQuery.OrderBy(e => EF.Property<object>(e, query.SortBy));
            else
                baseQuery = baseQuery.OrderBy(t => t.TestCode);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<PagedData<TestCapability>> GetPagedByTestCodeAsync(
            PaginationParameters<string> query, string? testCode)
        {
            var baseQuery = _context.TestCapabilities.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(testCode))
                baseQuery = baseQuery.Where(t => t.TestCode == testCode);

            baseQuery = ApplyTestCapabilityFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                baseQuery = query.Descending
                    ? baseQuery.OrderByDescending(e => EF.Property<object>(e, query.SortBy))
                    : baseQuery.OrderBy(e => EF.Property<object>(e, query.SortBy));
            else
                baseQuery = baseQuery.OrderBy(t => t.TestCode);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<PagedData<TestCapability>> GetPagedTestCapabilityByPortfolioAsync(
            PaginationParameters<string> query, string? portfolio)
        {
            var baseQuery = _context.TestCapabilities.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(portfolio))
                baseQuery = baseQuery.Where(t => t.PlanPortfolio == portfolio);

            baseQuery = ApplyTestCapabilityFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy) && query.SortBy != "ItemDescription")
                baseQuery = query.Descending
                    ? baseQuery.OrderByDescending(e => EF.Property<object>(e, query.SortBy))
                    : baseQuery.OrderBy(e => EF.Property<object>(e, query.SortBy));
            else
                baseQuery = baseQuery.OrderBy(t => t.TestCode);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<TestCapability?> GetByIdAsync(string testCode, string workGroup)
        {
            return await _context.TestCapabilities
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TestCode == testCode && t.WorkGroup == workGroup);
        }

        public async Task<TestCapability?> HasRelatedTestCapabilitiesValidRecordsAsync(string testCode)
        {
            return await _context.TestCapabilities
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TestCode.ToLower() == testCode.ToLower());
        }

        public async Task<TestCapability> AddAsync(TestCapability entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            await _context.TestCapabilities.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<TestCapability> UpdateAsync(TestCapability entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(string testCode, string workGroup)
        {
            var entity = await _context.TestCapabilities
                .FirstOrDefaultAsync(t =>
                    t.TestCode == testCode &&
                    t.WorkGroup == workGroup &&
                    t.FpsYear == _fpsRequestContext.FpsYear);

            if (entity is null) return false;

            _context.TestCapabilities.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string testCode, string portfolio)
        {
            return await _context.TestCapabilities
                .AsNoTracking()
                .AnyAsync(t =>
                    t.TestCode.ToLower() == testCode.Trim().ToLower() &&
                    t.PlanPortfolio.ToLower() == portfolio.Trim().ToLower());
        }

        public async Task<PagedData<WgTestCapabilitiesWithDescription>> GetPagedWgTestCapabilitiesWithDescriptionAsync(
            PaginationParameters<string> query, string workGroup)
        {
            var baseQuery = _context.TestCapabilities.AsNoTracking()
                .Where(testCapability => testCapability.WorkGroup == workGroup)
                .Join(_context.TestorProducts.AsNoTracking(),
                    testCapability => testCapability.TestCode,
                    testProduct => testProduct.ItemCode,
                    (testCapability, testProduct) => new WgTestCapabilitiesWithDescription
                    {
                        WorkGroup = testCapability.WorkGroup,
                        TestCode = testCapability.TestCode,
                        ItemDescription = testProduct.ItemDescription
                    })
                .Distinct()
                .AsQueryable();

            baseQuery = ApplyUserTestCapabilityFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                baseQuery = query.Descending
                    ? baseQuery.OrderByDescending(e => EF.Property<object>(e, query.SortBy))
                    : baseQuery.OrderBy(e => EF.Property<object>(e, query.SortBy));
            else
                baseQuery = baseQuery.OrderBy(t => t.TestCode);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        private static IQueryable<TestCapability> ApplyTestCapabilityFilter(
            IQueryable<TestCapability> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue("TestCode", out string? testCode) && !string.IsNullOrWhiteSpace(testCode))
                query = query.Where(t => EF.Functions.ILike(t.TestCode, $"%{testCode}%"));

            if (filters.TryGetValue("WorkGroup", out string? workGroup) && !string.IsNullOrWhiteSpace(workGroup))
                query = query.Where(t => EF.Functions.ILike(t.WorkGroup, $"%{workGroup}%"));

            if (filters.TryGetValue("PlanPortfolio", out string? portfolio) && !string.IsNullOrWhiteSpace(portfolio))
                query = query.Where(t => EF.Functions.ILike(t.PlanPortfolio, $"%{portfolio}%"));

            return query;
        }

        private static IQueryable<WgTestCapabilitiesWithDescription> ApplyUserTestCapabilityFilter(
            IQueryable<WgTestCapabilitiesWithDescription> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue(nameof(WgTestCapabilitiesWithDescription.WorkGroup), out var workGroup)
                && !string.IsNullOrWhiteSpace(workGroup))
                query = query.Where(t => EF.Functions.ILike(t.WorkGroup!, $"%{workGroup}%"));

            if (filters.TryGetValue(nameof(WgTestCapabilitiesWithDescription.TestCode), out var testCode)
                && !string.IsNullOrWhiteSpace(testCode))
                query = query.Where(t => EF.Functions.ILike(t.TestCode!, $"%{testCode}%"));

            if (filters.TryGetValue(nameof(WgTestCapabilitiesWithDescription.ItemDescription), out var itemDescription)
                && !string.IsNullOrWhiteSpace(itemDescription))
                query = query.Where(t => EF.Functions.ILike(t.ItemDescription!, $"%{itemDescription}%"));

            return query;
        }

        // ── Plan CrossTab ─────────────────────────────────────────────────────

        public Task BuildTestPlanSummaryAsync()
        {
           
            return Task.CompletedTask;
        }

        public async Task<CrossTabPagedResult> GetPagedTestPlanCrossTabAsync(
            PaginationParameters<string> query)
        {
            
            var testCapabilities = await _context.TestCapabilities
                .AsNoTracking().ToListAsync();

            var testRequirements = await _context.TestRequirements
                .AsNoTracking().ToListAsync();

            var testorProducts = await _context.TestorProducts
                .AsNoTracking().ToListAsync();

            var projectViews = await _context.ProjectViews
                .AsNoTracking().ToListAsync();

            var programs = await _context.Programs
                .AsNoTracking().ToListAsync();

            // fps.vworkgroup_general is used on the plan-side only (mirrors INNER JOIN in vw_test_plan_cost_pivot_src).
            // Note: WorkGroupViews maps to fps.vworkgroup which includes extra workgroups not in vworkgroup_general,
            // Use WorkGroupGeneralViews instead.
            var workGroupGeneralViews = await _context.WorkGroupGeneralViews
                .AsNoTracking().ToListAsync();

            // fps.vtestreqbreakdown already aggregates tbltestreqwg + tbltestrccost
            // + vworkgroup_general into (testcode, shortdescription, pc, totalcost).
            var reqBreakdownRows = await _context.TestReqBreakdownViews
                .AsNoTracking().ToListAsync();

            // ── Step 2: Resolve workgroup priority level per test code ────────
            // Mirrors fps.vw_test_workgroup_level:
                 var workgroupLevelByTestCode = testCapabilities
                .GroupBy(capability => capability.TestCode)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        HighestLevel   = group.Max(c => ResolveWorkgroupLevel(c.WorkGroup)),
                        WorkgroupCount = group.Count()
                    });

            // ── Step 3: Resolve default (representative) workgroup per test code
            // Mirrors fps.vw_test_default_workgroup:
            var defaultWorkgroupByTestCode = testCapabilities
                .Where(capability =>
                    workgroupLevelByTestCode.TryGetValue(capability.TestCode, out var levelInfo)
                    && ResolveWorkgroupLevel(capability.WorkGroup) == levelInfo.HighestLevel)
                .GroupBy(capability => capability.TestCode)
                .ToDictionary(
                    group => group.Key,
                    group =>
                    {
                        var levelInfo = workgroupLevelByTestCode[group.Key];
                        return group.Min(capability =>
                            levelInfo.WorkgroupCount == 1
                                ? capability.WorkGroup
                                : capability.WorkGroup.StartsWith("lt", StringComparison.OrdinalIgnoreCase)
                                    ? "LTM"
                                    : capability.WorkGroup.StartsWith("sv", StringComparison.OrdinalIgnoreCase)
                                        ? "SVXX"
                                        : capability.WorkGroup)!;
                    });

            // ── Step 4: Build plan-cost pivot source ──────────────────────────
            // Mirrors fps.vw_test_plan_cost_pivot_src:
            //   SUM(norequired * unitprice) grouped by (testcode, programno)
            // Only includes test codes whose default workgroup exists in vworkgroup.
            // INNER JOIN fps.vworkgroup_general — excludes testcodes whose default workgroup
            // does not exist in vworkgroup_general (matches vw_test_plan_cost_pivot_src exactly).
            var validWorkgroupNames = workGroupGeneralViews
                .Where(wg => wg.WorkGroup != null)
                .Select(wg => wg.WorkGroup)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var testorProductByTestCode = testorProducts
                .ToDictionary(tp => tp.ItemCode, StringComparer.OrdinalIgnoreCase);

            // Map buyer (project code) → list of programme numbers from vtlkpproject
            var programmesByBuyerProject = projectViews
                .Where(pv => pv.ParentProject != null && pv.Program != null)
                .GroupBy(pv => pv.ParentProject!)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(pv => pv.Program!).Distinct().ToList(),
                    StringComparer.OrdinalIgnoreCase);

            // All programme numbers in the current FPS year (defines pivot columns)
            var allProgrammeNumbers = programs
                .Select(p => p.ProgramNo)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p)
                .ToList();

            var allProgrammeNumberSet = allProgrammeNumbers
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // One aggregated plan-cost row per (owner, testcode, programno)
            // Mirrors ACCESS: GROUP BY TestOrProduct.Owner, tlkpTestReqmt.TestCode PIVOT tlkpProgram.ProgramNo
            var planCostRows = (
                from requirement in testRequirements
                where defaultWorkgroupByTestCode.ContainsKey(requirement.TestCode)
                   && validWorkgroupNames.Contains(defaultWorkgroupByTestCode[requirement.TestCode])
                   && testorProductByTestCode.ContainsKey(requirement.TestCode)
                   && programmesByBuyerProject.ContainsKey(requirement.Buyer)
                from programmeNo in programmesByBuyerProject[requirement.Buyer]
                where allProgrammeNumberSet.Contains(programmeNo)
                let owner    = testorProductByTestCode[requirement.TestCode].Owner
                let planCost = (requirement.UnitPrice ?? 0m) * (decimal)(requirement.NoRequired ?? 0)
                group planCost by new { Owner = owner, requirement.TestCode, ProgrammeNo = programmeNo } into costGroup
                select new
                {
                    TestCode    = costGroup.Key.TestCode,
                    ProgrammeNo = costGroup.Key.ProgrammeNo,
                    PlanCost    = costGroup.Sum()
                }).ToList();

            // Plan total per test code (sum across all programmes)
            var planTotalByTestCode = planCostRows
                .GroupBy(r => r.TestCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(r => r.PlanCost),
                    StringComparer.OrdinalIgnoreCase);

            // Plan cost lookup: testcode → (programmeNo → cost)
            var planCostByTestCodeAndProgramme = planCostRows
                .GroupBy(r => r.TestCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(
                        r => r.ProgrammeNo,
                        r => r.PlanCost,
                        StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);

            // ── Step 5: Build req-cost pivot from vtestreqbreakdown ───────────
            // Profit centre columns (defines req pivot columns; prefixed "pc_")
            var allProfitCentres = reqBreakdownRows
                .Where(r => r.Pc != null)
                .Select(r => r.Pc!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(pc => pc)
                .ToList();

            // Req total cost + short description per test code
            var reqSummaryByTestCode = reqBreakdownRows
                .GroupBy(r => r.TestCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        ShortDescription = g.First().ShortDescription,
                        TotalReqCost     = g.Sum(r => r.TotalCost ?? 0m)
                    },
                    StringComparer.OrdinalIgnoreCase);

            // Req cost lookup: testcode → (profitCentre → cost)
            var reqCostByTestCodeAndProfitCentre = reqBreakdownRows
                .Where(r => r.Pc != null)
                .GroupBy(r => r.TestCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(r => r.Pc!, StringComparer.OrdinalIgnoreCase)
                          .ToDictionary(
                              pcGroup => pcGroup.Key,
                              pcGroup => pcGroup.Sum(r => r.TotalCost ?? 0m),
                              StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);

            // ── Step 6: Define final column order ─────────────────────────────
            // Matches sp_build_vw_testplan_summary column order:
           
            var orderedColumns = new List<string> { "testcode", "shortdescription", "plan_total" }
                .Concat(allProgrammeNumbers)
                .Append("req_totalcost")
                .Concat(allProfitCentres.Select(pc => "pc_" + pc))
                .ToList();

       
            // only test codes present in BOTH the plan pivot and the req breakdown
            // are included — identical to sp_build_vw_testplan_summary's JOIN chain.
            var allTestCodes = planTotalByTestCode.Keys
                .Intersect(reqSummaryByTestCode.Keys, StringComparer.OrdinalIgnoreCase)
                .OrderBy(tc => tc)
                .ToList();

            var pivotedRows = allTestCodes.Select(testCode =>
            {
                var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

                row["testcode"]         = testCode;
                row["shortdescription"] = reqSummaryByTestCode.GetValueOrDefault(testCode)?.ShortDescription;
                row["plan_total"]       = planTotalByTestCode.GetValueOrDefault(testCode).ToString();

                foreach (var programmeNo in allProgrammeNumbers)
                    row[programmeNo] = planCostByTestCodeAndProgramme
                        .GetValueOrDefault(testCode)
                        ?.GetValueOrDefault(programmeNo)
                        .ToString() ?? null;

                row["req_totalcost"] = reqSummaryByTestCode
                    .GetValueOrDefault(testCode)?.TotalReqCost.ToString() ?? null;

                foreach (var profitCentre in allProfitCentres)
                    row["pc_" + profitCentre] = reqCostByTestCodeAndProfitCentre
                        .GetValueOrDefault(testCode)
                        ?.GetValueOrDefault(profitCentre)
                        .ToString() ?? null;

                return row;
            }).ToList();

            // ── Step 8: Apply text filters in-memory ──────────────────────────
            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var activeFilters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter);
                if (activeFilters != null)
                {
                    if (activeFilters.TryGetValue("testcode", out var testCodeFilter)
                        && !string.IsNullOrWhiteSpace(testCodeFilter))
                        pivotedRows = pivotedRows
                            .Where(r => r["testcode"]?
                                .Contains(testCodeFilter, StringComparison.OrdinalIgnoreCase) == true)
                            .ToList();

                    if (activeFilters.TryGetValue("shortdescription", out var shortDescFilter)
                        && !string.IsNullOrWhiteSpace(shortDescFilter))
                        pivotedRows = pivotedRows
                            .Where(r => r["shortdescription"]?
                                .Contains(shortDescFilter, StringComparison.OrdinalIgnoreCase) == true)
                            .ToList();
                }
            }

           
            var totalMatchingRows = pivotedRows.Count;

            // ── Apply sorting ─────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                var sortKey = query.SortBy.ToLowerInvariant();
                pivotedRows = query.Descending
                    ? pivotedRows.OrderByDescending(r => GetSortValue(r, sortKey)).ToList()
                    : pivotedRows.OrderBy(r => GetSortValue(r, sortKey)).ToList();
            }

            var pagedRows = pivotedRows
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            return new CrossTabPagedResult
            {
                Columns    = orderedColumns,
                Rows       = pagedRows,
                TotalCount = totalMatchingRows,
                Page       = query.Page,
                PageSize   = query.PageSize
            };
        }

   
        private static int ResolveWorkgroupLevel(string workGroup) =>
            workGroup.StartsWith("lt", StringComparison.OrdinalIgnoreCase) ? 3 :
            workGroup.StartsWith("sv", StringComparison.OrdinalIgnoreCase) ? 2 : 1;

        /// <summary>
        /// Returns a sort-comparable value for a pivot row column.
        /// Numeric columns sort as decimal; text columns sort as string.
        /// </summary>
        private static IComparable GetSortValue(Dictionary<string, string?> row, string key)
        {
            var raw = row.TryGetValue(key, out var val) ? val : null;
            if (raw != null && decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var number))
                return number;
            return raw ?? string.Empty;
        }
    }
}
