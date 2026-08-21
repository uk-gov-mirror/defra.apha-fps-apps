using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates;
using Apha.BatchJobs.Application.Jobs.ManualJobs.BulkRates.Services;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.BulkRates;
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
/// <see cref="BulkAnimalRatesService"/> revalidates and applies changes inside a single
/// transaction against SELECT ... FOR UPDATE-locked live rows, using the same shared
/// <see cref="IStaffAnimalValidationService"/> the API validator calls, comparing
/// against the frozen source_*/effective_*/calculated_action/validation_version
/// columns across all five mutable fields. Mirrors
/// <c>BulkStaffRatesServiceIntegrationTests</c>'s structure. Tests use explicit
/// DELETE-based cleanup (not transaction rollback) because ExecuteAsync commits its own
/// transaction internally.
/// </summary>
[Trait("Category", "Integration")]
public sealed class BulkAnimalRatesServiceIntegrationTests : IAsyncLifetime
{
    private const string DefaultConnectionString =
        "Host=fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com;Port=5432;Database=batchjobs;Username=fpsdev;Password=ijZFiEr5BnKoiLXxD1g7Zg;SSL Mode=Require;Trust Server Certificate=true";
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
        new BulkRatesRepository(new TestDbContextFactory(_connectionString), NullLogger<BulkRatesRepository>.Instance),
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

    private async Task InsertLiveAnimalRowAsync(
        NpgsqlConnection conn, string animalType, int fpsYear,
        decimal dailyRate, decimal defraDailyRate, bool planByWeek, string species, string securityLevel)
    {
        await EnsureYearInMasterAsync(conn, fpsYear);
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

    // â”€â”€ Update action applies the frozen effective_* state (all 5 fields) and
    // writes history only for the fields that actually changed â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
            calculatedAction: "Update",
            sourceDailyRate: 10.00m, sourceDefraDailyRate: 10.00m, sourcePlanByWeek: false, sourceSpecies: "Bovine", sourceSecurityLevel: "Low",
            effectiveDailyRate: 15.00m, effectiveDefraDailyRate: 10.00m, effectivePlanByWeek: false, effectiveSpecies: "Bovine", effectiveSecurityLevel: "High",
            validationVersion: 1);

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

    // â”€â”€ NoChange action applies nothing and writes no history â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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
            calculatedAction: "NoChange",
            sourceDailyRate: 10.00m, sourceDefraDailyRate: 10.00m, sourcePlanByWeek: true, sourceSpecies: "Ovine", sourceSecurityLevel: "Low",
            effectiveDailyRate: 10.00m, effectiveDefraDailyRate: 10.00m, effectivePlanByWeek: true, effectiveSpecies: "Ovine", effectiveSecurityLevel: "Low",
            validationVersion: 1);

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

    // â”€â”€ Insert action creates live row and writes five history rows â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [SkippableFact]
    public async Task ExecuteAsync_InsertAction_CreatesLiveRowAndFiveHistoryRows()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2093;
        const string animalType = "SA5-A10";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertInsertStagingRowAsync(conn, jobQueueId, animalType,
            effectiveSpecies: "Bovine", effectiveSecurityLevel: "Low",
            effectiveDailyRate: 15.00m, effectiveDefraDailyRate: 12.00m, effectivePlanByWeek: true);

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkAnimalRatesUpdate, fpsYear));

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT species, security_level, dailyrate::numeric, defradailyrate::numeric, planbyweek FROM fps.tblanimals WHERE animaltype = @t AND fpsyear = @y;";
                cmd.Parameters.AddWithValue("t", animalType);
                cmd.Parameters.AddWithValue("y", fpsYear);
                await using var r = await cmd.ExecuteReaderAsync();
                Assert.True(await r.ReadAsync(), "Live row must exist after Insert.");
                Assert.Equal("Bovine", r.GetString(0));
                Assert.Equal("Low", r.GetString(1));
                Assert.Equal(15.00m, r.GetDecimal(2));
                Assert.Equal(12.00m, r.GetDecimal(3));
                Assert.True(r.GetBoolean(4));
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT fieldname, oldvalue, changetype FROM fps.rate_change_history WHERE jobqueueid = @jqid ORDER BY fieldname;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                var fields = new List<(string field, bool oldNull, string changeType)>();
                await using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    fields.Add((r.GetString(0), r.IsDBNull(1), r.GetString(2)));
                Assert.Equal(5, fields.Count);
                Assert.All(fields, f => Assert.True(f.oldNull, $"old_value must be NULL for Insert history row '{f.field}'"));
                Assert.All(fields, f => Assert.Equal("Insert", f.changeType));
                Assert.Equal(["dailyrate", "defradailyrate", "planbyweek", "security_level", "species"], fields.Select(f => f.field).ToList());
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [animalType]);
        }
    }

    // â”€â”€ Insert history is written for false, zero, and null effective values â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [SkippableFact]
    public async Task ExecuteAsync_InsertAction_WritesFalseZeroAndNullFieldsAsHistoryRows()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2094;
        const string animalType = "SA5-A11";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertInsertStagingRowAsync(conn, jobQueueId, animalType,
            effectiveSpecies: null, effectiveSecurityLevel: null,
            effectiveDailyRate: 0m, effectiveDefraDailyRate: 0m, effectivePlanByWeek: false);

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkAnimalRatesUpdate, fpsYear));

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                Assert.Equal(5, (int)(await cmd.ExecuteScalarAsync())!);
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [animalType]);
        }
    }

    // â”€â”€ Insert conflict: target row already exists â€” throws, no new row, no history â”€

    [SkippableFact]
    public async Task ExecuteAsync_InsertAction_WhenTargetAlreadyExists_ThrowsAndRollsBack()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2095;
        const string animalType = "SA5-A12";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertLiveAnimalRowAsync(conn, animalType, fpsYear, dailyRate: 10m, defraDailyRate: 10m, planByWeek: false, species: "Ovine", securityLevel: "Low");
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertInsertStagingRowAsync(conn, jobQueueId, animalType,
            effectiveSpecies: "Bovine", effectiveSecurityLevel: "High",
            effectiveDailyRate: 20m, effectiveDefraDailyRate: 20m, effectivePlanByWeek: true);

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkAnimalRatesUpdate, fpsYear)));
            Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync())!);
            }

            await using (var cmd = conn.CreateCommand())
            {
                // Pre-existing row is unchanged
                cmd.CommandText = "SELECT species FROM fps.tblanimals WHERE animaltype = @t AND fpsyear = @y;";
                cmd.Parameters.AddWithValue("t", animalType);
                cmd.Parameters.AddWithValue("y", fpsYear);
                Assert.Equal("Ovine", (string)(await cmd.ExecuteScalarAsync())!);
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [animalType]);
        }
    }

    // â”€â”€ Mixed request: Insert + Update + NoChange â€” correct history, no staging after commit â”€

    [SkippableFact]
    public async Task ExecuteAsync_MixedInsertUpdateNoChange_CorrectHistoryAndNoStagingAfterCommit()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2096;
        const string typeInsert   = "SA5-A13"; // alphabetically first â†’ processed first
        const string typeUpdate   = "SA5-A14";
        const string typeNoChange = "SA5-A15";
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertLiveAnimalRowAsync(conn, typeUpdate,   fpsYear, 10m, 10m, false, "Bovine", "Low");
        await InsertLiveAnimalRowAsync(conn, typeNoChange, fpsYear, 10m, 10m, false, "Bovine", "Low");
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertInsertStagingRowAsync(conn, jobQueueId, typeInsert,
            effectiveSpecies: "Bovine", effectiveSecurityLevel: "Low",
            effectiveDailyRate: 5m, effectiveDefraDailyRate: 5m, effectivePlanByWeek: false);
        await InsertFrozenStagingRowAsync(conn, jobQueueId, typeUpdate,
            dailyRate: 20m, defraDailyRate: 10m, planByWeek: false, species: "Bovine", securityLevel: "Low",
            calculatedAction: "Update",
            sourceDailyRate: 10m, sourceDefraDailyRate: 10m, sourcePlanByWeek: false, sourceSpecies: "Bovine", sourceSecurityLevel: "Low",
            effectiveDailyRate: 20m, effectiveDefraDailyRate: 10m, effectivePlanByWeek: false, effectiveSpecies: "Bovine", effectiveSecurityLevel: "Low",
            validationVersion: 1);
        await InsertFrozenStagingRowAsync(conn, jobQueueId, typeNoChange,
            dailyRate: 10m, defraDailyRate: 10m, planByWeek: false, species: "Bovine", securityLevel: "Low",
            calculatedAction: "NoChange",
            sourceDailyRate: 10m, sourceDefraDailyRate: 10m, sourcePlanByWeek: false, sourceSpecies: "Bovine", sourceSecurityLevel: "Low",
            effectiveDailyRate: 10m, effectiveDefraDailyRate: 10m, effectivePlanByWeek: false, effectiveSpecies: "Bovine", effectiveSecurityLevel: "Low",
            validationVersion: 1);

        try
        {
            await CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkAnimalRatesUpdate, fpsYear));

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.tblanimals WHERE fpsyear = @y AND animaltype = @t;";
                cmd.Parameters.AddWithValue("y", fpsYear);
                cmd.Parameters.AddWithValue("t", typeInsert);
                Assert.Equal(1, (int)(await cmd.ExecuteScalarAsync())!);
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT changetype FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                var changeTypes = new List<string>();
                await using var r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                    changeTypes.Add(r.GetString(0));
                // 5 Insert rows for typeInsert + 1 Update row for typeUpdate (dailyrate changed)
                Assert.Equal(5, changeTypes.Count(ct => ct == "Insert"));
                Assert.Equal(1, changeTypes.Count(ct => ct == "Update"));
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
            await CleanupAsync(jobQueueId, fpsYear, [typeInsert, typeUpdate, typeNoChange]);
        }
    }

    // â”€â”€ Mixed atomic rollback: Insert A + Update B + Insert C conflicts â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // All changes including A's Insert and B's Update must roll back; C stays unchanged.

    [SkippableFact]
    public async Task ExecuteAsync_MixedRequest_WhenLaterInsertConflicts_AllPriorChangesRollBack()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2097;
        const string typeA = "SA5-A16"; // Insert (new)
        const string typeB = "SA5-A17"; // Update (pre-exists)
        const string typeC = "SA5-A18"; // Insert (pre-exists = conflict)
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertLiveAnimalRowAsync(conn, typeB, fpsYear, 10m, 10m, false, "Bovine", "Low");
        await InsertLiveAnimalRowAsync(conn, typeC, fpsYear, 10m, 10m, false, "Bovine", "Low");
        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertInsertStagingRowAsync(conn, jobQueueId, typeA,
            effectiveSpecies: "Bovine", effectiveSecurityLevel: "Low",
            effectiveDailyRate: 5m, effectiveDefraDailyRate: 5m, effectivePlanByWeek: false);
        await InsertFrozenStagingRowAsync(conn, jobQueueId, typeB,
            dailyRate: 20m, defraDailyRate: 10m, planByWeek: false, species: "Bovine", securityLevel: "Low",
            calculatedAction: "Update",
            sourceDailyRate: 10m, sourceDefraDailyRate: 10m, sourcePlanByWeek: false, sourceSpecies: "Bovine", sourceSecurityLevel: "Low",
            effectiveDailyRate: 20m, effectiveDefraDailyRate: 10m, effectivePlanByWeek: false, effectiveSpecies: "Bovine", effectiveSecurityLevel: "Low",
            validationVersion: 1);
        await InsertInsertStagingRowAsync(conn, jobQueueId, typeC,
            effectiveSpecies: "Bovine", effectiveSecurityLevel: "High",
            effectiveDailyRate: 99m, effectiveDefraDailyRate: 99m, effectivePlanByWeek: true);

        try
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkAnimalRatesUpdate, fpsYear)));

            await using (var cmd = conn.CreateCommand())
            {
                // A must not have been inserted
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.tblanimals WHERE animaltype = @t AND fpsyear = @y;";
                cmd.Parameters.AddWithValue("t", typeA);
                cmd.Parameters.AddWithValue("y", fpsYear);
                Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync())!);
            }

            await using (var cmd = conn.CreateCommand())
            {
                // B's update must have rolled back
                cmd.CommandText = "SELECT dailyrate::numeric FROM fps.tblanimals WHERE animaltype = @t AND fpsyear = @y;";
                cmd.Parameters.AddWithValue("t", typeB);
                cmd.Parameters.AddWithValue("y", fpsYear);
                Assert.Equal(10m, (decimal)(await cmd.ExecuteScalarAsync())!);
            }

            await using (var cmd = conn.CreateCommand())
            {
                // C is unchanged (Low, not High)
                cmd.CommandText = "SELECT security_level FROM fps.tblanimals WHERE animaltype = @t AND fpsyear = @y;";
                cmd.Parameters.AddWithValue("t", typeC);
                cmd.Parameters.AddWithValue("y", fpsYear);
                Assert.Equal("Low", (string)(await cmd.ExecuteScalarAsync())!);
            }

            await using (var cmd = conn.CreateCommand())
            {
                // No history from A or B
                cmd.CommandText = "SELECT COUNT(*)::int FROM fps.rate_change_history WHERE jobqueueid = @jqid;";
                cmd.Parameters.AddWithValue("jqid", jobQueueId);
                Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync())!);
            }
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [typeA, typeB, typeC]);
        }
    }

    // â”€â”€ Unexpected actions: ZeroRateWithdrawal, NotFound, Invalid â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [SkippableFact]
    public async Task ExecuteAsync_UnexpectedAction_ZeroRateWithdrawal_Throws()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        await AssertUnexpectedActionThrowsAsync("ZeroRateWithdrawal", "SA5-A20", 2098);
    }

    [SkippableFact]
    public async Task ExecuteAsync_UnexpectedAction_NotFound_Throws()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        await AssertUnexpectedActionThrowsAsync("NotFound", "SA5-A21", 2098);
    }

    [SkippableFact]
    public async Task ExecuteAsync_UnexpectedAction_Invalid_Throws()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");
        await AssertUnexpectedActionThrowsAsync("Invalid", "SA5-A22", 2098);
    }

    private async Task AssertUnexpectedActionThrowsAsync(string action, string animalType, int fpsYear)
    {
        var jobQueueId = Guid.NewGuid();
        var jobExecutionId = Guid.NewGuid();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await InsertJobQueueAsync(conn, jobQueueId, jobExecutionId, fpsYear);
        await InsertFrozenStagingRowAsync(
            conn, jobQueueId, animalType,
            dailyRate: 10m, defraDailyRate: 10m, planByWeek: false, species: "Bovine", securityLevel: "Low",
            calculatedAction: action,
            sourceDailyRate: 10m, sourceDefraDailyRate: 10m, sourcePlanByWeek: false, sourceSpecies: "Bovine", sourceSecurityLevel: "Low",
            effectiveDailyRate: 10m, effectiveDefraDailyRate: 10m, effectivePlanByWeek: false, effectiveSpecies: "Bovine", effectiveSecurityLevel: "Low",
            validationVersion: 1);

        try
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateService().ExecuteAsync(new BulkRatesExecutionContext(jobExecutionId, BatchJobNames.BulkAnimalRatesUpdate, fpsYear)));
            Assert.Contains(action, ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            await CleanupAsync(jobQueueId, fpsYear, [animalType]);
        }
    }

    // â”€â”€ Helper for Insert-action staging rows (source fields represent absent state) â”€

    private async Task InsertInsertStagingRowAsync(
        NpgsqlConnection conn, Guid jobQueueId, string animalType,
        string? effectiveSpecies, string? effectiveSecurityLevel,
        decimal effectiveDailyRate, decimal effectiveDefraDailyRate, bool effectivePlanByWeek)
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
                (@jqid, @animaltype, @species, @securitylevel, null, null, null,
                 'Insert',
                 0, 0, false, '', '',
                 @effectivedailyrate, @effectivedefradailyrate, @effectiveplanbyweek, @effectivespecies, @effectivesecuritylevel,
                 1);";
        cmd.Parameters.AddWithValue("jqid",           jobQueueId);
        cmd.Parameters.AddWithValue("animaltype",      animalType);
        cmd.Parameters.AddWithValue("species",         (object?)effectiveSpecies       ?? DBNull.Value);
        cmd.Parameters.AddWithValue("securitylevel",   (object?)effectiveSecurityLevel ?? DBNull.Value);
        cmd.Parameters.AddWithValue("effectivedailyrate",      effectiveDailyRate);
        cmd.Parameters.AddWithValue("effectivedefradailyrate", effectiveDefraDailyRate);
        cmd.Parameters.AddWithValue("effectiveplanbyweek",     effectivePlanByWeek);
        cmd.Parameters.AddWithValue("effectivespecies",        (object?)effectiveSpecies       ?? DBNull.Value);
        cmd.Parameters.AddWithValue("effectivesecuritylevel",  (object?)effectiveSecurityLevel ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    // â”€â”€ Direct 23505 mapping: row inserted by liveLookup pre-check miss
    // (concurrent-race stand-in: pre-existing row hidden from test, INSERT raised 23505)
    // Proves the catch clause in InsertAnimalRowAsync maps 23505 â†’ InvalidOperationException.

    [SkippableFact]
    public async Task InsertAnimalRowAsync_WhenInsertRaises23505_ThrowsControlledConflictError()
    {
        Skip.IfNot(CanRunIntegrationTests(), _skipReason ?? "Integration DB unavailable.");

        const int fpsYear = 2099;
        const string animalType = "SA5-A30";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await EnsureYearInMasterAsync(conn, fpsYear);

        // Pre-create the row so the subsequent INSERT will collide.
        await InsertLiveAnimalRowAsync(conn, animalType, fpsYear, 10m, 10m, false, "Ovine", "Low");
        await using var tx = await conn.BeginTransactionAsync();
        try
        {
            var stagingRow = new AnimalStagingRow(
                JobQueueId:          Guid.NewGuid(),
                AnimalType:          animalType,
                Species:             "Bovine",
                SecurityLevel:       "High",
                DailyRate:           20m,
                DefraDailyRate:      20m,
                PlanByWeek:          true,
                CalculatedAction:    "Insert",
                SourceDailyRate:     0m,
                SourceDefraDailyRate: 0m,
                SourcePlanByWeek:    false,
                SourceSpecies:       null,
                SourceSecurityLevel: null,
                EffectiveDailyRate:      20m,
                EffectiveDefraDailyRate: 20m,
                EffectivePlanByWeek:     true,
                EffectiveSpecies:        "Bovine",
                EffectiveSecurityLevel:  "High",
                ValidationVersion:   1);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => BulkRatesRepository.InsertAnimalRowAsync(conn, tx, stagingRow, fpsYear, CancellationToken.None));
            Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.IsType<Npgsql.PostgresException>(ex.InnerException);
            Assert.Equal("23505", ((Npgsql.PostgresException)ex.InnerException!).SqlState);

            await tx.RollbackAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
        finally
        {
            await using var cleanConn = new NpgsqlConnection(_connectionString);
            await cleanConn.OpenAsync();
            await using var cmd = cleanConn.CreateCommand();
            cmd.CommandText = "DELETE FROM fps.tblanimals WHERE animaltype = @t AND fpsyear = @y; DELETE FROM fps.tblyearmaster WHERE fpsyear = @y AND createdby = 'integration-test';";
            cmd.Parameters.AddWithValue("t", animalType);
            cmd.Parameters.AddWithValue("y", fpsYear);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
