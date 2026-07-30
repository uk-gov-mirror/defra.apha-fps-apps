using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apha.PIMS.DataAccess.Repository
{
    public class YearlyFinancialDataRepository : IYearlyFinancialDataRepository
    {
        private readonly PimsDbContext _context;

        public YearlyFinancialDataRepository(PimsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedData<YearlyFinancialData>> GetAllAsync(
            string project,
            PaginationParameters<string> paging)
        {
            IQueryable<YearlyFinancialData> query = _context.YearlyFinancialData
                .AsNoTracking()
                .Where(e => e.Project == project);

            query = ApplySearch(query, paging.Search);
            query = ApplySorting(query, paging.SortBy, paging.Descending);

            int totalRecords = await query.CountAsync();

            List<YearlyFinancialData> data = paging.Page == -1
                ? await query.ToListAsync()
                : await query
                      .Skip((paging.Page - 1) * paging.PageSize)
                      .Take(paging.PageSize)
                      .ToListAsync();

            var pagination = new PaginationData
            {
                PageNumber   = paging.Page,
                PageSize     = paging.PageSize,
                TotalRecords = totalRecords,
                TotalPages   = paging.Page == -1
                    ? 1
                    : (int)Math.Ceiling((double)totalRecords / paging.PageSize)
            };

            return new PagedData<YearlyFinancialData>(data, pagination);
        }

        public async Task<YearlyFinancialData?> GetByKeyAsync(short year, string project)
        {
            return await _context.YearlyFinancialData
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Year == year && e.Project == project);
        }

       
        public async Task<bool> ExistsAsync(short year, string project)
        {
            return await _context.YearlyFinancialData
                .AsNoTracking()
                .AnyAsync(e => e.Year == year && e.Project == project);
        }

       
        public async Task<YearlyFinancialData> CreateAsync(YearlyFinancialData entity)
        {
            await _context.YearlyFinancialData.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<YearlyFinancialData> UpdateAsync(YearlyFinancialData entity)
        {
            _context.YearlyFinancialData.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        
        public async Task<bool> DeleteAsync(short year, string project)
        {
            int affected = await _context.YearlyFinancialData
                .Where(e => e.Year == year && e.Project == project)
                .ExecuteDeleteAsync();

            return affected > 0;
        }

      
        public async Task<IReadOnlyList<PactProjectYearCosts>> GetPactCostsAsync(
            string project,
            short year)
        {
            
            ProjectRadTrackData? rtd = await _context.ProjectRadTrackData
                .AsNoTracking()
                .Where(r => r.Parentproject == project)
                .FirstOrDefaultAsync();

           
            List<ProjectMonthFinal> allMonths = await _context.ProjectMonthFinals
                .AsNoTracking()
                .Where(pmf => pmf.Project == project)
                .ToListAsync();

           
            static short DeriveFiscalYear(ProjectMonthFinal pmf, ProjectRadTrackData? rtd)
            {
                if (rtd is { Useprojectyear: -1 } && rtd.Startdate.HasValue)
                {
                    int shift = (int)pmf.Monthno + 3 - rtd.Startdate.Value.Month;
                    return (short)new DateTime(pmf.Year, 1, 1).AddMonths(shift).Year;
                }
                return pmf.Year;
            }

            List<ProjectMonthFinal> monthsForYear = allMonths
                .Where(pmf => DeriveFiscalYear(pmf, rtd) == year)
                .ToList();

            if (monthsForYear.Count == 0)
                return Array.Empty<PactProjectYearCosts>();

            IEnumerable<short>  calendarYears = monthsForYear.Select(m => m.Year).Distinct();
            IEnumerable<double> monthNos      = monthsForYear.Select(m => m.Monthno).Distinct();

            List<TimeCostCalcs> tccRows = await _context.TimeCostCalcs
                .AsNoTracking()
                .Where(t => t.Project == project
                         && calendarYears.Contains(t.Year)
                         && monthNos.Contains(t.Month))
                .ToListAsync();

           
            var tccLookup = tccRows
                .GroupBy(t => (t.Year, t.Month))
                .ToDictionary(
                    g => g.Key,
                    g => (
                        Pay:      g.Sum(t => t.Pay      ?? 0m),
                        NonPayOH: g.Sum(t => (t.Nonpay  ?? 0m) + (t.Overhead ?? 0m))
                    ));

            // ── 4. Aggregate per monthno into PactProjectYearCosts rows ──────────────
            List<PactProjectYearCosts> rows = monthsForYear
                .GroupBy(pmf => pmf.Monthno)
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    short calYear = g.First().Year;
                    tccLookup.TryGetValue((calYear, g.Key), out var tcc);

                    return new PactProjectYearCosts
                    {
                        Project      = project,
                        Year         = (double)year,
                        MonthNo      = g.Key,
                        SubContracts = g.Sum(pmf => pmf.Subcontracts  ?? 0m),
                        Animals      = g.Sum(pmf => pmf.Animals       ?? 0m),
                        Tests        = g.Sum(pmf => pmf.Transfercosts ?? 0m),
                        Pay          = tcc.Pay,
                        NonPayOH     = tcc.NonPayOH,
                        Hours        = g.Sum(pmf => pmf.Totalhours    ?? 0d),
                        TotalCosts   = g.Sum(pmf => pmf.Totalcost     ?? 0m),
                        TimeCost     = g.Sum(pmf => pmf.Timecosts     ?? 0m),
                    };
                })
                .ToList();

           
            Projects? proj = await _context.MyTlkpProjects
                .AsNoTracking()
                .Where(p => p.Parentproject == project && p.Year == year)
                .FirstOrDefaultAsync();

            decimal? custIncome = proj?.Custincome;
            decimal? budgetCvl  = proj?.BudgetCvl;

            foreach (PactProjectYearCosts row in rows)
            {
                row.CustIncome = custIncome;
                row.BudgetCvl  = budgetCvl;
            }

            return rows.AsReadOnly();
        }

        public async Task<string?> GetSettingValueByIdAsync(string id)
        {
            return await _context.DatabaseSettings
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => s.Setting)
                .FirstOrDefaultAsync();
        }

        // ─── Private helpers ────────────────────────────────────────────────────────

      
        private static IQueryable<YearlyFinancialData> ApplySearch(
            IQueryable<YearlyFinancialData> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;

            string s = search.ToLower();

            // Search year as string equivalent, project code, costedby username
            return query.Where(e =>
                e.Project.ToLower().Contains(s) ||
                (e.CostedBy != null && e.CostedBy.ToLower().Contains(s)) ||
                (e.AdjustmentComment != null && e.AdjustmentComment.ToLower().Contains(s)));
        }

      
        private static IQueryable<YearlyFinancialData> ApplySorting(
            IQueryable<YearlyFinancialData> query,
            string? sortBy,
            bool descending)
        {
            return (sortBy?.ToLower()) switch
            {
                "year"                => ApplyOrder(query, e => e.Year,               descending),
                "project"             => ApplyOrder(query, e => e.Project,            descending),
                "bfbudget"            => ApplyOrder(query, e => e.BfBudget,           descending),
                "pybudget"            => ApplyOrder(query, e => e.PyBudget,           descending),
                "seedcorn"            => ApplyOrder(query, e => e.Seedcorn,           descending),
                "manhours"            => ApplyOrder(query, e => e.ManHours,           descending),
                "mandays"             => ApplyOrder(query, e => e.ManDays,            descending),
                "manyears"            => ApplyOrder(query, e => e.ManYears,           descending),
                "paycosts"            => ApplyOrder(query, e => e.PayCosts,           descending),
                "nonpayohcosts"       => ApplyOrder(query, e => e.NonPayOhCosts,      descending),
                "testcosts"           => ApplyOrder(query, e => e.TestCosts,          descending),
                "animalcosts"         => ApplyOrder(query, e => e.AnimalCosts,        descending),
                "nonanimalcosts"      => ApplyOrder(query, e => e.NonAnimalCosts,     descending),
                "adjustment"          => ApplyOrder(query, e => e.Adjustment,         descending),
                "actualexpenditure"   => ApplyOrder(query, e => e.ActualExpenditure,  descending),
                "actualmanyears"      => ApplyOrder(query, e => e.ActualManYears,     descending),
                "vlabudget"           => ApplyOrder(query, e => e.VlaBudget,          descending),
                "locked"              => ApplyOrder(query, e => e.Locked,             descending),
                "datecosted"          => ApplyOrder(query, e => e.DateCosted,         descending),
                "costedby"            => ApplyOrder(query, e => e.CostedBy,           descending),
                _                     => query.OrderBy(e => e.Year).ThenBy(e => e.Project)
            };
        }

        private static IQueryable<T> ApplyOrder<T, TKey>(
            IQueryable<T> query,
            Expression<Func<T, TKey>> keySelector,
            bool descending)
            => descending
               ? query.OrderByDescending(keySelector)
               : query.OrderBy(keySelector);
    }
}
