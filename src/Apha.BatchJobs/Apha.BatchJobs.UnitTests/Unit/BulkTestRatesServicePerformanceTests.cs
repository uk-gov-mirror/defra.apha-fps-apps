using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.BulkRates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using NSubstitute;
using System.Diagnostics;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Full-volume (~700 FEC + ~3,200 AGRUP rows, the legacy annual-upload baseline
/// per baseline spec Â§2.1) diagnostics for the Bulk Rates worker pipeline.
///
/// Assertions here are intentionally soft (timing/memory are recorded via
/// <see cref="ITestOutputHelper"/>, not hard-gated) â€” shared/slow CI hardware makes strict
/// wall-clock thresholds flaky. The one bound that IS hard-asserted is the total worker
/// transaction duration: the SELECT ... FOR UPDATE lock window held during revalidation
/// is a real correctness/availability risk if unbounded, not merely a nicety.
///
/// No-N+1 is verified by code inspection, not runtime instrumentation: every bulk lookup helper in
/// <see cref="BulkTestRatesService"/> (live FEC/AGRUP row lock, project lookup, capability
/// lookup, snapshot read) issues exactly one `= ANY(@array)`-parameterized query regardless of
/// row count â€” confirmed by reading every one of them, not inferred. The genuinely interesting
/// finding this test surfaces is different: <c>WriteHistoryInsideTransactionAsync</c> writes one
/// `fps.rate_change_history` row per field-change via a per-row loop (not a bulk insert) â€” a
/// real O(n) round-trip count inside the FOR UPDATE-locked transaction, worth measuring rather
/// than assuming away.
/// </summary>
[Trait("Category", "Integration")]
public sealed class BulkTestRatesServicePerformanceTests : IAsyncLifetime
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=batch_jobs_foundation_db_cloud;Username=postgres;Password=admin123;SSL Mode=Disable";
    private readonly string _connectionString;
    private readonly ITestOutputHelper _output;
    private string? _skipReason;

    private const int FpsYear = 2088;
    private const int FecRowCount = 700;
    private const int AgrupRowCount = 3200;

    public BulkTestRatesServicePerformanceTests(ITestOutputHelper output)
    {
        _output = output;
        _connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString")
            ?? DefaultConnectionString;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await using var context = CreateDbContext();
            if (!await context.Database.CanConnectAsync())
            {
                _skipReason = "Integration DB unavailable.";
                return;
            }

            var count = await context.Database
                .SqlQuery<int>($@"
                    SELECT COUNT(*)::int AS ""Value""
                    FROM fps.job_master m
                    JOIN fps.job_status s ON s.jobid = m.jobid
                    WHERE m.jobname = {BatchJobNames.BulkTestRatesUpdate} AND s.status = 'Running'")
                .SingleAsync();

            if (count == 0)
                _skipReason = $"job_master/job_status seed for '{BatchJobNames.BulkTestRatesUpdate}' + 'Running' is not provisioned.";
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private bool CanRunIntegrationTests() => string.IsNullOrWhiteSpace(_skipReason);

    private BatchJobsDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<BatchJobsDbContext>().UseNpgsql(_connectionString).Options);

    private sealed class TestDbContextFactory(string connectionString) : IDbContextFactory<BatchJobsDbContext>
    {
        public BatchJobsDbContext CreateDbContext() =>
            new(new DbContextOptionsBuilder<BatchJobsDbContext>().UseNpgsql(connectionString).Options);
    }

    private BulkTestRatesService CreateService() => new(
        new BulkRatesRepository(new TestDbContextFactory(_connectionString), NullLogger<BulkRatesRepository>.Instance),
        Substitute.For<IJobExecutionRepository>(),
        NullLogger<BulkTestRatesService>.Instance);

    // â”€â”€ Real Postgres: full worker pipeline at volume â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [SkippableFact]
    public async Task ExecuteAsync_FullVolume_CompletesWithinBoundedTransactionWindow()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var seedSw = Stopwatch.StartNew();
        await SeedFullVolumeAsync(conn);
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId);
        seedSw.Stop();
        _output.WriteLine($"[perf] Seed {FecRowCount} FEC + {AgrupRowCount} AGRUP (live + staged): {seedSw.ElapsedMilliseconds} ms.");

        try
        {
            var runSw = Stopwatch.StartNew();
            var memoryBefore = GC.GetTotalMemory(forceFullCollection: false);

            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, FpsYear));

            runSw.Stop();
            var memoryAfter = GC.GetTotalMemory(forceFullCollection: false);

            _output.WriteLine($"[perf] ExecuteAsync (revalidate under FOR UPDATE + apply {FecRowCount + AgrupRowCount} rows " +
                               $"+ write {(FecRowCount * 2) + AgrupRowCount} history rows + commit): {runSw.ElapsedMilliseconds} ms.");
            _output.WriteLine($"[perf] Approx managed memory delta: {(memoryAfter - memoryBefore) / 1024.0 / 1024.0:F1} MB.");

            // Hard-gated: this is the one number that's a genuine risk regardless of hardware â€”
            // the FOR UPDATE lock is held for (approximately) this whole duration, and an
            // unbounded hold at annual-upload volume would block concurrent work elsewhere in
            // the schema. 30s is generous for local/shared hardware while still
            // catching a real quadratic-ish regression (e.g. an accidental per-row lookup query).
            Assert.True(runSw.Elapsed < TimeSpan.FromSeconds(30),
                $"Worker transaction took {runSw.Elapsed} for {FecRowCount + AgrupRowCount} rows â€” " +
                "investigate for an accidental per-row query before raising this bound.");

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                var historyCount = (int)(await cmd.ExecuteScalarAsync())!;
                Assert.Equal((FecRowCount * 2) + AgrupRowCount, historyCount);
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.testorproduct WHERE fpsyear = @year AND unitpricevla::numeric = 15.00;";
                cmd.Parameters.AddWithValue("year", FpsYear);
                Assert.Equal(FecRowCount, (int)(await cmd.ExecuteScalarAsync())!);
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.tlkptestreqmt WHERE fpsyear = @year AND unitprice::numeric = 8.00;";
                cmd.Parameters.AddWithValue("year", FpsYear);
                Assert.Equal(AgrupRowCount, (int)(await cmd.ExecuteScalarAsync())!);
            }
        }
        finally
        {
            await CleanupFullVolumeAsync(jobQueueId);
        }
    }

    // â”€â”€ Volume seeding (literal SQL, not parameterized â€” see note below) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // Every value here is generated by this loop, never external/user input, so inlining into
    // the VALUES list (rather than one NpgsqlParameter per column per row, which would push
    // the AGRUP staging insert alone to 35,000+ parameters) is safe and keeps setup to a
    // handful of round trips instead of thousands.

    private static async Task SeedFullVolumeAsync(NpgsqlConnection conn)
    {
        var liveFec = new StringBuilder();
        var stagedFecRows = new List<string>();
        for (var i = 0; i < FecRowCount; i++)
        {
            var code = $"PERF-FEC-{i:D4}";
            liveFec.Append(liveFec.Length == 0 ? "" : ",")
                .Append($"('{code}','Perf item {i}',10.00,'PT','Perf',10.00,{FpsYear})");
        }
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = $@"
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES {liveFec};";
            await cmd.ExecuteNonQueryAsync();
        }

        var liveAgrup = new StringBuilder();
        for (var i = 0; i < AgrupRowCount; i++)
        {
            var fecCode = $"PERF-FEC-{i % FecRowCount:D4}";
            var buyer = $"PERF-BUYER-{i:D4}";
            liveAgrup.Append(liveAgrup.Length == 0 ? "" : ",")
                .Append($"('{fecCode}','{buyer}',5.00,NULL,NULL,{FpsYear})");
        }
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = $@"
                INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, projectbuyercode, testbuyercode, fpsyear)
                VALUES {liveAgrup};";
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task SeedStagingAsync(NpgsqlConnection conn, Guid jobQueueId)
    {
        var stagedFec = new StringBuilder();
        for (var i = 0; i < FecRowCount; i++)
        {
            var code = $"PERF-FEC-{i:D4}";
            stagedFec.Append(stagedFec.Length == 0 ? "" : ",")
                .Append($"('{jobQueueId}','{code}',10.00,10.00,15.00,'Update',15.00,10.00,1)");
        }
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = $@"
                INSERT INTO fps.tblstagingtestorproduct
                    (jobqueueid, testcode, unitpricevla, defraunitprice, fecnewrate,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES {stagedFec};";
            await cmd.ExecuteNonQueryAsync();
        }

        var stagedAgrup = new StringBuilder();
        for (var i = 0; i < AgrupRowCount; i++)
        {
            var fecCode = $"PERF-FEC-{i % FecRowCount:D4}";
            var buyer = $"PERF-BUYER-{i:D4}";
            stagedAgrup.Append(stagedAgrup.Length == 0 ? "" : ",")
                .Append($"('{jobQueueId}','{fecCode}','{buyer}',NULL,8.00,NULL,NULL,'Update',8.00,5.00,1)");
        }
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = $@"
                INSERT INTO fps.tblstagingtlkptestreqmt
                    (jobqueueid, testcode, buyer, agrup, agrupnew, projectbuyercode, testbuyercode,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES {stagedAgrup};";
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task<int> ResolveStatusIdAsync(NpgsqlConnection conn, string status)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT s.statusid FROM fps.job_status s
            JOIN fps.job_master m ON m.jobid = s.jobid
            WHERE m.jobname = @jobname AND s.status = @status;";
        cmd.Parameters.AddWithValue("jobname", BatchJobNames.BulkTestRatesUpdate);
        cmd.Parameters.AddWithValue("status", status);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    private static async Task<int> ResolveJobIdAsync(NpgsqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT jobid FROM fps.job_master WHERE jobname = @jobname;";
        cmd.Parameters.AddWithValue("jobname", BatchJobNames.BulkTestRatesUpdate);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task InsertJobQueueAsync(NpgsqlConnection conn, Guid jobQueueId, Guid jobExecutionId)
    {
        var jobId = await ResolveJobIdAsync(conn);
        var statusId = await ResolveStatusIdAsync(conn, "Running");

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.job_queue
                    (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, fpsyear,
                     approved_by, approved_at_utc, upload_version, active_download_version)
                VALUES
                    (@jobqueueid, @jobexecutionid, @jobid, @statusid, 'perf-test-requester', NOW(), @fpsyear,
                     'perf-test-approver', NOW(), 1, NULL);";
            cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
            cmd.Parameters.AddWithValue("jobexecutionid", jobExecutionId);
            cmd.Parameters.AddWithValue("jobid", jobId);
            cmd.Parameters.AddWithValue("statusid", statusId);
            cmd.Parameters.AddWithValue("fpsyear", FpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        // Staging must be inserted after job_queue exists (FK) â€” done here rather than in
        // SeedFullVolumeAsync so the live-data seed and the FK-dependent staging seed stay
        // in their natural dependency order without the caller needing to know why.
        await SeedStagingAsync(conn, jobQueueId);
    }

    private async Task CleanupFullVolumeAsync(Guid jobQueueId)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        async Task Exec(string sql, Action<NpgsqlCommand>? bind = null)
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            bind?.Invoke(cmd);
            await cmd.ExecuteNonQueryAsync();
        }

        await Exec("DELETE FROM fps.rate_change_history WHERE jobqueueid = @jqid;", c => c.Parameters.AddWithValue("jqid", jobQueueId));
        await Exec("DELETE FROM fps.job_queue_log WHERE jobqueueid = @jqid;", c => c.Parameters.AddWithValue("jqid", jobQueueId));
        await Exec("DELETE FROM fps.tblstagingtlkptestreqmt WHERE jobqueueid = @jqid;", c => c.Parameters.AddWithValue("jqid", jobQueueId));
        await Exec("DELETE FROM fps.tblstagingtestorproduct WHERE jobqueueid = @jqid;", c => c.Parameters.AddWithValue("jqid", jobQueueId));
        await Exec("DELETE FROM fps.job_queue WHERE jobqueueid = @jqid;", c => c.Parameters.AddWithValue("jqid", jobQueueId));
        await Exec("DELETE FROM fps.tlkptestreqmt WHERE fpsyear = @fpsyear;", c => c.Parameters.AddWithValue("fpsyear", FpsYear));
        await Exec("DELETE FROM fps.testorproduct WHERE fpsyear = @fpsyear;", c => c.Parameters.AddWithValue("fpsyear", FpsYear));
    }
}

