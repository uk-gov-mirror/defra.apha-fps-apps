using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Interfaces;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.BulkRates;
using Apha.BatchJobs.Infrastructure.Services.BulkRates;
using Apha.Common.BulkRates.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using NSubstitute;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed integration tests:
/// <see cref="BulkTestRatesService"/> revalidates and applies changes inside a single
/// transaction against SELECT ... FOR UPDATE-locked live rows, using the same shared
/// <see cref="IBulkRatesValidationService"/> the API validator calls. Tests use explicit
/// DELETE-based cleanup (not transaction rollback) because ExecuteAsync commits its own
/// transaction internally.
/// </summary>
public sealed class BulkTestRatesServiceIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=batch_jobs_foundation_db_cloud;Username=postgres;Password=admin123;SSL Mode=Disable";
    private readonly string _connectionString;
    private string? _skipReason;

    public BulkTestRatesServiceIntegrationTests()
    {
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
        new TestDbContextFactory(_connectionString),
        new BulkRatesRepository(new TestDbContextFactory(_connectionString), NullLogger<BulkRatesRepository>.Instance),
        Substitute.For<IJobExecutionRepository>(),
        new BulkRatesValidationService(),
        NullLogger<BulkTestRatesService>.Instance);

    private async Task<int> ResolveStatusIdAsync(NpgsqlConnection conn, string status)
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

    private async Task<int> ResolveJobIdAsync(NpgsqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT jobid FROM fps.job_master WHERE jobname = @jobname;";
        cmd.Parameters.AddWithValue("jobname", BatchJobNames.BulkTestRatesUpdate);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task InsertJobQueueAsync(
        NpgsqlConnection conn, Guid jobQueueId, Guid jobExecutionId, int fpsYear,
        int? uploadVersion, int? activeDownloadVersion)
    {
        var jobId = await ResolveJobIdAsync(conn);
        var statusId = await ResolveStatusIdAsync(conn, "Running");

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO fps.job_queue
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, fpsyear,
                 approved_by, approved_at_utc, upload_version, active_download_version)
            VALUES
                (@jobqueueid, @jobexecutionid, @jobid, @statusid, 'integration-test-requester', NOW(), @fpsyear,
                 'integration-test-approver', NOW(), @uploadversion, @activedownloadversion);";
        cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
        cmd.Parameters.AddWithValue("jobexecutionid", jobExecutionId);
        cmd.Parameters.AddWithValue("jobid", jobId);
        cmd.Parameters.AddWithValue("statusid", statusId);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.AddWithValue("uploadversion", (object?)uploadVersion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("activedownloadversion", (object?)activeDownloadVersion ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task CleanupAsync(Guid jobQueueId, int fpsYear, string[] testCodes)
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
        await Exec("DELETE FROM fps.tlkptestreqmt WHERE fpsyear = @fpsyear AND testcode = ANY(@codes);",
            c => { c.Parameters.AddWithValue("fpsyear", fpsYear); c.Parameters.Add(new NpgsqlParameter("codes", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text) { Value = testCodes }); });
        await Exec("DELETE FROM fps.testorproduct WHERE fpsyear = @fpsyear AND itemcode = ANY(@codes);",
            c => { c.Parameters.AddWithValue("fpsyear", fpsYear); c.Parameters.Add(new NpgsqlParameter("codes", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text) { Value = testCodes }); });
        await Exec("DELETE FROM fps.testreq_log WHERE fpsyear = @fpsyear AND testcode = ANY(@codes);",
            c => { c.Parameters.AddWithValue("fpsyear", fpsYear); c.Parameters.Add(new NpgsqlParameter("codes", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Text) { Value = testCodes }); });
        await Exec("DELETE FROM fps.tlkpproject WHERE fpsyear = @fpsyear;", c => c.Parameters.AddWithValue("fpsyear", fpsYear));
        await Exec("DELETE FROM fps.tlkpprogram WHERE fpsyear = @fpsyear;", c => c.Parameters.AddWithValue("fpsyear", fpsYear));
    }

    // â”€â”€ FEC existing row, null rate â†’ zero + retain â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [SkippableFact]
    public async Task ExecuteAsync_FecExistingRowBlankRate_ZeroesAndRetains()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2098;
        const string testCode = "T-WK01";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@code, 'Existing item', 10.00, 'PT', 'X', 10.00, @year);";
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtestorproduct
                    (jobqueueid, testcode, unitpricevla, defraunitprice, fecnewrate,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES
                    (@jqid, @code, 10.00, 10.00, NULL, 'ZeroRateWithdrawal', 0, 10.00, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("code", testCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear));

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT unitpricevla::numeric, defraunitprice::numeric FROM fps.testorproduct WHERE itemcode = @code AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(0m, r.GetDecimal(0));
                Assert.Equal(0m, r.GetDecimal(1));
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT changetype, newvalue FROM fps.rate_change_history WHERE jobqueueid = @jqid ORDER BY fieldname;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                await using var r = await cmd.ExecuteReaderAsync();
                var rows = new List<(string, string)>();
                while (await r.ReadAsync())
                    rows.Add((r.GetString(0), r.GetString(1)));

                Assert.Equal(2, rows.Count);
                Assert.All(rows, row => Assert.Equal("ZeroRateWithdrawal", row.Item1));
                Assert.All(rows, row => Assert.Equal("0", row.Item2));
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    // â”€â”€ AGRUP existing row, null rate â†’ zero + retain â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [SkippableFact]
    public async Task ExecuteAsync_AgrupExistingRowBlankRate_ZeroesAndRetains()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2097;
        const string testCode = "T-WK02";
        const string buyer = "BYR1";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // Live FEC row so AGRUP's TestCode existence check resolves without staging a FEC row.
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@code, 'Existing item', 40.00, 'PT', 'X', 40.00, @year);";
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, projectbuyercode, testbuyercode, fpsyear)
                VALUES (@code, @buyer, 5.00, @buyer, NULL, @year);";
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtlkptestreqmt
                    (jobqueueid, testcode, buyer, agrup, agrupnew, projectbuyercode, testbuyercode,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES
                    (@jqid, @code, @buyer, 5.00, NULL, @buyer, NULL, 'ZeroRateWithdrawal', 0, 5.00, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear));

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT unitprice::numeric FROM fps.tlkptestreqmt WHERE testcode = @code AND buyer = @buyer AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("buyer", buyer);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(0m, r.GetDecimal(0));
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT changetype, newvalue FROM fps.rate_change_history WHERE jobqueueid = @jqid AND ratecategory = 'AGRUP';";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal("ZeroRateWithdrawal", r.GetString(0));
                Assert.Equal("0", r.GetString(1));
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    // â”€â”€ New AGRUP insert uses staged routing fields, not hardcoded Buyer â”€â”€

    [SkippableFact]
    public async Task ExecuteAsync_NewAgrupRow_UsesStagedRoutingFields_NotHardcodedBuyer()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2096;
        const string testCode = "T-WK03";
        const string buyer = "NEWBUYER";
        const string projectBuyerCode = "PRJ-WK03";
        const string testBuyerCode = "TBC-WK03";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@code, 'Existing item', 12.00, 'PT', 'X', 12.00, @year);";
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO fps.tlkpprogram (programno, sector_name, fpsyear) VALUES ('PRG-WK03', 'Wave-WK03', @year);";
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tlkpproject
                    (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                     isdefraproject, incomeaccountcode, fpsyear)
                VALUES
                    (@code, 'WK03 Project', 'PRG-WK03', 'WK03 Customer', 0, 0, 'Active', 'General', 0, 'IA-WK03', @year);";
            cmd.Parameters.AddWithValue("code", projectBuyerCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtlkptestreqmt
                    (jobqueueid, testcode, buyer, agrup, agrupnew, projectbuyercode, testbuyercode,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES
                    (@jqid, @code, @buyer, NULL, 15.00, @projectbuyercode, @testbuyercode, 'Insert', 15.00, NULL, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            cmd.Parameters.AddWithValue("projectbuyercode", projectBuyerCode);
            cmd.Parameters.AddWithValue("testbuyercode", testBuyerCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear));

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT unitprice::numeric, projectbuyercode, testbuyercode
                    FROM fps.tlkptestreqmt WHERE testcode = @code AND buyer = @buyer AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("buyer", buyer);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(15.00m, r.GetDecimal(0));
                // Staged ProjectBuyerCode/TestBuyerCode are written verbatim â€” no
                // longer the old hardcoded ProjectBuyerCode = Buyer / omitted TestBuyerCode.
                Assert.Equal(projectBuyerCode, r.GetString(1));
                Assert.NotEqual(buyer, r.GetString(1));
                Assert.Equal(testBuyerCode, r.GetString(2));
            }

            // Audit completeness: the applied value must match what was frozen as
            // "approved" at release (effective_new_rate = 15.00 staged above), not merely
            // some independently re-derived number that happens to look right.
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT newvalue FROM fps.rate_change_history WHERE jobqueueid = @jqid AND fieldname = 'unitprice';";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal("15.00", r.GetString(0));
            }
        }
        finally
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM fps.tlkptestreqmt WHERE testcode = @code AND buyer = @buyer AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("buyer", buyer);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await cmd.ExecuteNonQueryAsync();
            }
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    // ── Frozen calculated_action disagrees with re-derivation → throws ──────────────

    [SkippableFact]
    public async Task ExecuteAsync_WhenFrozenActionDriftsFromRederivation_ThrowsAndAppliesNothing()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2095;
        const string testCode = "T-WK04";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // Live row already exists â€” a frozen "Insert" can never be correct for it.
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@code, 'Existing item', 20.00, 'PT', 'X', 20.00, @year);";
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtestorproduct
                    (jobqueueid, testcode, unitpricevla, defraunitprice, fecnewrate,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES
                    (@jqid, @code, 20.00, 20.00, 25.00, 'Insert', 25.00, NULL, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("code", testCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear)));

            Assert.Contains("drift", ex.Message, StringComparison.OrdinalIgnoreCase);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT unitpricevla::numeric FROM fps.testorproduct WHERE itemcode = @code AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(20.00m, r.GetDecimal(0));
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync())!);
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    // ── Live source rate drifts to a third value that doesn't change the
    // frozen action/effective-rate → throws. CalculatedAction/EffectiveNewRate alone cannot
    // catch this (both stay 'Update'/120 whether the live baseline is £100 or £110) — only
    // comparing source_current_rate against the live rate locked just now can. ──

    [SkippableFact]
    public async Task ExecuteAsync_WhenLiveSourceRateDriftsButActionAndRateUnchanged_ThrowsAndAppliesNothing()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2096;
        const string testCode = "T-WK04B";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // Live rate has drifted to £110 since release — the frozen source was £100. The
        // approved change (£120) and its classification ('Update') are unaffected by this
        // drift on their own: £120 != £110 just as £120 != £100.
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@code, 'Drifted item', 110.00, 'PT', 'X', 110.00, @year);";
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtestorproduct
                    (jobqueueid, testcode, unitpricevla, defraunitprice, fecnewrate,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES
                    (@jqid, @code, 100.00, 100.00, 120.00, 'Update', 120.00, 100.00, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("code", testCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear)));

            Assert.Contains("Live rate changed after approval", ex.Message);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT unitpricevla::numeric, defraunitprice::numeric FROM fps.testorproduct WHERE itemcode = @code AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(110.00m, r.GetDecimal(0));
                Assert.Equal(110.00m, r.GetDecimal(1));
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync())!);
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    // ── FEC's two live rate columns (UnitPriceVla, DefraUnitPrice) diverge from
    // each other → throws even though the column the freeze compares against (DefraUnitPrice)
    // still matches the frozen source. Comparing DefraUnitPrice alone would miss this. ──

    [SkippableFact]
    public async Task ExecuteAsync_WhenFecUnitPriceVlaDivergesFromDefraUnitPrice_ThrowsAndAppliesNothing()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2097;
        const string testCode = "T-WK04C";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // DefraUnitPrice (100) still matches the frozen source, but UnitPriceVla has moved to
        // 110 via some other write path — comparing DefraUnitPrice alone would look unchanged.
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@code, 'Diverged item', 110.00, 'PT', 'X', 100.00, @year);";
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtestorproduct
                    (jobqueueid, testcode, unitpricevla, defraunitprice, fecnewrate,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES
                    (@jqid, @code, 100.00, 100.00, 120.00, 'Update', 120.00, 100.00, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("code", testCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear)));

            Assert.Contains("Live rate changed after approval", ex.Message);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT unitpricevla::numeric, defraunitprice::numeric FROM fps.testorproduct WHERE itemcode = @code AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(110.00m, r.GetDecimal(0));
                Assert.Equal(100.00m, r.GetDecimal(1));
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync())!);
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    // ── BC-05 interim rule: live-only AGRUP row under a withdrawn FEC code ──────────

    [SkippableFact]
    public async Task ExecuteAsync_LiveAgrupRowNotInSnapshot_UnderWithdrawnFec_Throws()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2094;
        const string testCode = "T-WK05";
        const string buyer = "LIVEBUYER";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@code, 'Existing item', 30.00, 'PT', 'X', 30.00, @year);";
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        // Live AGRUP row under this TestCode that was never staged and never part of any
        // download snapshot â€” exactly the gap the BC-05 check must catch.
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, projectbuyercode, testbuyercode, fpsyear)
                VALUES (@code, @buyer, 50.00, @buyer, NULL, @year);";
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        // FEC withdrawal (correctly frozen â€” no drift here, so the drift check passes and
        // the BC-05 check is what must fire).
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtestorproduct
                    (jobqueueid, testcode, unitpricevla, defraunitprice, fecnewrate,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES
                    (@jqid, @code, 30.00, 30.00, NULL, 'ZeroRateWithdrawal', 0, 30.00, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("code", testCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear)));

            Assert.Contains("BC-05", ex.Message, StringComparison.OrdinalIgnoreCase);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT unitpricevla::numeric FROM fps.testorproduct WHERE itemcode = @code AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(30.00m, r.GetDecimal(0));
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT unitprice::numeric FROM fps.tlkptestreqmt WHERE testcode = @code AND buyer = @buyer AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("buyer", buyer);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(50.00m, r.GetDecimal(0));
            }
        }
        finally
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM fps.tlkptestreqmt WHERE testcode = @code AND buyer = @buyer AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("buyer", buyer);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await cmd.ExecuteNonQueryAsync();
            }
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    // ── A live reference removed after approval must fail the worker, not silently apply ──

    [SkippableFact]
    public async Task ExecuteAsync_LiveProjectRemovedAfterFreeze_ThrowsAndAppliesNothing()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2093;
        const string testCode = "T-WK07";
        const string buyer = "RMVBUYER";
        const string projectBuyerCode = "PRJ-WK07";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // Live FEC row this new AGRUP row routes against.
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@code, 'Existing item', 40.00, 'PT', 'X', 40.00, @year);";
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        // Project valid at "release" time — this is what the release-time freeze would have frozen against.
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO fps.tlkpprogram (programno, sector_name, fpsyear) VALUES ('PRG-WK07', 'Wave-WK07', @year);";
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tlkpproject
                    (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                     isdefraproject, incomeaccountcode, fpsyear)
                VALUES
                    (@code, 'WK07 Project', 'PRG-WK07', 'WK07 Customer', 0, 0, 'Active', 'General', 0, 'IA-WK07', @year);";
            cmd.Parameters.AddWithValue("code", projectBuyerCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtlkptestreqmt
                    (jobqueueid, testcode, buyer, agrup, agrupnew, projectbuyercode, testbuyercode,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES
                    (@jqid, @code, @buyer, NULL, 25.00, @projectbuyercode, NULL, 'Insert', 25.00, NULL, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            cmd.Parameters.AddWithValue("projectbuyercode", projectBuyerCode);
            await cmd.ExecuteNonQueryAsync();
        }

        // The project is removed AFTER release/freeze but BEFORE worker execution — the exact
        // scenario this test covers: a live reference disappearing between approval and execution.
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "DELETE FROM fps.tlkpproject WHERE parentproject = @code AND fpsyear = @year;";
            cmd.Parameters.AddWithValue("code", projectBuyerCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear)));

            Assert.Contains("revalidation blocked", ex.Message, StringComparison.OrdinalIgnoreCase);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.tlkptestreqmt WHERE testcode = @code AND buyer = @buyer AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("buyer", buyer);
                cmd.Parameters.AddWithValue("year", fpsYear);
                Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync())!);
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync())!);
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    // ── FEC insert must precede a dependent new-AGRUP-in-the-same-upload insert ─────

    [SkippableFact]
    public async Task ExecuteAsync_NewFecAndDependentNewAgrup_SameUpload_BothApply()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2092;
        const string testCode = "T-WK08";
        const string buyer = "DEPBUYER";
        const string projectBuyerCode = "PRJ-WK08";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // No live fps.testorproduct row — the FEC TestCode is brand new in this same upload,
        // and the dependent AGRUP row must be able to route against it via the staged FEC sheet
        // (not a live lookup), proving FEC applies first within the transaction (spec §2.4/§15.2).
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO fps.tlkpprogram (programno, sector_name, fpsyear) VALUES ('PRG-WK08', 'Wave-WK08', @year);";
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tlkpproject
                    (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                     isdefraproject, incomeaccountcode, fpsyear)
                VALUES
                    (@code, 'WK08 Project', 'PRG-WK08', 'WK08 Customer', 0, 0, 'Active', 'General', 0, 'IA-WK08', @year);";
            cmd.Parameters.AddWithValue("code", projectBuyerCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtestorproduct
                    (jobqueueid, testcode, unitpricevla, defraunitprice, fecnewrate,
                     itemdescription, shortdescription, owner,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES
                    (@jqid, @code, NULL, NULL, 18.00, 'WK08 Item', 'WK08', 'PT', 'Insert', 18.00, NULL, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("code", testCode);
            await cmd.ExecuteNonQueryAsync();
        }
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtlkptestreqmt
                    (jobqueueid, testcode, buyer, agrup, agrupnew, projectbuyercode, testbuyercode,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES
                    (@jqid, @code, @buyer, NULL, 22.00, @projectbuyercode, NULL, 'Insert', 22.00, NULL, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            cmd.Parameters.AddWithValue("projectbuyercode", projectBuyerCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear));

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT unitpricevla::numeric FROM fps.testorproduct WHERE itemcode = @code AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(18.00m, r.GetDecimal(0));
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT unitprice::numeric FROM fps.tlkptestreqmt WHERE testcode = @code AND buyer = @buyer AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("buyer", buyer);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(22.00m, r.GetDecimal(0));
            }
        }
        finally
        {
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM fps.tlkptestreqmt WHERE testcode = @code AND buyer = @buyer AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("buyer", buyer);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await cmd.ExecuteNonQueryAsync();
            }
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    // ── A duplicate invocation must not reapply an already-committed change ────────

    [SkippableFact]
    public async Task ExecuteAsync_CalledTwiceForSameRequest_SecondCallThrows_DoesNotReapply()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2091;
        const string testCode = "T-WK09";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@code, 'Existing item', 5.00, 'PT', 'X', 5.00, @year);";
            cmd.Parameters.AddWithValue("code", testCode);
            cmd.Parameters.AddWithValue("year", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtestorproduct
                    (jobqueueid, testcode, unitpricevla, defraunitprice, fecnewrate,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES
                    (@jqid, @code, 5.00, 5.00, 9.00, 'Update', 9.00, 5.00, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("code", testCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            // First call commits normally and clears staging (spec §10.6).
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear));

            int historyCountAfterFirstRun;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                historyCountAfterFirstRun = (int)(await cmd.ExecuteScalarAsync())!;
            }
            Assert.Equal(2, historyCountAfterFirstRun); // unitpricevla + defraunitprice

            // Second call for the same JobExecutionId/JobQueueId — staging is empty, so the
            // worker must reject it rather than silently doing nothing or reapplying.
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear)));
            Assert.Contains("no staging rows found", ex.Message, StringComparison.OrdinalIgnoreCase);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT unitpricevla::numeric FROM fps.testorproduct WHERE itemcode = @code AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("code", testCode);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(9.00m, r.GetDecimal(0)); // unchanged by the rejected second call
            }

            int historyCountAfterSecondAttempt;
            await using (var cmd2 = conn.CreateCommand())
            {
                cmd2.CommandText = "SELECT COUNT(*)::int FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd2.Parameters.AddWithValue("jqid", jobQueueId);
                historyCountAfterSecondAttempt = (int)(await cmd2.ExecuteScalarAsync())!;
            }
            Assert.Equal(historyCountAfterFirstRun, historyCountAfterSecondAttempt);
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    // ── testreq_log audit ────────────────────────────────────────────────────

    private static async Task<int> CountTestreqLogAsync(
        NpgsqlConnection conn, string testCode, string buyer, int fpsYear)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(*)::int FROM fps.testreq_log
            WHERE testcode = @testcode AND buyer = @buyer AND fpsyear = @fpsyear;";
        cmd.Parameters.AddWithValue("testcode", testCode);
        cmd.Parameters.AddWithValue("buyer", buyer);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    private static async Task<(double UnitPrice, string InsertDelete, short? Active)> ReadLatestTestreqLogAsync(
        NpgsqlConnection conn, string testCode, string buyer, int fpsYear)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT unitprice, insert_delete, active
            FROM fps.testreq_log
            WHERE testcode = @testcode AND buyer = @buyer AND fpsyear = @fpsyear
            ORDER BY sequenceno DESC LIMIT 1;";
        cmd.Parameters.AddWithValue("testcode", testCode);
        cmd.Parameters.AddWithValue("buyer", buyer);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        await using var r = await cmd.ExecuteReaderAsync();
        Assert.True(await r.ReadAsync(), "Expected at least one testreq_log row.");
        return (r.GetDouble(0), r.GetString(1).Trim(), r.IsDBNull(2) ? null : r.GetInt16(2));
    }

    [SkippableFact]
    public async Task ExecuteAsync_AgrupInsert_WritesOneIRowToTestreqLog()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2091;
        const string testCode = "T-TRL-I";
        const string buyer = "VLA";
        const string projectBuyerCode = "PROJ-I";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tlkpprogram (programno, sector_name, fpsyear) VALUES ('PRG-TRL-I', 'TRL', @y) ON CONFLICT DO NOTHING;
                INSERT INTO fps.tlkpproject
                    (parentproject, projecttitle, program, customer, transferincome, custincome,
                     projectstatus, disease, isdefraproject, incomeaccountcode, fpsyear)
                VALUES (@p, 'TRL-I Project', 'PRG-TRL-I', 'TRL', 0, 0, 'Active', 'General', 0, 'IA-TRL-I', @y)
                ON CONFLICT DO NOTHING;
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@tc, 'Desc', 10.00, 'PT', 'Short', 10.00, @y) ON CONFLICT DO NOTHING;";
            cmd.Parameters.AddWithValue("y", fpsYear);
            cmd.Parameters.AddWithValue("p", projectBuyerCode);
            cmd.Parameters.AddWithValue("tc", testCode);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtlkptestreqmt
                    (jobqueueid, testcode, buyer, agrup, agrupnew, projectbuyercode,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES (@jqid, @tc, @buyer, NULL, 20.00, @pbc, 'Insert', 20.00, NULL, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("tc", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            cmd.Parameters.AddWithValue("pbc", projectBuyerCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear));

            var count = await CountTestreqLogAsync(conn, testCode, buyer, fpsYear);
            Assert.Equal(1, count);

            var (unitPrice, insertDelete, active) = await ReadLatestTestreqLogAsync(conn, testCode, buyer, fpsYear);
            Assert.Equal(20.00, unitPrice, precision: 4);
            Assert.Equal("I", insertDelete);
            Assert.Equal((short)1, active);
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_AgrupUpdate_WritesOneIRowToTestreqLog()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2092;
        const string testCode = "T-TRL-U";
        const string buyer = "VLA";
        const string projectBuyerCode = "PROJ-U";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tlkpprogram (programno, sector_name, fpsyear) VALUES ('PRG-TRL-U', 'TRL', @y) ON CONFLICT DO NOTHING;
                INSERT INTO fps.tlkpproject
                    (parentproject, projecttitle, program, customer, transferincome, custincome,
                     projectstatus, disease, isdefraproject, incomeaccountcode, fpsyear)
                VALUES (@p, 'TRL-U Project', 'PRG-TRL-U', 'TRL', 0, 0, 'Active', 'General', 0, 'IA-TRL-U', @y)
                ON CONFLICT DO NOTHING;
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@tc, 'Desc', 10.00, 'PT', 'Short', 10.00, @y) ON CONFLICT DO NOTHING;
                INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, norequired, projectbuyercode, datecreated, active, fpsyear)
                VALUES (@tc, @buyer, 10.00, 5, @p, NOW(), 1, @y);";
            cmd.Parameters.AddWithValue("y", fpsYear);
            cmd.Parameters.AddWithValue("p", projectBuyerCode);
            cmd.Parameters.AddWithValue("tc", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtlkptestreqmt
                    (jobqueueid, testcode, buyer, agrup, agrupnew, projectbuyercode,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES (@jqid, @tc, @buyer, 10.00, 25.00, @pbc, 'Update', 25.00, 10.00, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("tc", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            cmd.Parameters.AddWithValue("pbc", projectBuyerCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear));

            var count = await CountTestreqLogAsync(conn, testCode, buyer, fpsYear);
            Assert.Equal(1, count);

            // Full snapshot: verify the complete final live-row image was persisted
            await using (var snapCmd = conn.CreateCommand())
            {
                snapCmd.CommandText = @"
                    SELECT unitprice, insert_delete, norequired, projectbuyercode, testbuyercode, active
                    FROM fps.testreq_log
                    WHERE testcode = @testcode AND buyer = @buyer AND fpsyear = @fpsyear
                    ORDER BY sequenceno DESC LIMIT 1;";
                snapCmd.Parameters.AddWithValue("testcode", testCode);
                snapCmd.Parameters.AddWithValue("buyer", buyer);
                snapCmd.Parameters.AddWithValue("fpsyear", fpsYear);
                await using var sr = await snapCmd.ExecuteReaderAsync();
                Assert.True(await sr.ReadAsync(), "Expected testreq_log row.");
                Assert.Equal(25.00, sr.GetDouble(0), precision: 4);        // unitprice = new rate
                Assert.Equal("I", sr.GetString(1).Trim());                 // insert_delete
                Assert.Equal(5, sr.IsDBNull(2) ? (int?)null : sr.GetInt32(2)); // norequired from live row
                Assert.Equal(projectBuyerCode, sr.IsDBNull(3) ? null : sr.GetString(3)); // projectbuyercode
                Assert.Null(sr.IsDBNull(4) ? null : sr.GetString(4));      // testbuyercode (null in seed)
                Assert.Equal((short)1, sr.IsDBNull(5) ? (short?)null : sr.GetInt16(5)); // active from live row
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_AgrupZeroRateWithdrawal_WritesOneIRowWithZeroRate()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2093;
        const string testCode = "T-TRL-Z";
        const string buyer = "VLA";
        const string projectBuyerCode = "PROJ-Z";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tlkpprogram (programno, sector_name, fpsyear) VALUES ('PRG-TRL-Z', 'TRL', @y) ON CONFLICT DO NOTHING;
                INSERT INTO fps.tlkpproject
                    (parentproject, projecttitle, program, customer, transferincome, custincome,
                     projectstatus, disease, isdefraproject, incomeaccountcode, fpsyear)
                VALUES (@p, 'TRL-Z Project', 'PRG-TRL-Z', 'TRL', 0, 0, 'Active', 'General', 0, 'IA-TRL-Z', @y)
                ON CONFLICT DO NOTHING;
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@tc, 'Desc', 5.00, 'PT', 'Short', 5.00, @y) ON CONFLICT DO NOTHING;
                INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, norequired, projectbuyercode, datecreated, active, fpsyear)
                VALUES (@tc, @buyer, 5.00, 2, @p, NOW(), 1, @y);";
            cmd.Parameters.AddWithValue("y", fpsYear);
            cmd.Parameters.AddWithValue("p", projectBuyerCode);
            cmd.Parameters.AddWithValue("tc", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtlkptestreqmt
                    (jobqueueid, testcode, buyer, agrup, agrupnew, projectbuyercode,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES (@jqid, @tc, @buyer, 5.00, NULL, @pbc, 'ZeroRateWithdrawal', 0.00, 5.00, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("tc", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            cmd.Parameters.AddWithValue("pbc", projectBuyerCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear));

            var count = await CountTestreqLogAsync(conn, testCode, buyer, fpsYear);
            Assert.Equal(1, count);

            var (unitPrice, insertDelete, _) = await ReadLatestTestreqLogAsync(conn, testCode, buyer, fpsYear);
            Assert.Equal(0.00, unitPrice, precision: 4);
            Assert.Equal("I", insertDelete);
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_AgrupUnchanged_WritesNothingToTestreqLog()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2094;
        const string testCode = "T-TRL-N";
        const string buyer = "VLA";
        const string projectBuyerCode = "PROJ-N";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tlkpprogram (programno, sector_name, fpsyear) VALUES ('PRG-TRL-N', 'TRL', @y) ON CONFLICT DO NOTHING;
                INSERT INTO fps.tlkpproject
                    (parentproject, projecttitle, program, customer, transferincome, custincome,
                     projectstatus, disease, isdefraproject, incomeaccountcode, fpsyear)
                VALUES (@p, 'TRL-N Project', 'PRG-TRL-N', 'TRL', 0, 0, 'Active', 'General', 0, 'IA-TRL-N', @y)
                ON CONFLICT DO NOTHING;
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@tc, 'Desc', 12.00, 'PT', 'Short', 12.00, @y) ON CONFLICT DO NOTHING;
                INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, norequired, projectbuyercode, datecreated, active, fpsyear)
                VALUES (@tc, @buyer, 12.00, 3, @p, NOW(), 1, @y);";
            cmd.Parameters.AddWithValue("y", fpsYear);
            cmd.Parameters.AddWithValue("p", projectBuyerCode);
            cmd.Parameters.AddWithValue("tc", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtlkptestreqmt
                    (jobqueueid, testcode, buyer, agrup, agrupnew, projectbuyercode,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES (@jqid, @tc, @buyer, 12.00, 12.00, @pbc, 'NoChange', 12.00, 12.00, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("tc", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            cmd.Parameters.AddWithValue("pbc", projectBuyerCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear));

            var count = await CountTestreqLogAsync(conn, testCode, buyer, fpsYear);
            Assert.Equal(0, count);
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }

    [SkippableFact]
    public async Task ExecuteAsync_AgrupInsert_RollbackRemovesBothLiveAndTestreqLogRows()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        // Drift: frozen='Insert' but live row already exists → re-derivation='Update' → throw before commit
        const int fpsYear = 2095;
        const string testCode = "T-TRL-RB";
        const string buyer = "VLA";
        const string projectBuyerCode = "PROJ-RB";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tlkpprogram (programno, sector_name, fpsyear) VALUES ('PRG-TRL-RB', 'TRL', @y) ON CONFLICT DO NOTHING;
                INSERT INTO fps.tlkpproject
                    (parentproject, projecttitle, program, customer, transferincome, custincome,
                     projectstatus, disease, isdefraproject, incomeaccountcode, fpsyear)
                VALUES (@p, 'TRL-RB Project', 'PRG-TRL-RB', 'TRL', 0, 0, 'Active', 'General', 0, 'IA-TRL-RB', @y)
                ON CONFLICT DO NOTHING;
                INSERT INTO fps.testorproduct (itemcode, itemdescription, unitpricevla, owner, shortdescription, defraunitprice, fpsyear)
                VALUES (@tc, 'Desc', 8.00, 'PT', 'Short', 8.00, @y) ON CONFLICT DO NOTHING;
                INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, norequired, projectbuyercode, datecreated, active, fpsyear)
                VALUES (@tc, @buyer, 8.00, 1, @p, NOW(), 1, @y);";
            cmd.Parameters.AddWithValue("y", fpsYear);
            cmd.Parameters.AddWithValue("p", projectBuyerCode);
            cmd.Parameters.AddWithValue("tc", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            await cmd.ExecuteNonQueryAsync();
        }

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear, uploadVersion: 1, activeDownloadVersion: null);

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
                INSERT INTO fps.tblstagingtestorproduct
                    (jobqueueid, testcode, unitpricevla, defraunitprice, fecnewrate,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES (@jqid, @tc, 8.00, 8.00, 8.00, 'NoChange', 8.00, 8.00, 1);
                INSERT INTO fps.tblstagingtlkptestreqmt
                    (jobqueueid, testcode, buyer, agrup, agrupnew, projectbuyercode,
                     calculated_action, effective_new_rate, source_current_rate, validation_version)
                VALUES (@jqid, @tc, @buyer, NULL, 30.00, @pbc, 'Insert', 30.00, NULL, 1);";
            cmd.Parameters.AddWithValue("jqid", jobQueueId);
            cmd.Parameters.AddWithValue("tc", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            cmd.Parameters.AddWithValue("pbc", projectBuyerCode);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkTestRatesUpdate, fpsYear)));

            // Nothing committed — testreq_log must be empty, live rate unchanged
            Assert.Equal(0, await CountTestreqLogAsync(conn, testCode, buyer, fpsYear));

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT unitprice::numeric FROM fps.tlkptestreqmt WHERE testcode = @tc AND buyer = @buyer AND fpsyear = @y;";
            cmd.Parameters.AddWithValue("tc", testCode);
            cmd.Parameters.AddWithValue("buyer", buyer);
            cmd.Parameters.AddWithValue("y", fpsYear);
            await using var r = await cmd.ExecuteReaderAsync();
            Assert.True(await r.ReadAsync());
            Assert.Equal(8.00m, r.GetDecimal(0));
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [testCode]);
        }
    }
}
