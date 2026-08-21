using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class CreateFpsTotalsStep : RecreateSummariesExecutionStepBase
{
    private readonly ILogger<CreateFpsTotalsStep> _logger;

    public CreateFpsTotalsStep()
        : this(NullLogger<CreateFpsTotalsStep>.Instance)
    {
    }

    public CreateFpsTotalsStep(ILogger<CreateFpsTotalsStep> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override string StepName => "CreateFpsTotals"; // last hardened: 2026-06-29

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        _logger.LogInformation("CreateFpsTotals step starting");

        _logger.LogInformation("CreateFpsTotals validating year-scoped view contract");
        // ✓ RUNTIME MARKER: CreateFpsTotals using year-scoped joins on (ParentProject, FpsYear).
        // Validates totals view contract and prevents cross-year row fanout before query execution.
        await EnsureTotalsViewsAreYearScopedAsync(db, cancellationToken);
        _logger.LogInformation("CreateFpsTotals view contract validated");

        _logger.LogInformation("CreateFpsTotals loading joined totals rows for year {FpsYear}", context.FpsYear);
        var rawRows = await LoadRawTotalsRowsAsync(db, context.FpsYear, cancellationToken);
        _logger.LogInformation("CreateFpsTotals loaded {RowCount} raw joined rows", rawRows.Count);

        _logger.LogInformation("CreateFpsTotals mapping rows to target table shape");
        var rows = MapToTotalsRows(rawRows);
        _logger.LogInformation("CreateFpsTotals mapped {RowCount} unique rows for insert", rows.Count);

        db.ChangeTracker.Clear();
        _logger.LogInformation("CreateFpsTotals inserting rows into fps.fpsyeartotals");
        await db.RsFpsYearTotals.AddRangeAsync(rows, cancellationToken);
        var savedRows = await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("CreateFpsTotals save completed | RowsAffected={RowsAffected}", savedRows);
        return savedRows;
    }

    private static async Task<List<RawTotalsRow>> LoadRawTotalsRowsAsync(BatchJobsDbContext db, int fpsYear, CancellationToken cancellationToken)
    {
        // ✓ Year-scoped: query is restricted to the single fpsYear from the job parameter.
        // Legacy SQL ran against a per-year database; in PostgreSQL fpsyear is the shared-table discriminator.
        // Composite joins on (JobCode, FpsYear) additionally prevent cross-year row fanout.
        return await (
            from p in db.RsTlkpProject.AsNoTracking().Where(p => p.FpsYear == fpsYear)
            join add0 in db.RsQryTotalAdditionalCosts.AsNoTracking()
                on new { JobCode = p.ParentProject, p.FpsYear }
                equals new { add0.JobCode, add0.FpsYear } into add1
            from add in add1.DefaultIfEmpty()
            join ani0 in db.RsQryTotalAnimalCosts.AsNoTracking()
                on new { JobCode = p.ParentProject, p.FpsYear }
                equals new { ani0.JobCode, ani0.FpsYear } into ani1
            from ani in ani1.DefaultIfEmpty()
            join stf0 in db.RsQryTotalStaffCosts.AsNoTracking()
                on new { JobCode = p.ParentProject, p.FpsYear }
                equals new { stf0.JobCode, stf0.FpsYear } into stf1
            from stf in stf1.DefaultIfEmpty()
            join tst0 in db.RsQryTotalTestCosts.AsNoTracking()
                on new { JobCode = p.ParentProject, p.FpsYear }
                equals new { tst0.JobCode, tst0.FpsYear } into tst1
            from tst in tst1.DefaultIfEmpty()
            select new RawTotalsRow
            {
                ParentProject = p.ParentProject,
                Program = p.Program,
                TotalAdditionalCosts = add.TotalAdditionalCosts,
                TotalAnimalCosts = ani.TotalAnimalCosts,
                TotalStaffCosts = stf.TotalStaffCosts,
                TotalTestCosts = tst.TotalTestCosts,
                PlanCaseworkDebit = p.PlanCaseworkDebit,
                CustIncome = p.CustIncome,
                TransferIncome = p.TransferIncome,
                BudgetCvl = p.BudgetCvl,
                RequiredProfit = p.Profit,
                Manager = p.Manager,
                Customer = p.Customer,
                ProjectStatus = p.ProjectStatus,
                PvsIncome = p.PvsIncome,
                TotalPayCosts = stf.TotalPayCosts,
                FpsYear = p.FpsYear
            })
            .ToListAsync(cancellationToken);
    }

    private static List<RsFpsYearTotalsTable> MapToTotalsRows(IEnumerable<RawTotalsRow> rawRows)
    {
        return rawRows
            .Select(r => new RsFpsYearTotalsTable
            {
                ParentProject = r.ParentProject,
                Program = r.Program ?? string.Empty,
                TotalAdditionalCosts = r.TotalAdditionalCosts ?? 0m,
                TotalAnimalCosts = (double?)(r.TotalAnimalCosts ?? 0m),
                TotalStaffCosts = (double?)(r.TotalStaffCosts ?? 0m),
                TotalTestCosts = (double?)(r.TotalTestCosts ?? 0m),
                TotalCosts = (double)(r.TotalAdditionalCosts ?? 0m)
                    + (double)(r.TotalAnimalCosts ?? 0m)
                    + (double)(r.TotalStaffCosts ?? 0m)
                    + (double)(r.TotalTestCosts ?? 0m)
                    + (double)(r.PlanCaseworkDebit ?? 0m),
                CustIncome = r.CustIncome ?? 0m,
                TransferIncome = r.TransferIncome ?? 0m,
                TotalIncome = (r.CustIncome ?? 0m) + (r.TransferIncome ?? 0m),
                BudgetCvl = r.BudgetCvl ?? 0m,
                RequiredProfit = r.RequiredProfit ?? 0m,
                Manager = r.Manager ?? string.Empty,
                Customer = r.Customer ?? string.Empty,
                ProjectStatus = r.ProjectStatus ?? string.Empty,
                PvsIncome = r.PvsIncome ?? 0m,
                PlanCaseworkDebit = r.PlanCaseworkDebit ?? 0m,
                TotalPayCosts = (double?)(r.TotalPayCosts ?? 0m),
                FpsYear = r.FpsYear
            })
            // Enforce uniqueness (parentproject + fpsyear)
            .GroupBy(r => new { r.ParentProject, r.FpsYear })
            .Select(g => g.First())
            .ToList();
    }

    private async Task EnsureTotalsViewsAreYearScopedAsync(BatchJobsDbContext db, CancellationToken cancellationToken)
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
                missingViews.Add(viewName);
            }
        }

        _logger.LogInformation("CreateFpsTotals probing view contract: qrytotaladditionalcosts");
        await ProbeViewYearColumnAsync("qrytotaladditionalcosts", db.RsQryTotalAdditionalCosts.Select(x => x.FpsYear));
        _logger.LogInformation("CreateFpsTotals probing view contract: qrytotalanimalcosts");
        await ProbeViewYearColumnAsync("qrytotalanimalcosts", db.RsQryTotalAnimalCosts.Select(x => x.FpsYear));
        _logger.LogInformation("CreateFpsTotals probing view contract: qrytotalstaffcosts");
        await ProbeViewYearColumnAsync("qrytotalstaffcosts", db.RsQryTotalStaffCosts.Select(x => x.FpsYear));
        _logger.LogInformation("CreateFpsTotals probing view contract: qrytotaltestcosts");
        await ProbeViewYearColumnAsync("qrytotaltestcosts", db.RsQryTotalTestCosts.Select(x => x.FpsYear));

        if (missingViews.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"CreateFpsTotals requires year-scoped fps views with fpsyear. Missing view contract: {string.Join(", ", missingViews)}. " +
            "Raise a DB CR to add fpsyear to these view outputs before runtime rollout.");
    }

    private sealed class RawTotalsRow
    {
        public string ParentProject { get; init; } = null!;
        public string? Program { get; init; }
        public decimal? TotalAdditionalCosts { get; init; }
        public decimal? TotalAnimalCosts { get; init; }
        public decimal? TotalStaffCosts { get; init; }
        public decimal? TotalTestCosts { get; init; }
        public decimal? PlanCaseworkDebit { get; init; }
        public decimal? CustIncome { get; init; }
        public decimal? TransferIncome { get; init; }
        public decimal? BudgetCvl { get; init; }
        public decimal? RequiredProfit { get; init; }
        public string? Manager { get; init; }
        public string? Customer { get; init; }
        public string? ProjectStatus { get; init; }
        public decimal? PvsIncome { get; init; }
        public decimal? TotalPayCosts { get; init; }
        public int FpsYear { get; init; }
    }
}
