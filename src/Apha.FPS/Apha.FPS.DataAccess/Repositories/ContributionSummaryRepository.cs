/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: LINQ-first repository implementing IContributionSummaryRepository for
 *     the frmTimeSellerPC form migration.
 *   - No stored procedures were present in the source for this form; the data was sourced
 *     entirely from the PostgreSQL view fps.vqryfrmtimesellerpc (read) and a writable table
 *     fps.tblkpcontributionsummary (CRUD). All operations are pure EF Core LINQ.
 *   - GetByProfitCentreAsync: scoped by profitCentre and FpsYear (via global query filter);
 *     supports filter-dict-based text search and dynamic sort.
 *   - GetSummaryTotalsAsync: aggregate LINQ query computing SUM(TotalFec), SUM(AssuredFec),
 *     SUM(TotalCont), AVG(OhRate) from ContributionSummary rows; ContributionTarget sourced
 *     from ProfitCentre.ContTarget. TotalBudgetBids (from fps.vqrytbidsum) stubbed with 0 —
 *     see DEFERRED note below.
 *   - CRUD methods (CreateAsync, UpdateAsync, DeleteAsync) follow the standard pattern
 *     established in ProfitCentreGradeRepository and other FPS repositories.
 *   - ExistsAsync/GetAllProfitCentreCodesAsync match the interface contract (Phase 2).
 *
 * PRESERVED:
 *   - All method signatures from IContributionSummaryRepository.cs (Phase 2) preserved verbatim.
 *   - FpsYear stamped from _dbContext.FilterFpsYear on write operations (same pattern as all
 *     other year-partitioned repositories).
 *   - AsNoTracking for all read paths.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: TotalBudgetBids in GetSummaryTotalsAsync is currently returned as 0.
 *     The source value is fps.vqrytbidsum.sumofgenbid joined via profitcentre+fpsyear+user_id.
 *     Map fps.vqrytbidsum as a read-only entity (HasNoKey/ToView) and replace the stub with
 *     a real LINQ query once the entity is registered in FpsDbContext.
 *   - TRANSFORMENGINE TODO: Verify that fps.tblkpcontributionsummary table exists and that EF
 *     Core migrations have been applied before running the repository in production.
 */

using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ContributionSummaryRepository : BaseRepository, IContributionSummaryRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public ContributionSummaryRepository(FpsDbContext dbContext, IFpsRequestContext requestContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        // TRANSFORMENGINE: GetByProfitCentreAsync — primary grid query, scoped by profitCentre;
        //   global query filter on FpsYear is applied automatically via HasQueryFilter in FpsDbContext.
        //   Supports filter dict JSON and dynamic column sort per existing repository conventions.
        public async Task<PagedData<ContributionSummary>> GetByProfitCentreAsync(
            PaginationParameters<string> query,
            string profitCentre)
        {
            ArgumentNullException.ThrowIfNull(query);
            if (string.IsNullOrWhiteSpace(profitCentre))
                throw new ArgumentException("ProfitCentre is required.", nameof(profitCentre));

            var q = _dbContext.ContributionSummaries
                .AsNoTracking()
                .Where(x => x.ProfitCentre == profitCentre)
                .AsQueryable();

            q = ApplyFilter(q, query.Filter);
            q = (IQueryable<ContributionSummary>)ApplySorting(q, query.SortBy, query.Descending);

            var list = await q.ToListAsync();
            return ApplyPaging(list, query.Page, query.PageSize);
        }

        // TRANSFORMENGINE: GetByIdAsync — single-row lookup by integer PK; used by edit/delete modal.
        public async Task<ContributionSummary?> GetByIdAsync(int id)
        {
            return await _dbContext.ContributionSummaries
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // TRANSFORMENGINE: CreateAsync — INSERT into fps.tblkpcontributionsummary;
        //   FpsYear stamped from context at write time (mirrors CreateAsync in ProfitCentreGradeRepository).
        public async Task<ContributionSummary> CreateAsync(ContributionSummary entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            // TRANSFORMENGINE: Stamp FpsYear from current request context before insert.
            entity.FpsYear = _dbContext.FilterFpsYear;

            _dbContext.ContributionSummaries.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        // TRANSFORMENGINE: UpdateAsync — UPDATE by integer PK; id parameter is authoritative
        //   (entity.Id is ignored), consistent with IContributionSummaryRepository contract.
        public async Task<ContributionSummary> UpdateAsync(int id, ContributionSummary entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var existing = await _dbContext.ContributionSummaries
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing is null)
                throw new KeyNotFoundException($"ContributionSummary with Id={id} not found.");

            // TRANSFORMENGINE: Map all updatable fields; Id and FpsYear are not overwritten.
            existing.Wg                 = entity.Wg;
            existing.Grade              = entity.Grade;
            existing.AvailHrs           = entity.AvailHrs;
            existing.ChgRate            = entity.ChgRate;
            existing.TotalPlanHrs       = entity.TotalPlanHrs;
            existing.TotalFec           = entity.TotalFec;
            existing.TotalPctPlanned    = entity.TotalPctPlanned;
            existing.AssuredPlanHrs     = entity.AssuredPlanHrs;
            existing.AssuredFec         = entity.AssuredFec;
            existing.AssuredPctPlanned  = entity.AssuredPctPlanned;
            existing.OhRate             = entity.OhRate;
            existing.TotalCont          = entity.TotalCont;
            existing.ProfitCentre       = entity.ProfitCentre;

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        // TRANSFORMENGINE: DeleteAsync — DELETE by integer PK; returns false if row not found.
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _dbContext.ContributionSummaries
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity is null)
                return false;

            _dbContext.ContributionSummaries.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        // TRANSFORMENGINE: GetSummaryTotalsAsync — aggregate query for summary boxes;
        //   mirrors recomputeSummaryFromRows() in contribution_summary.js.
        //   When fpsYear is null, uses current context year (global query filter is active).
        //   When fpsYear is explicitly provided, bypasses global filter and applies it directly.
        public async Task<ContributionSummaryTotals?> GetSummaryTotalsAsync(
            string profitCentre,
            int? fpsYear = null)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                return null;

            // TRANSFORMENGINE: Resolve the effective year for explicit-year queries.
            var effectiveYear = fpsYear ?? _dbContext.FilterFpsYear;

            // TRANSFORMENGINE: Choose query set — use global-filtered set when no explicit year
            //   is given; bypass filter and apply explicit year when one is provided.
            IQueryable<ContributionSummary> baseQuery = fpsYear.HasValue
                ? _dbContext.ContributionSummaries
                    .IgnoreQueryFilters()
                    .Where(x => x.ProfitCentre == profitCentre && x.FpsYear == effectiveYear)
                : _dbContext.ContributionSummaries
                    .Where(x => x.ProfitCentre == profitCentre);

            // TRANSFORMENGINE: Single-pass aggregate projection from ContributionSummary rows.
            //   Maps to JS recomputeSummaryFromRows():
            //     totalTimeFeeFromPlanHrs  = rows.reduce(acc + row.totalFec)  → SUM(TotalFec)
            //     assuredTimeFeeFromPlanHrs = rows.reduce(acc + row.assuredFec) → SUM(AssuredFec)
            //     totalCont                = rows.reduce(acc + row.totalCont) → SUM(TotalCont)
            //     avgOhRate                = rows.reduce(acc + row.ohRate) / rows.length → AVG(OhRate)
            var agg = await baseQuery
                .AsNoTracking()
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    RowCount                  = g.Count(),
                    TotalTimeFeeFromPlanHrs   = g.Sum(x => x.TotalFec),
                    AssuredTimeFeeFromPlanHrs = g.Sum(x => x.AssuredFec),
                    RateEfficacyTotalCont     = g.Sum(x => x.TotalCont),
                    RateEfficacyOhRate        = g.Average(x => x.OhRate),
                })
                .FirstOrDefaultAsync();

            // TRANSFORMENGINE: Return null when no rows exist for the given scope.
            if (agg is null || agg.RowCount == 0)
                return null;

            // TRANSFORMENGINE: ContributionTarget sourced from ProfitCentre.ContTarget
            //   (fps.tblkpprofitcentre.conttarget); mirrors view column pc.conttarget.
            var pc = await _dbContext.ProfitCentres
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProfitCentreId == profitCentre);

            var contributionTarget = pc?.ContTarget ?? 0m;

            // TRANSFORMENGINE TODO STUB: TotalBudgetBids = fps.vqrytbidsum.sumofgenbid for this
            //   profitCentre/year/user. fps.vqrytbidsum is not yet mapped as an entity in FpsDbContext.
            //   Until the entity is added, this returns 0, which matches the JS test data
            //   (totalBudgetBids: 0 in contribution_summary.js pageData.summary).
            //   Replace stub with real LINQ once VqrytbidsumView entity is registered.
            var totalBudgetBids = 0m; // TRANSFORMENGINE TODO STUB - replace with real vqrytbidsum lookup

            // TRANSFORMENGINE: Derive remaining summary fields per JS recomputeSummaryFromRows logic.
            var totalToRecover               = contributionTarget + totalBudgetBids;
            var totalTimeSurplusShortfall    = agg.TotalTimeFeeFromPlanHrs - totalToRecover;
            var assuredTimeSurplusShortfall  = agg.AssuredTimeFeeFromPlanHrs - totalToRecover;

            return new ContributionSummaryTotals
            {
                TotalBudgetBids              = totalBudgetBids,
                ContributionTarget           = contributionTarget,
                TotalToRecover               = totalToRecover,
                TotalTimeFeeFromPlanHrs      = agg.TotalTimeFeeFromPlanHrs,
                TotalTimeSurplusShortfall    = totalTimeSurplusShortfall,
                AssuredTimeFeeFromPlanHrs    = agg.AssuredTimeFeeFromPlanHrs,
                AssuredTimeSurplusShortfall  = assuredTimeSurplusShortfall,
                RateEfficacyOhRate           = agg.RateEfficacyOhRate,
                RateEfficacyTotalCont        = agg.RateEfficacyTotalCont,
            };
        }

        // TRANSFORMENGINE: GetAllProfitCentreCodesAsync — distinct codes from ContributionSummaries
        //   for the current FPS year (global filter active); used to populate the resource-centre
        //   dropdown (cs-resource-centre select in contribution_summary.js).
        public async Task<List<string>> GetAllProfitCentreCodesAsync()
        {
            return await _dbContext.ContributionSummaries
                .AsNoTracking()
                .Select(x => x.ProfitCentre)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        // TRANSFORMENGINE: ExistsAsync — AnyAsync-style guard used by service layer before
        //   UpdateAsync/DeleteAsync to return 404 cleanly; IgnoreQueryFilters not used
        //   because the service always operates within the current year context.
        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbContext.ContributionSummaries
                .AsNoTracking()
                .AnyAsync(x => x.Id == id);
        }

        // ── Private helpers ────────────────────────────────────────────────────────

        // TRANSFORMENGINE: ApplyFilter — filter-dict JSON to LINQ ILike predicates;
        //   mirrors the ApplyFilter pattern from ProfitCentreGradeRepository.
        private static IQueryable<ContributionSummary> ApplyFilter(
            IQueryable<ContributionSummary> query,
            string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(filter)
                ?? new Dictionary<string, string>();

            if (filterDict.TryGetValue("Wg", out var wg) && wg != null)
                query = query.Where(x => EF.Functions.ILike(x.Wg, $"%{wg}%"));

            if (filterDict.TryGetValue("Grade", out var grade) && grade != null)
                query = query.Where(x => EF.Functions.ILike(x.Grade, $"%{grade}%"));

            if (filterDict.TryGetValue("ProfitCentre", out var profitCentre) && profitCentre != null)
                query = query.Where(x => EF.Functions.ILike(x.ProfitCentre, $"%{profitCentre}%"));

            return query;
        }

        // TRANSFORMENGINE: ApplySorting — dynamic sort on all entity columns;
        //   mirrors the ApplySorting pattern from ProfitCentreGradeRepository.
        private static IQueryable ApplySorting(
            IQueryable<ContributionSummary> query,
            string? sortBy,
            bool descending)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                "wg"                => Order(query, x => x.Wg, descending),
                "grade"             => Order(query, x => x.Grade, descending),
                "availhrs"          => Order(query, x => x.AvailHrs, descending),
                "chgrate"           => Order(query, x => x.ChgRate, descending),
                "totalplanhrs"      => Order(query, x => x.TotalPlanHrs, descending),
                "totalfec"          => Order(query, x => x.TotalFec, descending),
                "totalpctplanned"   => Order(query, x => x.TotalPctPlanned, descending),
                "assuredplanhrs"    => Order(query, x => x.AssuredPlanHrs, descending),
                "assuredfec"        => Order(query, x => x.AssuredFec, descending),
                "assuredpctplanned" => Order(query, x => x.AssuredPctPlanned, descending),
                "ohrate"            => Order(query, x => x.OhRate, descending),
                "totalcont"         => Order(query, x => x.TotalCont, descending),
                "profitcentre"      => Order(query, x => x.ProfitCentre, descending),
                _                   => query.OrderBy(x => x.Wg).ThenBy(x => x.Grade),
            };
        }

        private static IQueryable Order<TKey>(
            IQueryable<ContributionSummary> query,
            System.Linq.Expressions.Expression<Func<ContributionSummary, TKey>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
