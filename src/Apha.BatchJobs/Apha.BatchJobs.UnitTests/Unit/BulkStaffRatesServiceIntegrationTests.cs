using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.BulkRates.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using NpgsqlTypes;
using Xunit;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// PostgreSQL-backed integration tests:
/// <see cref="BulkStaffRatesService"/> revalidates and applies changes inside a single
/// transaction against SELECT ... FOR UPDATE-locked live rows, using the same shared
/// <see cref="IStaffAnimalValidationService"/> the API validator calls, comparing
/// against the frozen source_*/effective_*/calculated_action/validation_version
/// columns. Mirrors <c>BulkTestRatesServiceIntegrationTests</c>'s structure. Tests use
/// explicit DELETE-based cleanup (not transaction rollback) because ExecuteAsync commits
/// its own transaction internally.
/// </summary>
[Trait("Category", "Integration")]
public sealed class BulkStaffRatesServiceIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString =
        "Host=fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com;Port=5432;Database=batchjobs;Username=fpsdev;Password=ijZFiEr5BnKoiLXxD1g7Zg;SSL Mode=Require;Trust Server Certificate=true";
    private readonly string _connectionString;
    private string? _skipReason;

    public BulkStaffRatesServiceIntegrationTests()
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
                    WHERE m.jobname = {BatchJobNames.BulkStaffRatesUpdate} AND s.status = 'Running'")
                .SingleAsync();

            if (count == 0)
                _skipReason = $"job_master/job_status seed for '{BatchJobNames.BulkStaffRatesUpdate}' + 'Running' is not provisioned.";
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

    private BulkStaffRatesService CreateService() => new(
        new BulkRatesRepository(new TestDbContextFactory(_connectionString), NullLogger<BulkRatesRepository>.Instance),
        NullLogger<BulkStaffRatesService>.Instance);

    private async Task<int> ResolveStatusIdAsync(NpgsqlConnection conn, string status)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT s.statusid FROM fps.job_status s
            JOIN fps.job_master m ON m.jobid = s.jobid
            WHERE m.jobname = @jobname AND s.status = @status;";
        cmd.Parameters.AddWithValue("jobname", BatchJobNames.BulkStaffRatesUpdate);
        cmd.Parameters.AddWithValue("status", status);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task<int> ResolveJobIdAsync(NpgsqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT jobid FROM fps.job_master WHERE jobname = @jobname;";
        cmd.Parameters.AddWithValue("jobname", BatchJobNames.BulkStaffRatesUpdate);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task InsertJobQueueAsync(
        NpgsqlConnection conn, Guid jobQueueId, Guid jobExecutionId, int fpsYear)
    {
        await EnsureYearInMasterAsync(conn, fpsYear);
        var jobId = await ResolveJobIdAsync(conn);
        var statusId = await ResolveStatusIdAsync(conn, "Running");

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO fps.job_queue
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, fpsyear,
                 approved_by, approved_at_utc)
            VALUES
                (@jobqueueid, @jobexecutionid, @jobid, @statusid, 'integration-test-requester', NOW(), @fpsyear,
                 'integration-test-approver', NOW());";
        cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
        cmd.Parameters.AddWithValue("jobexecutionid", jobExecutionId);
        cmd.Parameters.AddWithValue("jobid", jobId);
        cmd.Parameters.AddWithValue("statusid", statusId);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task InsertLiveStaffRowAsync(
        NpgsqlConnection conn, string pcGrade, int fpsYear, decimal payRate, decimal npr, decimal ohr)
    {
        await EnsureStaffPrerequisitesAsync(conn, fpsYear);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO fps.profitcentregrade (pcgrade, divisiongrade, gradecode, profitcentre, payrate, npr, ohr, fpsyear)
            VALUES (@pcgrade, 'IT-TDVG', 'IT-TGRADE', 'ADMIN', @payrate, @npr, @ohr, @fpsyear);";
        cmd.Parameters.AddWithValue("pcgrade", pcGrade);
        cmd.Parameters.AddWithValue("payrate", payRate);
        cmd.Parameters.AddWithValue("npr", npr);
        cmd.Parameters.AddWithValue("ohr", ohr);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task InsertFrozenStagingRowAsync(
        NpgsqlConnection conn, Guid jobQueueId, string pcGrade,
        decimal? payRate, decimal? npr, decimal? ohr,
        string calculatedAction,
        decimal sourcePayRate, decimal sourceNpr, decimal sourceOhr,
        decimal effectivePayRate, decimal effectiveNpr, decimal effectiveOhr,
        int validationVersion)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO fps.tblstagingprofitcentregrade
                (jobqueueid, pcgrade, payrate, npr, ohr,
                 calculated_action, source_payrate, source_npr, source_ohr,
                 effective_payrate, effective_npr, effective_ohr, validation_version)
            VALUES
                (@jqid, @pcgrade, @payrate, @npr, @ohr,
                 @action, @sourcepayrate, @sourcenpr, @sourceohr,
                 @effectivepayrate, @effectivenpr, @effectiveohr, @version);";
        cmd.Parameters.AddWithValue("jqid", jobQueueId);
        cmd.Parameters.AddWithValue("pcgrade", pcGrade);
        cmd.Parameters.AddWithValue("payrate", (object?)payRate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("npr", (object?)npr ?? DBNull.Value);
        cmd.Parameters.AddWithValue("ohr", (object?)ohr ?? DBNull.Value);
        cmd.Parameters.AddWithValue("action", calculatedAction);
        cmd.Parameters.AddWithValue("sourcepayrate", sourcePayRate);
        cmd.Parameters.AddWithValue("sourcenpr", sourceNpr);
        cmd.Parameters.AddWithValue("sourceohr", sourceOhr);
        cmd.Parameters.AddWithValue("effectivepayrate", effectivePayRate);
        cmd.Parameters.AddWithValue("effectivenpr", effectiveNpr);
        cmd.Parameters.AddWithValue("effectiveohr", effectiveOhr);
        cmd.Parameters.AddWithValue("version", validationVersion);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task CleanupAsync(Guid jobQueueId, int fpsYear, string[] pcGrades)
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
        await Exec("DELETE FROM fps.tblstagingprofitcentregrade WHERE jobqueueid = @jqid;", c => c.Parameters.AddWithValue("jqid", jobQueueId));
        await Exec("DELETE FROM fps.job_queue WHERE jobqueueid = @jqid;", c => c.Parameters.AddWithValue("jqid", jobQueueId));
        await Exec("DELETE FROM fps.profitcentregrade WHERE fpsyear = @fpsyear AND pcgrade = ANY(@grades);",
            c => { c.Parameters.AddWithValue("fpsyear", fpsYear); c.Parameters.Add(new NpgsqlParameter("grades", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = pcGrades }); });
        await Exec("DELETE FROM fps.divisiongrade WHERE divisiongrade = 'IT-TDVG' AND fpsyear = @fpsyear;",
            c => c.Parameters.AddWithValue("fpsyear", fpsYear));
        await Exec("DELETE FROM fps.grade WHERE gradecode = 'IT-TGRADE' AND fpsyear = @fpsyear;",
            c => c.Parameters.AddWithValue("fpsyear", fpsYear));
        // Only remove year-master rows that this test created
        await Exec("DELETE FROM fps.tblyearmaster WHERE fpsyear = @fpsyear AND createdby = 'integration-test';",
            c => c.Parameters.AddWithValue("fpsyear", fpsYear));
    }

    private async Task EnsureYearInMasterAsync(NpgsqlConnection conn, int fpsYear)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO fps.tblyearmaster (fpsyear, fpsyearcode, yearstatus, active, createdby)
            VALUES (@year, @code, 'Open', true, 'integration-test')
            ON CONFLICT (fpsyear) DO NOTHING;";
        cmd.Parameters.AddWithValue("year", fpsYear);
        cmd.Parameters.AddWithValue("code", fpsYear.ToString());
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task EnsureStaffPrerequisitesAsync(NpgsqlConnection conn, int fpsYear)
    {
        await EnsureYearInMasterAsync(conn, fpsYear);

        // fps.grade is partitioned â€” use SELECT-then-INSERT
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*)::int FROM fps.grade WHERE gradecode = 'IT-TGRADE' AND fpsyear = @year;";
            cmd.Parameters.AddWithValue("year", fpsYear);
            if ((int)(await cmd.ExecuteScalarAsync())! == 0)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "INSERT INTO fps.grade (gradecode, fpsyear) VALUES ('IT-TGRADE', @year);";
                cmd.Parameters.AddWithValue("year", fpsYear);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // fps.divisiongrade is partitioned â€” use SELECT-then-INSERT
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*)::int FROM fps.divisiongrade WHERE divisiongrade = 'IT-TDVG' AND fpsyear = @year;";
            cmd.Parameters.AddWithValue("year", fpsYear);
            if ((int)(await cmd.ExecuteScalarAsync())! == 0)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "INSERT INTO fps.divisiongrade (divisiongrade, gradecode, division, fpsyear) VALUES ('IT-TDVG', 'IT-TGRADE', 'BSD', @year);";
                cmd.Parameters.AddWithValue("year", fpsYear);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    // â”€â”€ Update action applies the frozen effective_* state and writes history â”€â”€â”€â”€â”€â”€â”€

    [SkippableFact]
    public async Task ExecuteAsync_UpdateAction_AppliesEffectiveStateAndWritesHistory()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2091;
        const string pcGrade = "SA5-S01";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertLiveStaffRowAsync(conn, pcGrade, fpsYear, payRate: 10.00m, npr: 5.00m, ohr: 2.00m);
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        // Live still matches the frozen source (10/5/2); the approved effective target
        // raises PayRate only (12.00), leaving NPR/OHR unchanged.
        await InsertFrozenStagingRowAsync(
            conn, jobQueueId, pcGrade, payRate: 12.00m, npr: 5.00m, ohr: 2.00m,
            calculatedAction: "Update",
            sourcePayRate: 10.00m, sourceNpr: 5.00m, sourceOhr: 2.00m,
            effectivePayRate: 12.00m, effectiveNpr: 5.00m, effectiveOhr: 2.00m,
            validationVersion: 1);

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkStaffRatesUpdate, fpsYear));

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT payrate::numeric, npr::numeric, ohr::numeric FROM fps.profitcentregrade WHERE pcgrade = @pcgrade AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("pcgrade", pcGrade);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(12.00m, r.GetDecimal(0));
                Assert.Equal(5.00m, r.GetDecimal(1));
                Assert.Equal(2.00m, r.GetDecimal(2));
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT fieldname, oldvalue, newvalue FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal("payrate", r.GetString(0));
                Assert.Equal(10.00m, decimal.Parse(r.GetString(1), System.Globalization.CultureInfo.InvariantCulture));
                Assert.Equal(12.00m, decimal.Parse(r.GetString(2), System.Globalization.CultureInfo.InvariantCulture));
                Assert.False(await r.ReadAsync(), "Only payrate changed â€” npr/ohr must not get a history row.");
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.tblstagingprofitcentregrade WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync())!);
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [pcGrade]);
        }
    }

    // â”€â”€ NoChange action applies nothing and writes no history â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [SkippableFact]
    public async Task ExecuteAsync_NoChangeAction_SkipsApplyAndHistory()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2092;
        const string pcGrade = "SA5-S02";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertLiveStaffRowAsync(conn, pcGrade, fpsYear, payRate: 10.00m, npr: 5.00m, ohr: 2.00m);
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertFrozenStagingRowAsync(
            conn, jobQueueId, pcGrade, payRate: 10.00m, npr: 5.00m, ohr: 2.00m,
            calculatedAction: "NoChange",
            sourcePayRate: 10.00m, sourceNpr: 5.00m, sourceOhr: 2.00m,
            effectivePayRate: 10.00m, effectiveNpr: 5.00m, effectiveOhr: 2.00m,
            validationVersion: 1);

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkStaffRatesUpdate, fpsYear));

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync())!);
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [pcGrade]);
        }
    }

    // â”€â”€ Unexpected actions: Insert, ZeroRateWithdrawal, NotFound, Invalid â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [SkippableFact]
    public async Task ExecuteAsync_InsertAction_ThrowsWithDiagnosticMessage()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        var ex = await AssertUnexpectedActionThrowsAsync("Insert", "SA5-S10", 2093);
        Assert.Contains("Staff Insert is not supported", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [SkippableFact]
    public async Task ExecuteAsync_UnexpectedAction_ZeroRateWithdrawal_Throws()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        await AssertUnexpectedActionThrowsAsync("ZeroRateWithdrawal", "SA5-S11", 2093);
    }

    [SkippableFact]
    public async Task ExecuteAsync_UnexpectedAction_NotFound_Throws()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        await AssertUnexpectedActionThrowsAsync("NotFound", "SA5-S12", 2093);
    }

    [SkippableFact]
    public async Task ExecuteAsync_UnexpectedAction_Invalid_Throws()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        await AssertUnexpectedActionThrowsAsync("Invalid", "SA5-S13", 2093);
    }

    private async Task<InvalidOperationException> AssertUnexpectedActionThrowsAsync(string action, string pcGrade, int fpsYear)
    {
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertFrozenStagingRowAsync(
            conn, jobQueueId, pcGrade,
            payRate: 10m, npr: 5m, ohr: 2m,
            calculatedAction: action,
            sourcePayRate: 10m, sourceNpr: 5m, sourceOhr: 2m,
            effectivePayRate: 10m, effectiveNpr: 5m, effectiveOhr: 2m,
            validationVersion: 1);

        try
        {
            return await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkStaffRatesUpdate, fpsYear)));
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [pcGrade]);
        }
    }
}

