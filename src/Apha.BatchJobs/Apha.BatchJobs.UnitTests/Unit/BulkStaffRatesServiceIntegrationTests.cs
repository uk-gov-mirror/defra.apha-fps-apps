using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.BulkRates;
using Apha.BatchJobs.Infrastructure.Services.BulkRates;
using Apha.Common.BulkRates.Validation.StaffAnimal;
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
public sealed class BulkStaffRatesServiceIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=batch_jobs_foundation_db_cloud;Username=postgres;Password=admin123;SSL Mode=Disable";
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
        new TestDbContextFactory(_connectionString),
        new BulkRatesRepository(new TestDbContextFactory(_connectionString), NullLogger<BulkRatesRepository>.Instance),
        new StaffAnimalValidationService(),
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
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO fps.profitcentregrade (pcgrade, divisiongrade, gradecode, profitcentre, payrate, npr, ohr, fpsyear)
            VALUES (@pcgrade, 'X', 'X', 'X', @payrate, @npr, @ohr, @fpsyear);";
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
    }

    // ── Update action applies the frozen effective_* state and writes history ───────

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
            calculatedAction: StaffAnimalCalculatedAction.Update,
            sourcePayRate: 10.00m, sourceNpr: 5.00m, sourceOhr: 2.00m,
            effectivePayRate: 12.00m, effectiveNpr: 5.00m, effectiveOhr: 2.00m,
            validationVersion: StaffAnimalValidationVersion.Current);

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
                Assert.Equal("10.00", r.GetString(1));
                Assert.Equal("12.00", r.GetString(2));
                Assert.False(await r.ReadAsync(), "Only payrate changed — npr/ohr must not get a history row.");
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

    // ── NoChange action applies nothing and writes no history ──────────────────────

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
            calculatedAction: StaffAnimalCalculatedAction.NoChange,
            sourcePayRate: 10.00m, sourceNpr: 5.00m, sourceOhr: 2.00m,
            effectivePayRate: 10.00m, effectiveNpr: 5.00m, effectiveOhr: 2.00m,
            validationVersion: StaffAnimalValidationVersion.Current);

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

    // ── Live source drifted since release → throws and applies nothing ─────────────

    [SkippableFact]
    public async Task ExecuteAsync_WhenLiveSourceDrifts_ThrowsAndAppliesNothing()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2093;
        const string pcGrade = "SA5-S03";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // Live NPR has moved to 6.00 since release — the frozen source was 5.00.
        await InsertLiveStaffRowAsync(conn, pcGrade, fpsYear, payRate: 10.00m, npr: 6.00m, ohr: 2.00m);
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertFrozenStagingRowAsync(
            conn, jobQueueId, pcGrade, payRate: 12.00m, npr: 5.00m, ohr: 2.00m,
            calculatedAction: StaffAnimalCalculatedAction.Update,
            sourcePayRate: 10.00m, sourceNpr: 5.00m, sourceOhr: 2.00m,
            effectivePayRate: 12.00m, effectiveNpr: 5.00m, effectiveOhr: 2.00m,
            validationVersion: StaffAnimalValidationVersion.Current);

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkStaffRatesUpdate, fpsYear)));

            Assert.Contains("drift", ex.Message, StringComparison.OrdinalIgnoreCase);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT payrate::numeric, npr::numeric FROM fps.profitcentregrade WHERE pcgrade = @pcgrade AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("pcgrade", pcGrade);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(10.00m, r.GetDecimal(0));
                Assert.Equal(6.00m, r.GetDecimal(1));
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
            await CleanupAsync(jobQueueId, fpsYear, [pcGrade]);
        }
    }

    // ── Live row removed after freeze → hard failure, never skipped ────────────────

    [SkippableFact]
    public async Task ExecuteAsync_WhenLiveRowRemovedAfterFreeze_ThrowsAndAppliesNothing()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2094;
        const string pcGrade = "SA5-S04";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // No live row inserted at all — simulates the grade being removed between release and
        // execution. NotFound is a hard failure, never skip-and-log.
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertFrozenStagingRowAsync(
            conn, jobQueueId, pcGrade, payRate: 12.00m, npr: 5.00m, ohr: 2.00m,
            calculatedAction: StaffAnimalCalculatedAction.Update,
            sourcePayRate: 10.00m, sourceNpr: 5.00m, sourceOhr: 2.00m,
            effectivePayRate: 12.00m, effectiveNpr: 5.00m, effectiveOhr: 2.00m,
            validationVersion: StaffAnimalValidationVersion.Current);

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkStaffRatesUpdate, fpsYear)));

            Assert.Contains("drift", ex.Message, StringComparison.OrdinalIgnoreCase);

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

    // ── Frozen validation_version no longer matches the deployed rule set ──────────

    [SkippableFact]
    public async Task ExecuteAsync_WhenValidationVersionMismatches_ThrowsAndAppliesNothing()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2095;
        const string pcGrade = "SA5-S05";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertLiveStaffRowAsync(conn, pcGrade, fpsYear, payRate: 10.00m, npr: 5.00m, ohr: 2.00m);
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertFrozenStagingRowAsync(
            conn, jobQueueId, pcGrade, payRate: 12.00m, npr: 5.00m, ohr: 2.00m,
            calculatedAction: StaffAnimalCalculatedAction.Update,
            sourcePayRate: 10.00m, sourceNpr: 5.00m, sourceOhr: 2.00m,
            effectivePayRate: 12.00m, effectiveNpr: 5.00m, effectiveOhr: 2.00m,
            validationVersion: StaffAnimalValidationVersion.Current + 999);

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkStaffRatesUpdate, fpsYear)));

            Assert.Contains("validation_version", ex.Message, StringComparison.OrdinalIgnoreCase);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT payrate::numeric FROM fps.profitcentregrade WHERE pcgrade = @pcgrade AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("pcgrade", pcGrade);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(10.00m, r.GetDecimal(0));
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [pcGrade]);
        }
    }
}
