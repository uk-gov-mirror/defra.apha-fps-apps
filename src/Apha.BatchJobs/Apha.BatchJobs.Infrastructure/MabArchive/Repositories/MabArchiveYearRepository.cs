using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Ports;
using Apha.BatchJobs.Domain.Interfaces.MabArchive;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Repositories;

/// <summary>
/// Manages yearly FPS archive data operations (delete, load, refresh).
/// Contract: delete/load/refresh operations are designed to run inside the orchestration transaction
/// provided by the caller so the full year cycle remains atomic.
/// </summary>
public sealed class MabArchiveYearRepository : IMabArchiveYearRepository
{
    private readonly BatchJobsDbContext _context;
    private readonly ILogger<MabArchiveYearRepository> _logger;
    private readonly IReadOnlyList<IMabArchiveLoader> _orderedLoaders;
    private readonly IMabArchiveLoader _projectAllLoader;
    private const int ExpectedLoaderCount = 24;

    /// <summary>
    /// Initializes a new instance of the <see cref="MabArchiveYearRepository"/> class.
    /// </summary>
    /// <param name="context">Batch jobs database context.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="loaders">Registered MABArchive loaders in metadata-defined sequence.</param>
    public MabArchiveYearRepository(
        BatchJobsDbContext context,
        ILogger<MabArchiveYearRepository> logger,
        IEnumerable<IMabArchiveLoader> loaders)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var loaderList = (loaders ?? throw new ArgumentNullException(nameof(loaders)))
            .OrderBy(l => l.Sequence)
            .ToList();

        if (loaderList.Count != ExpectedLoaderCount)
        {
            throw new InvalidOperationException($"MABArchive loader registration mismatch. Expected {ExpectedLoaderCount} loaders but found {loaderList.Count}.");
        }

        var duplicateSequences = loaderList
            .GroupBy(l => l.Sequence)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicateSequences.Length > 0)
        {
            throw new InvalidOperationException($"MABArchive loaders contain duplicate sequence values: {string.Join(",", duplicateSequences)}.");
        }

        var expectedSequences = Enumerable.Range(1, ExpectedLoaderCount);
        if (!expectedSequences.SequenceEqual(loaderList.Select(l => l.Sequence)))
        {
            throw new InvalidOperationException("MABArchive loader sequence must be contiguous from 1 to 24.");
        }

        _projectAllLoader = loaderList.Single(l => l.Sequence == ExpectedLoaderCount);
        _orderedLoaders = loaderList;
    }

    /// <summary>
    /// Deletes archive data for the specified year across archive tables in dependency order.
    /// Implements baseline SQL parity: full year-based wipe of archive dataset for the chosen year.
    /// Must be executed inside the caller's orchestration transaction.
    /// </summary>
    /// <param name="year">Target year to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total rows deleted.</returns>
    public async Task<int> DeleteYearDataAsync(int year, CancellationToken cancellationToken)
    {
        var targetYear = year;

        _logger.LogInformation("Deleting archive data for year {Year} across all archive tables in dependency order (baseline parity scope)", targetYear);

        try
        {
            var totalRowsAffected = 0;

            async Task DeleteWithYearAsync<T>(IQueryable<T> query, string tableName) where T : class
            {
                _logger.LogInformation("Deleting table {TableName} for year {Year}", tableName, targetYear);
                var deleteCount = await query.ExecuteDeleteAsync(cancellationToken);
                totalRowsAffected += deleteCount;
                _logger.LogInformation("Deleted {RowCount} rows from {TableName} for year {Year}", deleteCount, tableName, targetYear);
            }

            // Delete order must respect foreign key constraints.
            // Leaf tables first, then parent tables.
            await DeleteWithYearAsync(_context.MaDstMyTimeCostCalcs.Where(x => x.Year == targetYear), "mabarchive.my_timecostcalcs");
            await DeleteWithYearAsync(_context.MaDstMyMonthlyOutput.Where(x => x.Year == targetYear), "mabarchive.my_monthlyoutput");
            await DeleteWithYearAsync(_context.MaDstMyMonthlyTime.Where(x => x.Year == targetYear), "mabarchive.my_monthlytime");
            await DeleteWithYearAsync(_context.MaDstMyProjectMonthFinal.Where(x => x.Year == targetYear), "mabarchive.my_projectmonthfinal");
            await DeleteWithYearAsync(_context.MaDstMyProjInvoice.Where(x => x.Year == targetYear), "mabarchive.my_proj_invoice");
            await DeleteWithYearAsync(_context.MaDstMyProjSubContract.Where(x => x.Year == targetYear), "mabarchive.my_proj_subcontract");
            await DeleteWithYearAsync(_context.MaDstMyTblAdditionalCosts.Where(x => x.Year == targetYear), "mabarchive.my_tbladditionalcosts");
            await DeleteWithYearAsync(_context.MaDstMyTblAnimalReq.Where(x => x.Year == targetYear), "mabarchive.my_tblanimalreq");
            await DeleteWithYearAsync(_context.MaDstMyTblContract.Where(x => x.Year == targetYear), "mabarchive.my_tblcontract");
            await DeleteWithYearAsync(_context.MaDstMyTblStaffJob.Where(x => x.Year == targetYear), "mabarchive.my_tblstaffjob");
            await DeleteWithYearAsync(_context.MaDstMyTlkpTestReqmt.Where(x => x.Year == targetYear), "mabarchive.my_tlkptestreqmt");
            await DeleteWithYearAsync(_context.MaDstMyTestOrProduct.Where(x => x.Year == targetYear), "mabarchive.my_testorproduct");
            await DeleteWithYearAsync(_context.MaDstMyStaff.Where(x => x.Year == targetYear), "mabarchive.my_staff");
            await DeleteWithYearAsync(_context.MaDstMyWorkGroup.Where(x => x.Year == targetYear), "mabarchive.my_workgroup");
            await DeleteWithYearAsync(_context.MaDstMyTblProfitCentre.Where(x => x.Year == targetYear), "mabarchive.my_tblprofitcentre");
            await DeleteWithYearAsync(_context.MaDstMyProfitCentreGrade.Where(x => x.Year == targetYear), "mabarchive.my_profitcentregrade");
            await DeleteWithYearAsync(_context.MaDstMyWorkGroupGrade.Where(x => x.Year == targetYear), "mabarchive.my_workgroupgrade");
            await DeleteWithYearAsync(_context.MaDstMyTblAnimals.Where(x => x.Year == targetYear), "mabarchive.my_tblanimals");
            await DeleteWithYearAsync(_context.MaDstMyTlkpProgram.Where(x => x.Year == targetYear), "mabarchive.my_tlkpprogram");
            await DeleteWithYearAsync(_context.MaDstMyTlkpProject.Where(x => x.Year == targetYear), "mabarchive.my_tlkpproject");
            await DeleteWithYearAsync(_context.MaDstMyTlkpProjectAll.Where(x => x.Year == targetYear), "mabarchive.my_tlkpproject_all");
            await DeleteWithYearAsync(_context.MaDstMyFpsYearTotals.Where(x => x.Year == targetYear), "mabarchive.my_fpsyeartotals");
            await DeleteWithYearAsync(_context.MaDstTlkpYear.Where(x => x.Year == targetYear), "mabarchive.tlkpyear");

            // Guard: g_tlkpproject is a global/master reference table shared across years.
            // Never delete it as part of year-scoped cleanup.

            _logger.LogInformation("Deleted {TotalRowCount} total rows from archive tables for year {Year} (baseline parity scope)", totalRowsAffected, targetYear);
            return totalRowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete archive data for year {Year}", targetYear);
            throw;
        }
    }

    /// <summary>
    /// Loads archive data for the specified year from FPS source tables.
    /// Executes metadata-driven IMabArchiveLoader steps in baseline sequence order.
    /// All inserts are insert-only (no upsert); delete-then-insert is the idempotency mechanism per Assumption A3.
    /// Must be executed inside the caller's orchestration transaction.
    /// </summary>
    /// <param name="year">Target year to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total rows loaded.</returns>
    public async Task<int> LoadYearDataAsync(int year, CancellationToken cancellationToken)
    {
        var targetYear = year;

        _logger.LogInformation("Loading archive data for year {Year} - full sp_AddYearsFPSData fan-out ({LoaderCount} loaders, metadata sequence)", targetYear, _orderedLoaders.Count);

        var currentLoaderNumber = 0;
        var currentLoaderName = "NotStarted";

        try
        {
            var totalRowsAffected = 0;
            _context.ChangeTracker.Clear();

            foreach (var loader in _orderedLoaders)
            {
                _context.ChangeTracker.Clear();
                currentLoaderNumber = loader.Sequence;
                currentLoaderName = loader.Name;

                _logger.LogInformation("[{LoaderNumber}/{TotalLoaders}] Starting {LoaderName} for year {Year}", loader.Sequence, _orderedLoaders.Count, loader.Name, targetYear);

                   // Validation hook: record load performance and results
                   var sw = System.Diagnostics.Stopwatch.StartNew();
                   var rowCount = await loader.LoadAsync(targetYear, cancellationToken);
                   sw.Stop();

                   // Log performance and validate row count sanity
                   if (rowCount < 0)
                   {
                       _logger.LogWarning("[{LoaderNumber}] {LoaderName}: Unexpected negative row count {RowCount}. May indicate logic error.", loader.Sequence, loader.Name, rowCount);
                   }
                   if (sw.ElapsedMilliseconds > 30000)  // 30 seconds per loader is excessive
                   {
                       _logger.LogWarning("[{LoaderNumber}] {LoaderName}: Slow load detected ({DurationMs}ms for {RowCount} rows). May indicate N+1 or missing index.", loader.Sequence, loader.Name, sw.ElapsedMilliseconds, rowCount);
                   }
                totalRowsAffected += rowCount;
                _context.ChangeTracker.Clear();

                   _logger.LogInformation("[{LoaderNumber}/{TotalLoaders}] ✓ {LoaderName}: {RowCount} rows in {DurationMs}ms for year {Year}", loader.Sequence, _orderedLoaders.Count, loader.Name, rowCount, sw.ElapsedMilliseconds, targetYear);
            }

            _logger.LogInformation("LoadYearDataAsync complete: {TotalRowCount} total rows loaded for year {Year}", totalRowsAffected, targetYear);
            return totalRowsAffected;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load archive data for year {Year} while executing loader [{LoaderNumber}/{TotalLoaders}] {LoaderName}",
                targetYear,
                currentLoaderNumber,
                _orderedLoaders.Count,
                currentLoaderName);
            throw;
        }
    }

    /// <summary>
    /// Refreshes project cross-reference (my_tlkpproject_all) data for the specified year.
    /// Does not touch FPS totals, MABArchive transactional archive data, g_tlkpproject, or my_tlkpproject.
    /// Matches legacy sp_LoadFromFPS parity: the future/Planned-year branch only ever called
    /// sp_AddMY_tlkpProject_All. g_tlkpproject and my_tlkpproject are refreshed only as part of the
    /// Open year's full LoadYearDataAsync fan-out.
    /// Must be executed inside the caller's orchestration transaction.
    /// </summary>
    /// <param name="year">Target year to refresh.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Total rows affected in my_tlkpproject_all.</returns>
    public async Task<int> RefreshProjectsOnlyAsync(int year, CancellationToken cancellationToken)
    {
        var targetYear = year;

        _logger.LogInformation("Refreshing project cross-reference data (my_tlkpproject_all) for year {Year}", targetYear);

        try
        {
            var deletedProjectAllRows = await _context.MaDstMyTlkpProjectAll
                .Where(x => x.Year == targetYear)
                .ExecuteDeleteAsync(cancellationToken);
            _logger.LogInformation("Deleted {RowCount} rows in my_tlkpproject_all for year {Year} prior to refresh", deletedProjectAllRows, targetYear);

            var projectAllRows = await _projectAllLoader.LoadAsync(targetYear, cancellationToken);
            _logger.LogInformation("Refreshed {RowCount} rows in my_tlkpproject_all for year {Year}", projectAllRows, targetYear);

            return projectAllRows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh projects for year {Year}", targetYear);
            throw;
        }
    }
}
