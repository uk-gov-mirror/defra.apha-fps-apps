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
/// <see cref="BulkAnimalRatesService"/> revalidates and applies changes inside a single
/// transaction against SELECT ... FOR UPDATE-locked live rows, using the same shared
/// <see cref="IStaffAnimalValidationService"/> the API validator calls, comparing
/// against the frozen source_*/effective_*/calculated_action/validation_version
/// columns across all five mutable fields. Mirrors
/// <c>BulkStaffRatesServiceIntegrationTests</c>'s structure. Tests use explicit
/// DELETE-based cleanup (not transaction rollback) because ExecuteAsync commits its own
/// transaction internally.
/// </summary>
public sealed class BulkAnimalRatesServiceIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=batch_jobs_foundation_db_cloud;Username=postgres;Password=admin123;SSL Mode=Disable";
    private readonly string _connectionString;
    private string? _skipReason;

    public BulkAnimalRatesServiceIntegrationTests()
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
                    WHERE m.jobname = {BatchJobNames.BulkAnimalRatesUpdate} AND s.status = 'Running'")
                .SingleAsync();

            if (count == 0)
                _skipReason = $"job_master/job_status seed for '{BatchJobNames.BulkAnimalRatesUpdate}' + 'Running' is not provisioned.";
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

    private BulkAnimalRatesService CreateService() => new(
        new TestDbContextFactory(_connectionString),
        new BulkRatesRepository(new TestDbContextFactory(_connectionString), NullLogger<BulkRatesRepository>.Instance),
        new StaffAnimalValidationService(),
        NullLogger<BulkAnimalRatesService>.Instance);

    private async Task<int> ResolveStatusIdAsync(NpgsqlConnection conn, string status)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT s.statusid FROM fps.job_status s
            JOIN fps.job_master m ON m.jobid = s.jobid
            WHERE m.jobname = @jobname AND s.status = @status;";
        cmd.Parameters.AddWithValue("jobname", BatchJobNames.BulkAnimalRatesUpdate);
        cmd.Parameters.AddWithValue("status", status);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task<int> ResolveJobIdAsync(NpgsqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT jobid FROM fps.job_master WHERE jobname = @jobname;";
        cmd.Parameters.AddWithValue("jobname", BatchJobNames.BulkAnimalRatesUpdate);
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

    private async Task InsertLiveAnimalRowAsync(
        NpgsqlConnection conn, string animalType, int fpsYear,
        decimal dailyRate, decimal defraDailyRate, bool planByWeek, string species, string securityLevel)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO fps.tblanimals (animaltype, species, security_level, dailyrate, defradailyrate, planbyweek, fpsyear)
            VALUES (@animaltype, @species, @securitylevel, @dailyrate, @defradailyrate, @planbyweek, @fpsyear);";
        cmd.Parameters.AddWithValue("animaltype", animalType);
        cmd.Parameters.AddWithValue("species", species);
        cmd.Parameters.AddWithValue("securitylevel", securityLevel);
        cmd.Parameters.AddWithValue("dailyrate", dailyRate);
        cmd.Parameters.AddWithValue("defradailyrate", defraDailyRate);
        cmd.Parameters.AddWithValue("planbyweek", planByWeek);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task InsertFrozenStagingRowAsync(
        NpgsqlConnection conn, Guid jobQueueId, string animalType,
        decimal? dailyRate, decimal? defraDailyRate, bool? planByWeek, string? species, string? securityLevel,
        string calculatedAction,
        decimal sourceDailyRate, decimal sourceDefraDailyRate, bool sourcePlanByWeek, string sourceSpecies, string sourceSecurityLevel,
        decimal effectiveDailyRate, decimal effectiveDefraDailyRate, bool effectivePlanByWeek, string effectiveSpecies, string effectiveSecurityLevel,
        int validationVersion)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO fps.tblstaginganimals
                (jobqueueid, animaltype, species, security_level, dailyrate, defradailyrate, planbyweek,
                 calculated_action,
                 source_dailyrate, source_defradailyrate, source_planbyweek, source_species, source_securitylevel,
                 effective_dailyrate, effective_defradailyrate, effective_planbyweek, effective_species, effective_securitylevel,
                 validation_version)
            VALUES
                (@jqid, @animaltype, @species, @securitylevel, @dailyrate, @defradailyrate, @planbyweek,
                 @action,
                 @sourcedailyrate, @sourcedefradailyrate, @sourceplanbyweek, @sourcespecies, @sourcesecuritylevel,
                 @effectivedailyrate, @effectivedefradailyrate, @effectiveplanbyweek, @effectivespecies, @effectivesecuritylevel,
                 @version);";
        cmd.Parameters.AddWithValue("jqid", jobQueueId);
        cmd.Parameters.AddWithValue("animaltype", animalType);
        cmd.Parameters.AddWithValue("species", (object?)species ?? DBNull.Value);
        cmd.Parameters.AddWithValue("securitylevel", (object?)securityLevel ?? DBNull.Value);
        cmd.Parameters.AddWithValue("dailyrate", (object?)dailyRate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("defradailyrate", (object?)defraDailyRate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("planbyweek", (object?)planByWeek ?? DBNull.Value);
        cmd.Parameters.AddWithValue("action", calculatedAction);
        cmd.Parameters.AddWithValue("sourcedailyrate", sourceDailyRate);
        cmd.Parameters.AddWithValue("sourcedefradailyrate", sourceDefraDailyRate);
        cmd.Parameters.AddWithValue("sourceplanbyweek", sourcePlanByWeek);
        cmd.Parameters.AddWithValue("sourcespecies", sourceSpecies);
        cmd.Parameters.AddWithValue("sourcesecuritylevel", sourceSecurityLevel);
        cmd.Parameters.AddWithValue("effectivedailyrate", effectiveDailyRate);
        cmd.Parameters.AddWithValue("effectivedefradailyrate", effectiveDefraDailyRate);
        cmd.Parameters.AddWithValue("effectiveplanbyweek", effectivePlanByWeek);
        cmd.Parameters.AddWithValue("effectivespecies", effectiveSpecies);
        cmd.Parameters.AddWithValue("effectivesecuritylevel", effectiveSecurityLevel);
        cmd.Parameters.AddWithValue("version", validationVersion);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task CleanupAsync(Guid jobQueueId, int fpsYear, string[] animalTypes)
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
        await Exec("DELETE FROM fps.tblstaginganimals WHERE jobqueueid = @jqid;", c => c.Parameters.AddWithValue("jqid", jobQueueId));
        await Exec("DELETE FROM fps.job_queue WHERE jobqueueid = @jqid;", c => c.Parameters.AddWithValue("jqid", jobQueueId));
        await Exec("DELETE FROM fps.tblanimals WHERE fpsyear = @fpsyear AND animaltype = ANY(@types);",
            c => { c.Parameters.AddWithValue("fpsyear", fpsYear); c.Parameters.Add(new NpgsqlParameter("types", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = animalTypes }); });
    }

    // ── Update action applies the frozen effective_* state (all 5 fields) and
    // writes history only for the fields that actually changed ─────────────────────────

    [SkippableFact]
    public async Task ExecuteAsync_UpdateAction_AppliesEffectiveStateAndWritesHistory()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2091;
        const string animalType = "SA5-A01";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertLiveAnimalRowAsync(conn, animalType, fpsYear, dailyRate: 10.00m, defraDailyRate: 10.00m, planByWeek: false, species: "Bovine", securityLevel: "Low");
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        // Live still matches the frozen source; the approved effective target only raises
        // DailyRate and changes SecurityLevel, leaving the other three fields unchanged.
        await InsertFrozenStagingRowAsync(
            conn, jobQueueId, animalType,
            dailyRate: 15.00m, defraDailyRate: 10.00m, planByWeek: false, species: "Bovine", securityLevel: "High",
            calculatedAction: StaffAnimalCalculatedAction.Update,
            sourceDailyRate: 10.00m, sourceDefraDailyRate: 10.00m, sourcePlanByWeek: false, sourceSpecies: "Bovine", sourceSecurityLevel: "Low",
            effectiveDailyRate: 15.00m, effectiveDefraDailyRate: 10.00m, effectivePlanByWeek: false, effectiveSpecies: "Bovine", effectiveSecurityLevel: "High",
            validationVersion: StaffAnimalValidationVersion.Current);

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkAnimalRatesUpdate, fpsYear));

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT dailyrate::numeric, defradailyrate::numeric, planbyweek, species, security_level FROM fps.tblanimals WHERE animaltype = @animaltype AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("animaltype", animalType);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(15.00m, r.GetDecimal(0));
                Assert.Equal(10.00m, r.GetDecimal(1));
                Assert.False(r.GetBoolean(2));
                Assert.Equal("Bovine", r.GetString(3));
                Assert.Equal("High", r.GetString(4));
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT fieldname FROM fps.rate_change_history WHERE jobqueueid = @jqid ORDER BY fieldname;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                var fields = new List<string>();
                await using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    fields.Add(r.GetString(0));
                Assert.Equal(["dailyrate", "security_level"], fields);
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.tblstaginganimals WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync())!);
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [animalType]);
        }
    }

    // ── NoChange action applies nothing and writes no history ──────────────────────

    [SkippableFact]
    public async Task ExecuteAsync_NoChangeAction_SkipsApplyAndHistory()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2092;
        const string animalType = "SA5-A02";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertLiveAnimalRowAsync(conn, animalType, fpsYear, dailyRate: 10.00m, defraDailyRate: 10.00m, planByWeek: true, species: "Ovine", securityLevel: "Low");
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertFrozenStagingRowAsync(
            conn, jobQueueId, animalType,
            dailyRate: 10.00m, defraDailyRate: 10.00m, planByWeek: true, species: "Ovine", securityLevel: "Low",
            calculatedAction: StaffAnimalCalculatedAction.NoChange,
            sourceDailyRate: 10.00m, sourceDefraDailyRate: 10.00m, sourcePlanByWeek: true, sourceSpecies: "Ovine", sourceSecurityLevel: "Low",
            effectiveDailyRate: 10.00m, effectiveDefraDailyRate: 10.00m, effectivePlanByWeek: true, effectiveSpecies: "Ovine", effectiveSecurityLevel: "Low",
            validationVersion: StaffAnimalValidationVersion.Current);

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkAnimalRatesUpdate, fpsYear));

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync())!);
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [animalType]);
        }
    }

    // ── Live source drifted since release → throws and applies nothing ─────────────
    // Uses DefraDailyRate as the drifted field — one of the five mutable fields drift
    // detection is required to cover, not just DailyRate.

    [SkippableFact]
    public async Task ExecuteAsync_WhenLiveSourceDrifts_ThrowsAndAppliesNothing()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2093;
        const string animalType = "SA5-A03";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // Live DefraDailyRate has moved to 11.00 since release — the frozen source was 10.00.
        await InsertLiveAnimalRowAsync(conn, animalType, fpsYear, dailyRate: 10.00m, defraDailyRate: 11.00m, planByWeek: false, species: "Bovine", securityLevel: "Low");
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertFrozenStagingRowAsync(
            conn, jobQueueId, animalType,
            dailyRate: 15.00m, defraDailyRate: 10.00m, planByWeek: false, species: "Bovine", securityLevel: "Low",
            calculatedAction: StaffAnimalCalculatedAction.Update,
            sourceDailyRate: 10.00m, sourceDefraDailyRate: 10.00m, sourcePlanByWeek: false, sourceSpecies: "Bovine", sourceSecurityLevel: "Low",
            effectiveDailyRate: 15.00m, effectiveDefraDailyRate: 10.00m, effectivePlanByWeek: false, effectiveSpecies: "Bovine", effectiveSecurityLevel: "Low",
            validationVersion: StaffAnimalValidationVersion.Current);

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkAnimalRatesUpdate, fpsYear)));

            Assert.Contains("drift", ex.Message, StringComparison.OrdinalIgnoreCase);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT dailyrate::numeric, defradailyrate::numeric FROM fps.tblanimals WHERE animaltype = @animaltype AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("animaltype", animalType);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(10.00m, r.GetDecimal(0));
                Assert.Equal(11.00m, r.GetDecimal(1));
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
            await CleanupAsync(jobQueueId, fpsYear, [animalType]);
        }
    }

    // ── Live row removed after freeze → hard failure, never skipped ────────────────

    [SkippableFact]
    public async Task ExecuteAsync_WhenLiveRowRemovedAfterFreeze_ThrowsAndAppliesNothing()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2094;
        const string animalType = "SA5-A04";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // No live row inserted at all — simulates the animal type being removed between
        // release and execution. Hard failure, never skip-and-log.
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertFrozenStagingRowAsync(
            conn, jobQueueId, animalType,
            dailyRate: 15.00m, defraDailyRate: 10.00m, planByWeek: false, species: "Bovine", securityLevel: "Low",
            calculatedAction: StaffAnimalCalculatedAction.Update,
            sourceDailyRate: 10.00m, sourceDefraDailyRate: 10.00m, sourcePlanByWeek: false, sourceSpecies: "Bovine", sourceSecurityLevel: "Low",
            effectiveDailyRate: 15.00m, effectiveDefraDailyRate: 10.00m, effectivePlanByWeek: false, effectiveSpecies: "Bovine", effectiveSecurityLevel: "Low",
            validationVersion: StaffAnimalValidationVersion.Current);

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkAnimalRatesUpdate, fpsYear)));

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
            await CleanupAsync(jobQueueId, fpsYear, [animalType]);
        }
    }

    // ── Frozen validation_version no longer matches the deployed rule set ──────────

    [SkippableFact]
    public async Task ExecuteAsync_WhenValidationVersionMismatches_ThrowsAndAppliesNothing()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2095;
        const string animalType = "SA5-A05";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertLiveAnimalRowAsync(conn, animalType, fpsYear, dailyRate: 10.00m, defraDailyRate: 10.00m, planByWeek: false, species: "Bovine", securityLevel: "Low");
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertFrozenStagingRowAsync(
            conn, jobQueueId, animalType,
            dailyRate: 15.00m, defraDailyRate: 10.00m, planByWeek: false, species: "Bovine", securityLevel: "Low",
            calculatedAction: StaffAnimalCalculatedAction.Update,
            sourceDailyRate: 10.00m, sourceDefraDailyRate: 10.00m, sourcePlanByWeek: false, sourceSpecies: "Bovine", sourceSecurityLevel: "Low",
            effectiveDailyRate: 15.00m, effectiveDefraDailyRate: 10.00m, effectivePlanByWeek: false, effectiveSpecies: "Bovine", effectiveSecurityLevel: "Low",
            validationVersion: StaffAnimalValidationVersion.Current + 999);

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkAnimalRatesUpdate, fpsYear)));

            Assert.Contains("validation_version", ex.Message, StringComparison.OrdinalIgnoreCase);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT dailyrate::numeric FROM fps.tblanimals WHERE animaltype = @animaltype AND fpsyear = @year;";
                cmd.Parameters.AddWithValue("animaltype", animalType);
                cmd.Parameters.AddWithValue("year", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync());
                Assert.Equal(10.00m, r.GetDecimal(0));
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [animalType]);
        }
    }
}
