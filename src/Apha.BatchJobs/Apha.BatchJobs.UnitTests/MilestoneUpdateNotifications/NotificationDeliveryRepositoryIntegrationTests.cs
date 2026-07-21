using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Apha.BatchJobs.Infrastructure.Repositories.MilestoneUpdateNotifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Xunit;

namespace Apha.BatchJobs.UnitTests.MilestoneUpdateNotifications;

/// <summary>
/// PostgreSQL-backed integration tests for <see cref="NotificationDeliveryRepository"/>.
///
/// These tests commit rows to the live database and clean up in <see cref="DisposeAsync"/>.
/// They are guarded by <see cref="SkippableFactAttribute"/> + <see cref="Skip.IfNot"/>:
///   - Skip if the database cannot be reached (CI / sandbox — no Postgres available).
///   - Skip if the MilestoneUpdateNotifications job-master seed (CR053) is not yet applied.
///
/// To run locally, ensure the connection string is available via:
///   appsettings.Local.json in Apha.BatchJobs.Worker, OR
///   environment variable ConnectionStrings__FPSConnectionString.
/// Do NOT set RUN_INTEGRATION_TESTS in CI — the tests skip automatically when the DB is unreachable.
/// </summary>
public sealed class NotificationDeliveryRepositoryIntegrationTests : IAsyncLifetime
{
    // ── Connection ──────────────────────────────────────────────────────────────
    // Fallback used only when no local config file or env var is present (i.e. in CI/sandbox).
    // Without a password the connection attempt fails → InitializeAsync sets _skipReason → all tests skip.
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=batch_jobs_foundation_db_cloud;Username=postgres;Timeout=5";

    private readonly string _connectionString;
    private string? _skipReason;

    // Test-scoped job_queue row IDs — inserted in InitializeAsync, deleted in DisposeAsync.
    private Guid _testJobQueueId = Guid.Empty;
    private Guid _testJobQueueId2 = Guid.Empty;   // for tests that need a second independent queue row

    // ── Ctor ────────────────────────────────────────────────────────────────────
    public NotificationDeliveryRepositoryIntegrationTests()
    {
        _connectionString = ResolveConnectionString();
    }

    // ── IAsyncLifetime ──────────────────────────────────────────────────────────
    public async Task InitializeAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // Verify CR053 seed exists (job_master row for MilestoneUpdateNotifications).
            await using var checkCmd = new NpgsqlCommand(@"
                SELECT COUNT(*) FROM fps.job_master
                WHERE jobname = 'MilestoneUpdateNotifications'", conn);

            var count = (long)(await checkCmd.ExecuteScalarAsync())!;
            if (count == 0)
            {
                _skipReason = "CR053 seed (fps.job_master row for MilestoneUpdateNotifications) not applied on this database.";
                return;
            }

            // Verify notification tables from CR054/CR055 exist.
            await using var tableCheckCmd = new NpgsqlCommand(@"
                SELECT COUNT(*) FROM information_schema.tables
                WHERE table_schema = 'fps'
                  AND table_name IN ('notification_delivery', 'notification_delivery_project', 'notification_run_summary')", conn);
            var tableCount = (long)(await tableCheckCmd.ExecuteScalarAsync())!;
            if (tableCount < 3)
            {
                _skipReason = "CR054/CR055 tables (fps.notification_delivery, notification_delivery_project, notification_run_summary) not applied on this database.";
                return;
            }

            // Insert two test fps.job_queue rows (one per FK slot needed across all tests).
            _testJobQueueId  = await InsertTestJobQueueRowAsync(conn);
            _testJobQueueId2 = await InsertTestJobQueueRowAsync(conn);
        }
        catch (Exception ex)
        {
            _skipReason = $"Integration DB unavailable: {ex.Message}";
        }
    }

    public async Task DisposeAsync()
    {
        if (_testJobQueueId == Guid.Empty && _testJobQueueId2 == Guid.Empty)
            return;

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            foreach (var queueId in new[] { _testJobQueueId, _testJobQueueId2 }
                         .Where(id => id != Guid.Empty))
            {
                await using var cmd = new NpgsqlCommand(@"
                    DELETE FROM fps.notification_delivery WHERE jobqueueid = @id;
                    DELETE FROM fps.notification_run_summary WHERE jobqueueid = @id;
                    DELETE FROM fps.job_queue WHERE jobqueueid = @id;", conn);
                cmd.Parameters.AddWithValue("id", queueId);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch
        {
            // Best-effort cleanup — do not obscure a test failure.
        }
    }

    // ── Run summary lifecycle ───────────────────────────────────────────────────

    [SkippableFact]
    public async Task InsertRunSummaryAsync_InsertsRowWithPendingStatus()
    {
        Skip.IfNot(CanRun(), _skipReason!);

        var repo = CreateRepository();
        var runSummaryId = await repo.InsertRunSummaryAsync(
            _testJobQueueId, "MilestoneUpdate", fpsYear: 2026, monthNumber: 7,
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, runSummaryId);

        var row = await ReadRunSummaryAsync(runSummaryId);
        Assert.NotNull(row);
        Assert.Equal("Pending", row!["capssummarystatus"]);
        Assert.Equal(0, (int)row["candidatecount"]);
        Assert.True((bool)row["diagnosticavailable"]);
    }

    [SkippableFact]
    public async Task UpdateRunSummaryCountersAsync_UpdatesAllCounters()
    {
        Skip.IfNot(CanRun(), _skipReason!);

        var repo = CreateRepository();
        var runSummaryId = await repo.InsertRunSummaryAsync(
            _testJobQueueId, "MilestoneUpdate", 2026, 7, CancellationToken.None);

        var counters = new NotificationRunSummaryCounters
        {
            CandidateCount            = 10,
            CandidateProjectCount     = 20,
            IdentifiedRecipientCount  = 5,
            ManagerDeliveryCount      = 4,
            ManagerEmailAttemptCount  = 3,
            ManagerEmailSentCount     = 2,
            ManagerEmailFailedCount   = 1,
            ManagerEmailSkippedCount  = 0,
            DisabledRecipientCount    = 1,
            DisabledProjectCount      = 2,
            MissingEmailRecipientCount = 0,
            MissingEmailProjectCount  = 0,
            UnresolvedRecipientCount  = 3,
            UnresolvedProjectCount    = 6,
            DiagnosticAvailable       = true,
            OutcomeUnknownRecipientCount = 0,
            DuplicateSkippedCount     = 0,
        };

        await repo.UpdateRunSummaryCountersAsync(runSummaryId, counters, CancellationToken.None);

        var row = await ReadRunSummaryAsync(runSummaryId);
        Assert.Equal(10, (int)row!["candidatecount"]);
        Assert.Equal(20, (int)row!["candidateprojectcount"]);
        Assert.Equal(5,  (int)row!["identifiedrecipientcount"]);
        Assert.Equal(2,  (int)row!["manageremailsentcount"]);
        Assert.Equal(1,  (int)row!["manageremailfailedcount"]);
        Assert.Equal(3,  (int)row!["unresolvedrecipientcount"]);
        Assert.Equal(6,  (int)row!["unresolvedprojectcount"]);
    }

    [SkippableFact]
    public async Task FinalizeRunSummaryAsync_UpdatesCapsStatusToSent()
    {
        Skip.IfNot(CanRun(), _skipReason!);

        var repo = CreateRepository();
        var runSummaryId = await repo.InsertRunSummaryAsync(
            _testJobQueueId, "MilestoneUpdate", 2026, 7, CancellationToken.None);

        var sentAt = DateTime.UtcNow;
        await repo.FinalizeRunSummaryAsync(runSummaryId, "Sent", null, sentAt, CancellationToken.None);

        var row = await ReadRunSummaryAsync(runSummaryId);
        Assert.Equal("Sent", row!["capssummarystatus"]);
        Assert.Null(row["capssummaryfailuremessage"]);
        Assert.NotNull(row["capssummarysentatutc"]);
    }

    [SkippableFact]
    public async Task FinalizeRunSummaryAsync_UpdatesCapsStatusToFailed_WithMessage()
    {
        Skip.IfNot(CanRun(), _skipReason!);

        var repo = CreateRepository();
        var runSummaryId = await repo.InsertRunSummaryAsync(
            _testJobQueueId, "MilestoneUpdate", 2026, 7, CancellationToken.None);

        await repo.FinalizeRunSummaryAsync(runSummaryId, "Failed", "SMTP error", null, CancellationToken.None);

        var row = await ReadRunSummaryAsync(runSummaryId);
        Assert.Equal("Failed", row!["capssummarystatus"]);
        Assert.Equal("SMTP error", row!["capssummaryfailuremessage"]);
        Assert.Null(row["capssummarysentatutc"]);
    }

    // ── Delivery row lifecycle ───────────────────────────────────────────────────

    [SkippableFact]
    public async Task InsertPendingDeliveryAsync_InsertsParentWithPendingChildren()
    {
        Skip.IfNot(CanRun(), _skipReason!);

        var repo = CreateRepository();
        var key = MakeKey("RCP001");
        var children = new List<(string, int, string, string?)>
        {
            ("AH0001", 2026, "Pending", null),
            ("AH0002", 2026, "Skipped", "NoValidProjectLinks"),
        };

        var deliveryId = await repo.InsertPendingDeliveryAsync(
            _testJobQueueId, key, "M12345", "Test Manager", "mgr@example.com",
            isForceResend: false, "v1", children, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, deliveryId);

        var (parent, childRows) = await ReadDeliveryAsync(deliveryId);
        Assert.Equal("Pending", parent!["deliverystatus"]);
        Assert.Equal("mgr@example.com", parent["recipientemail"]);
        Assert.Equal(2, childRows.Count);
        Assert.Contains(childRows, r => (string)r["deliverystatus"] == "Pending");
        Assert.Contains(childRows, r => (string)r["deliverystatus"] == "Skipped"
                                     && (string)r["outcomereason"] == "NoValidProjectLinks");
    }

    [SkippableFact]
    public async Task UpdateDeliveryToSendingAsync_TransitionsPendingToSending()
    {
        Skip.IfNot(CanRun(), _skipReason!);

        var repo = CreateRepository();
        var deliveryId = await InsertTestDeliveryAsync(repo, "RCP002", "Pending");

        await repo.UpdateDeliveryToSendingAsync(deliveryId, CancellationToken.None);

        var (parent, _) = await ReadDeliveryAsync(deliveryId);
        Assert.Equal("Sending", parent!["deliverystatus"]);
    }

    [SkippableFact]
    public async Task UpdateDeliveryOutcomeAsync_Sent_UpdatesParentAndPendingChildren_PreservesSkipped()
    {
        Skip.IfNot(CanRun(), _skipReason!);

        var repo = CreateRepository();
        var key = MakeKey("RCP003");
        var children = new List<(string, int, string, string?)>
        {
            ("AH0001", 2026, "Pending",  null),
            ("AH0002", 2026, "Skipped",  "NoValidProjectLinks"),
        };

        var deliveryId = await repo.InsertPendingDeliveryAsync(
            _testJobQueueId, key, null, "Mgr", "mgr@test.com",
            false, "v1", children, CancellationToken.None);
        await repo.UpdateDeliveryToSendingAsync(deliveryId, CancellationToken.None);

        var sentAt = DateTime.UtcNow;
        await repo.UpdateDeliveryOutcomeAsync(deliveryId, "Sent", null, sentAt, CancellationToken.None);

        var (parent, childRows) = await ReadDeliveryAsync(deliveryId);
        Assert.Equal("Sent", parent!["deliverystatus"]);
        Assert.NotNull(parent["sentatutc"]);

        // Pending child promoted to Sent; Skipped child preserved.
        Assert.Contains(childRows, r => (string)r["deliverystatus"] == "Sent");
        Assert.Contains(childRows, r => (string)r["deliverystatus"] == "Skipped");
    }

    [SkippableFact]
    public async Task UpdateDeliveryOutcomeAsync_Failed_UpdatesParentAndPendingChildren()
    {
        Skip.IfNot(CanRun(), _skipReason!);

        var repo = CreateRepository();
        var deliveryId = await InsertTestDeliveryAsync(repo, "RCP004", "Pending");
        await repo.UpdateDeliveryToSendingAsync(deliveryId, CancellationToken.None);
        await repo.UpdateDeliveryOutcomeAsync(deliveryId, "Failed", "Graph error", null, CancellationToken.None);

        var (parent, childRows) = await ReadDeliveryAsync(deliveryId);
        Assert.Equal("Failed", parent!["deliverystatus"]);
        Assert.Equal("Graph error", parent!["failuremessage"]);
        Assert.All(childRows, r => Assert.Equal("Failed", (string)r["deliverystatus"]));
    }

    [SkippableFact]
    public async Task InsertSkippedDeliveryAsync_InsertsSkippedParentWithProjectRows()
    {
        Skip.IfNot(CanRun(), _skipReason!);

        var repo = CreateRepository();
        var key = MakeKey("RCP005");
        var projects = new List<(string, int)> { ("AH0001", 2026), ("AH0002", 2026) };

        await repo.InsertSkippedDeliveryAsync(
            _testJobQueueId, key, null, "Disabled Mgr", null,
            "RecipientDisabled", "v1", projects, CancellationToken.None);

        // Verify via GetExistingAttemptAsync
        var attempt = await repo.GetExistingAttemptAsync(key, CancellationToken.None);
        Assert.NotNull(attempt);
        Assert.Equal("Skipped", attempt!.DeliveryStatus);
    }

    // ── Three-outcome check (GetExistingAttemptAsync, TransitionToOutcomeUnknownAsync) ──

    [SkippableFact]
    public async Task GetExistingAttemptAsync_WhenNoPriorDelivery_ReturnsNull()
    {
        Skip.IfNot(CanRun(), _skipReason!);

        var repo = CreateRepository();
        var key = MakeKey("RCP_NONE");

        var result = await repo.GetExistingAttemptAsync(key, CancellationToken.None);

        Assert.Null(result);
    }

    [SkippableFact]
    public async Task GetExistingAttemptAsync_ReturnsMostRecentDelivery()
    {
        Skip.IfNot(CanRun(), _skipReason!);

        var repo = CreateRepository();
        var key = MakeKey("RCP006");

        // Insert a Skipped row first, then a Sent row (simulates forceResend scenario).
        await repo.InsertSkippedDeliveryAsync(
            _testJobQueueId, key, null, "Mgr", null,
            "RecipientDisabled", "v1", [("AH0001", 2026)], CancellationToken.None);

        var deliveryId = await InsertTestDeliveryAsync(repo, "RCP006", "Pending");
        await repo.UpdateDeliveryToSendingAsync(deliveryId, CancellationToken.None);
        await repo.UpdateDeliveryOutcomeAsync(deliveryId, "Sent", null, DateTime.UtcNow, CancellationToken.None);

        var attempt = await repo.GetExistingAttemptAsync(key, CancellationToken.None);

        Assert.NotNull(attempt);
        Assert.Equal("Sent", attempt!.DeliveryStatus);
        Assert.Equal(deliveryId, attempt.NotificationDeliveryId);
    }

    [SkippableFact]
    public async Task TransitionToOutcomeUnknownAsync_UpdatesSendingRowAndPendingChildren()
    {
        Skip.IfNot(CanRun(), _skipReason!);

        var repo = CreateRepository();
        var key = MakeKey("RCP007");
        var children = new List<(string, int, string, string?)>
        {
            ("AH0001", 2026, "Pending", null),
            ("AH0002", 2026, "Skipped", "NoValidProjectLinks"),
        };

        var deliveryId = await repo.InsertPendingDeliveryAsync(
            _testJobQueueId, key, null, "Mgr", "mgr@test.com",
            false, "v1", children, CancellationToken.None);
        await repo.UpdateDeliveryToSendingAsync(deliveryId, CancellationToken.None);

        await repo.TransitionToOutcomeUnknownAsync(deliveryId, CancellationToken.None);

        var (parent, childRows) = await ReadDeliveryAsync(deliveryId);
        Assert.Equal("OutcomeUnknown", parent!["deliverystatus"]);
        Assert.Contains("crashed", ((string)parent["failuremessage"]).ToLowerInvariant());

        // Only the Pending child is transitioned; Skipped is preserved.
        Assert.Contains(childRows, r => (string)r["deliverystatus"] == "OutcomeUnknown");
        Assert.Contains(childRows, r => (string)r["deliverystatus"] == "Skipped");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    // ── Connection string resolution ────────────────────────────────────────────
    // Checks appsettings.Local.json (dotnet config format) which TestConnectionStringResolver
    // deliberately skips (it only reads appsettings.json / appsettings.Development.json).
    // Falls back gracefully — connection failure in InitializeAsync sets _skipReason and all tests skip.

    private static string ResolveConnectionString()
    {
        // 1. Environment variable (CI override or local override)
        var fromEnv = Environment.GetEnvironmentVariable("ConnectionStrings__FPSConnectionString");
        if (!string.IsNullOrWhiteSpace(fromEnv) && !fromEnv.StartsWith('<'))
            return fromEnv;

        // 2. appsettings.Local.json beside the Worker project (dotnet config format)
        var localFile = FindWorkerLocalJson();
        if (localFile is not null)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(localFile, optional: false)
                .Build();
            var cs = config.GetConnectionString("FPSConnectionString");
            if (!string.IsNullOrWhiteSpace(cs) && !cs.StartsWith('<'))
                return cs;
        }

        // 3. No credentials available — return password-less fallback so InitializeAsync
        //    catches the auth failure and skips cleanly (same as CI behaviour).
        return DefaultConnectionString;
    }

    private static string? FindWorkerLocalJson()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "Apha.BatchJobs.Worker", "appsettings.Local.json");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        return null;
    }

    private NotificationDeliveryRepository CreateRepository() =>
        new(_connectionString, NullLogger<NotificationDeliveryRepository>.Instance);

    private bool CanRun() => string.IsNullOrWhiteSpace(_skipReason);

    /// <summary>Inserts a test fps.job_queue row and returns its jobqueueid.</summary>
    private static async Task<Guid> InsertTestJobQueueRowAsync(NpgsqlConnection conn)
    {
        var id = Guid.NewGuid();
        // Use any Running-status row for the job. If the seed has no Running statusid,
        // fall back to the first available status for this job.
        await using var cmd = new NpgsqlCommand(@"
            INSERT INTO fps.job_queue
                (jobqueueid, jobexecutionid, jobid, statusid, requestedby, requested_at_utc, startdatetime)
            SELECT @id, @execid, m.jobid, s.statusid,
                   'integration-test-notification-delivery', now(), now()
            FROM fps.job_master m
            JOIN fps.job_status s ON s.jobid = m.jobid
            WHERE m.jobname = 'MilestoneUpdateNotifications'
            ORDER BY CASE s.status WHEN 'Running' THEN 0 ELSE 1 END, s.statusid
            LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("execid", Guid.NewGuid());
        await cmd.ExecuteNonQueryAsync();
        return id;
    }

    /// <summary>Inserts a minimal delivery row for use as a test fixture.</summary>
    private async Task<Guid> InsertTestDeliveryAsync(
        NotificationDeliveryRepository repo,
        string recipientSuffix,
        string status)
    {
        var key = MakeKey(recipientSuffix);
        return await repo.InsertPendingDeliveryAsync(
            _testJobQueueId, key, null, $"Mgr-{recipientSuffix}", $"{recipientSuffix}@test.com",
            false, "v1",
            [("AH0001", 2026, status, null)],
            CancellationToken.None);
    }

    private NotificationDeliveryKey MakeKey(string recipientSuffix) => new(
        NotificationType: "MilestoneUpdate",
        FpsYear:          2026,
        MonthNumber:      7,
        RecipientId:      $"INTTEST-{recipientSuffix}");

    private async Task<Dictionary<string, object?>> ReadRunSummaryAsync(Guid runSummaryId)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT * FROM fps.notification_run_summary WHERE notificationrunsummaryid = @id", conn);
        cmd.Parameters.AddWithValue("id", runSummaryId);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return new Dictionary<string, object?>();
        return Enumerable.Range(0, reader.FieldCount)
            .ToDictionary(
                reader.GetName,
                i => reader.IsDBNull(i) ? (object?)null : reader.GetValue(i),
                StringComparer.OrdinalIgnoreCase);
    }

    private async Task<(Dictionary<string, object?>? Parent, List<Dictionary<string, object?>> Children)>
        ReadDeliveryAsync(Guid deliveryId)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        Dictionary<string, object?>? parent = null;
        await using (var cmd = new NpgsqlCommand(
            "SELECT * FROM fps.notification_delivery WHERE notificationdeliveryid = @id", conn))
        {
            cmd.Parameters.AddWithValue("id", deliveryId);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                parent = Enumerable.Range(0, reader.FieldCount)
                    .ToDictionary(
                        reader.GetName,
                        i => reader.IsDBNull(i) ? (object?)null : reader.GetValue(i),
                        StringComparer.OrdinalIgnoreCase);
        }

        var children = new List<Dictionary<string, object?>>();
        await using (var cmd = new NpgsqlCommand(
            "SELECT * FROM fps.notification_delivery_project WHERE notificationdeliveryid = @id", conn))
        {
            cmd.Parameters.AddWithValue("id", deliveryId);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                children.Add(Enumerable.Range(0, reader.FieldCount)
                    .ToDictionary(
                        reader.GetName,
                        i => reader.IsDBNull(i) ? (object?)null : reader.GetValue(i),
                        StringComparer.OrdinalIgnoreCase));
        }

        return (parent, children);
    }
}
