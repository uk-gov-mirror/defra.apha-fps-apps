using Apha.BatchJobs.Domain.Interfaces.MabArchive;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Repositories;

/// <summary>
/// Rebuilds FPS source totals before archive load.
/// </summary>
public sealed class FpsTotalsRepository : IFpsTotalsRepository
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<FpsTotalsRepository> _logger;
    private readonly MabArchiveSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="FpsTotalsRepository"/> class.
    /// </summary>
    /// <param name="context">Batch jobs database context.</param>
    /// <param name="logger">Logger instance.</param>
    public FpsTotalsRepository(
        BatchJobsDbContext context,
        ILogger<FpsTotalsRepository> logger,
        IOptions<MabArchiveSettings> settings)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? new MabArchiveSettings();
    }

    /// <summary>
    /// Rebuilds FPS source totals for the specified year.
    /// </summary>
    /// <param name="year">Target FPS year.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The number of rows inserted into fps.fpsyeartotals.</returns>
    public async Task<int> RebuildSourceTotalsAsync(int year, CancellationToken cancellationToken)
    {
        var targetYear = year;

        _logger.LogInformation("Rebuilding FPS source totals for year {Year}", targetYear);

        try
        {
            if (_settings.StrictYearIsolation)
            {
                await EnsureTotalsViewsAreYearScopedAsync(cancellationToken);
                _logger.LogInformation("Strict year isolation check passed for totals source views");
            }

            var testCostsSource = _context.RsQryTotalTestCosts
                .Select(x => new { x.JobCode, x.FpsYear, x.TotalTestCosts });

            _logger.LogInformation("Using test costs source view: fps.qrytotaltestcosts");

            var deleteRows = await _context.RsFpsYearTotals
                .Where(row => row.FpsYear == targetYear)
                .ExecuteDeleteAsync(cancellationToken);

            _logger.LogInformation("Deleted {RowCount} existing totals rows for year {Year}", deleteRows, targetYear);

            // Rebuild totals from source using C# logic equivalent to the baseline sp_createFPSTotals formulas and null handling.
            // Materialize first to avoid PostgreSQL money/numeric COALESCE translation edge cases.
            var rawRows = await (
                from t in _context.RsTlkpProject
                where t.FpsYear == targetYear
                join a in _context.RsQryTotalAdditionalCosts
                    on new { t.ParentProject, t.FpsYear } equals new { ParentProject = a.JobCode, a.FpsYear }
                    into additionalCostsJoin
                from a in additionalCostsJoin.DefaultIfEmpty()
                join an in _context.RsQryTotalAnimalCosts
                    on new { t.ParentProject, t.FpsYear } equals new { ParentProject = an.JobCode, an.FpsYear }
                    into animalCostsJoin
                from an in animalCostsJoin.DefaultIfEmpty()
                join s in _context.RsQryTotalStaffCosts
                    on new { t.ParentProject, t.FpsYear } equals new { ParentProject = s.JobCode, s.FpsYear }
                    into staffCostsJoin
                from s in staffCostsJoin.DefaultIfEmpty()
                join tst in testCostsSource
                    on new { t.ParentProject, t.FpsYear } equals new { ParentProject = tst.JobCode, tst.FpsYear }
                    into testCostsJoin
                from tst in testCostsJoin.DefaultIfEmpty()
                select new
                {
                    t.ParentProject,
                    t.Program,
                    AdditionalCosts = a.TotalAdditionalCosts,
                    AnimalCosts = an.TotalAnimalCosts,
                    StaffCosts = s.TotalStaffCosts,
                    TestCosts = tst.TotalTestCosts,
                    t.CustIncome,
                    t.TransferIncome,
                    t.BudgetCvl,
                    t.Profit,
                    t.Manager,
                    t.Customer,
                    t.ProjectStatus,
                    t.PvsIncome,
                    t.PlanCaseworkDebit,
                    s.TotalPayCosts,
                    t.FpsYear
                })
                .Distinct()
                .ToListAsync(cancellationToken);

            // Deduplicate by primary key before transforms to avoid EF tracker conflicts
            rawRows = rawRows
                .DistinctBy(r => new { r.ParentProject, r.FpsYear })
                .ToList();

            var totalsRows = rawRows
                .Select(r => new
                {
                    r.ParentProject,
                    r.Program,
                    TotalAdditionalCosts = r.AdditionalCosts ?? 0m,
                    TotalAnimalCosts = (double?)(r.AnimalCosts ?? 0m),
                    TotalStaffCosts = (double?)(r.StaffCosts ?? 0m),
                    TotalTestCosts = (double?)(r.TestCosts ?? 0m),
                    TotalCosts = (double?)(r.AdditionalCosts ?? 0m)
                        + (double?)(r.AnimalCosts ?? 0m)
                        + (double?)(r.StaffCosts ?? 0m)
                        + (double?)(r.TestCosts ?? 0m)
                        + (double?)(r.PlanCaseworkDebit ?? 0m),
                    r.CustIncome,
                    r.TransferIncome,
                    TotalIncome = r.CustIncome + r.TransferIncome,
                    r.BudgetCvl,
                    RequiredProfit = r.Profit,
                    r.Manager,
                    r.Customer,
                    r.ProjectStatus,
                    PvsIncome = r.PvsIncome ?? 0m,
                    PlanCaseworkDebit = r.PlanCaseworkDebit ?? 0m,
                    TotalPayCosts = (double?)(r.TotalPayCosts ?? 0m),
                    r.FpsYear
                })
                .Distinct()
                .Select(r => new RsFpsYearTotalsTable
                {
                    ParentProject = r.ParentProject,
                    Program = r.Program,
                    TotalAdditionalCosts = r.TotalAdditionalCosts,
                    TotalAnimalCosts = r.TotalAnimalCosts,
                    TotalStaffCosts = r.TotalStaffCosts,
                    TotalTestCosts = r.TotalTestCosts,
                    TotalCosts = r.TotalCosts,
                    CustIncome = r.CustIncome,
                    TransferIncome = r.TransferIncome,
                    TotalIncome = r.TotalIncome,
                    BudgetCvl = r.BudgetCvl,
                    RequiredProfit = r.RequiredProfit,
                    Manager = r.Manager,
                    Customer = r.Customer,
                    ProjectStatus = r.ProjectStatus,
                    PvsIncome = r.PvsIncome,
                    PlanCaseworkDebit = r.PlanCaseworkDebit,
                    TotalPayCosts = r.TotalPayCosts,
                    FpsYear = r.FpsYear
                })
                .ToList();

            if (totalsRows.Count == 0)
            {
                _logger.LogInformation("Inserted 0 rebuilt totals rows for year {Year}", targetYear);
                return 0;
            }

            // Clear tracked entities from previous iterations to avoid key conflicts
            _context.ChangeTracker.Clear();

            await _context.RsFpsYearTotals.AddRangeAsync(totalsRows, cancellationToken);
            var insertRows = await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Inserted {RowCount} rebuilt totals rows for year {Year}", insertRows, targetYear);

            return insertRows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rebuild FPS source totals for year {Year}", targetYear);
            throw;
        }
    }

    private async Task EnsureTotalsViewsAreYearScopedAsync(CancellationToken cancellationToken)
    {
        var missingViews = new List<string>();

        async Task ProbeViewYearColumnAsync(string viewName, IQueryable<int> yearProbe)
        {
            try
            {
                _ = await yearProbe
                    .Take(1)
                    .ToListAsync(cancellationToken);
            }
            catch (PostgresException ex) when (ex.SqlState is "42703" or "42P01")
            {
                // 42703: undefined_column, 42P01: undefined_table/relation
                // Either indicates the expected year-scoped view shape is unavailable.
                missingViews.Add(viewName);
            }
        }

        await ProbeViewYearColumnAsync("qrytotaladditionalcosts", _context.RsQryTotalAdditionalCosts.Select(x => x.FpsYear));
        await ProbeViewYearColumnAsync("qrytotalanimalcosts", _context.RsQryTotalAnimalCosts.Select(x => x.FpsYear));
        await ProbeViewYearColumnAsync("qrytotalstaffcosts", _context.RsQryTotalStaffCosts.Select(x => x.FpsYear));
        await ProbeViewYearColumnAsync("qrytotaltestcosts", _context.RsQryTotalTestCosts.Select(x => x.FpsYear));

        if (missingViews.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Strict year isolation is enabled, but these fps source views are missing fpsyear: {string.Join(", ", missingViews)}. " +
            "Update source views to expose fpsyear before running MABArchive totals rebuild.");
    }
}
