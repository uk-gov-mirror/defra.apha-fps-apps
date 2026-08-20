using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;
using Apha.BatchJobs.Domain.Interfaces.MilestoneUpdateNotifications;
using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Apha.BatchJobs.Infrastructure.Repositories.MilestoneUpdateNotifications;

/// <summary>
/// Npgsql-backed write repository for the MilestoneUpdateNotifications delivery audit tables
/// (<c>fps.notification_delivery</c>, <c>fps.notification_delivery_project</c>) and the
/// per-execution run summary (<c>fps.notification_run_summary</c>).
///
/// Raw Npgsql (not EF LINQ) is intentional: these operations require explicit transaction
/// boundaries and INSERT-then-UPDATE sequences that are simpler and safer to express in SQL
/// directly than through a change-tracking DbContext. This matches the pattern established
/// by <c>BulkRatesRepository</c> for other write-intensive paths in this codebase.
///
/// All write methods that touch <c>fps.notification_delivery</c> are expected to be called
/// while <c>fps.job_lock</c> is held by <c>JobOrchestrator</c> — the lock is the primary
/// protection against cross-execution races.
/// </summary>
public sealed class NotificationDeliveryRepository : INotificationDeliveryRepository
{
    private readonly string _connectionString;
    private readonly ILogger<NotificationDeliveryRepository> _logger;

    public NotificationDeliveryRepository(string connectionString, ILogger<NotificationDeliveryRepository> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // -----------------------------------------------------------------------
    // Run summary (fps.notification_run_summary)
    // -----------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<Guid> GetOrCreateRunSummaryAsync(
        Guid jobQueueId,
        string notificationType,
        int fpsYear,
        int monthNumber,
        CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        // ON CONFLICT (jobqueueid) makes this safe for a whole-job retry that re-invokes the job
        // with the same jobQueueId: the first attempt's row is found and reused instead of a
        // unique-violation on the jobqueueid constraint masking the original transient failure.
        const string sql = @"
            INSERT INTO fps.notification_run_summary (
                jobqueueid, notificationtype, fpsyear, monthnumber,
                candidatecount, candidateprojectcount, identifiedrecipientcount,
                managerdeliverycount, manageremailattemptcount, manageremailsentcount,
                manageremailfailedcount, manageremailskippedcount,
                disabledrecipientcount, disabledprojectcount,
                missingemailrecipientcount, missingemailprojectcount,
                unresolvedrecipientcount, unresolvedprojectcount,
                diagnosticavailable, outcomeunknownrecipientcount,
                duplicateskippedcount, capssummarystatus, createdatutc, updatedatutc
            ) VALUES (
                @jobqueueid, @notificationtype, @fpsyear, @monthnumber,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                NULL, NULL, true, 0, 0, 'Pending', now(), now()
            )
            ON CONFLICT (jobqueueid) DO UPDATE SET
                fpsyear      = EXCLUDED.fpsyear,
                monthnumber  = EXCLUDED.monthnumber,
                updatedatutc = now()
            RETURNING notificationrunsummaryid";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
        cmd.Parameters.AddWithValue("notificationtype", notificationType);
        cmd.Parameters.AddWithValue("fpsyear", fpsYear);
        cmd.Parameters.AddWithValue("monthnumber", monthNumber);

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        var id = (Guid)result!;

        _logger.LogInformation(
            "notification_run_summary ready | RunSummaryId={RunSummaryId} | JobQueueId={JobQueueId} | Year={Year} | Month={Month}",
            id, jobQueueId, fpsYear, monthNumber);

        return id;
    }

    /// <inheritdoc />
    public async Task UpdateRunSummaryCountersAsync(
        Guid runSummaryId,
        NotificationRunSummaryCounters counters,
        CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        const string sql = @"
            UPDATE fps.notification_run_summary SET
                candidatecount               = @candidatecount,
                candidateprojectcount         = @candidateprojectcount,
                identifiedrecipientcount      = @identifiedrecipientcount,
                managerdeliverycount          = @managerdeliverycount,
                manageremailattemptcount      = @manageremailattemptcount,
                manageremailsentcount         = @manageremailsentcount,
                manageremailfailedcount       = @manageremailfailedcount,
                manageremailskippedcount      = @manageremailskippedcount,
                disabledrecipientcount        = @disabledrecipientcount,
                disabledprojectcount          = @disabledprojectcount,
                missingemailrecipientcount    = @missingemailrecipientcount,
                missingemailprojectcount      = @missingemailprojectcount,
                unresolvedrecipientcount      = @unresolvedrecipientcount,
                unresolvedprojectcount        = @unresolvedprojectcount,
                diagnosticavailable           = @diagnosticavailable,
                outcomeunknownrecipientcount  = @outcomeunknownrecipientcount,
                duplicateskippedcount         = @duplicateskippedcount,
                updatedatutc                  = now()
            WHERE notificationrunsummaryid = @id";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", runSummaryId);
        cmd.Parameters.AddWithValue("candidatecount", counters.CandidateCount);
        cmd.Parameters.AddWithValue("candidateprojectcount", counters.CandidateProjectCount);
        cmd.Parameters.AddWithValue("identifiedrecipientcount", counters.IdentifiedRecipientCount);
        cmd.Parameters.AddWithValue("managerdeliverycount", counters.ManagerDeliveryCount);
        cmd.Parameters.AddWithValue("manageremailattemptcount", counters.ManagerEmailAttemptCount);
        cmd.Parameters.AddWithValue("manageremailsentcount", counters.ManagerEmailSentCount);
        cmd.Parameters.AddWithValue("manageremailfailedcount", counters.ManagerEmailFailedCount);
        cmd.Parameters.AddWithValue("manageremailskippedcount", counters.ManagerEmailSkippedCount);
        cmd.Parameters.AddWithValue("disabledrecipientcount", counters.DisabledRecipientCount);
        cmd.Parameters.AddWithValue("disabledprojectcount", counters.DisabledProjectCount);
        cmd.Parameters.AddWithValue("missingemailrecipientcount", counters.MissingEmailRecipientCount);
        cmd.Parameters.AddWithValue("missingemailprojectcount", counters.MissingEmailProjectCount);
        AddNullableInt(cmd, "unresolvedrecipientcount", counters.UnresolvedRecipientCount);
        AddNullableInt(cmd, "unresolvedprojectcount", counters.UnresolvedProjectCount);
        cmd.Parameters.AddWithValue("diagnosticavailable", counters.DiagnosticAvailable);
        cmd.Parameters.AddWithValue("outcomeunknownrecipientcount", counters.OutcomeUnknownRecipientCount);
        cmd.Parameters.AddWithValue("duplicateskippedcount", counters.DuplicateSkippedCount);

        var rows = await cmd.ExecuteNonQueryAsync(cancellationToken);
        if (rows == 0)
        {
            _logger.LogWarning(
                "UpdateRunSummaryCountersAsync matched 0 rows — RunSummaryId={RunSummaryId} may not exist",
                runSummaryId);
        }
    }

    /// <inheritdoc />
    public async Task FinalizeRunSummaryAsync(
        Guid runSummaryId,
        string capsStatus,
        string? capsFailureMessage,
        DateTime? capsSentAtUtc,
        CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        const string sql = @"
            UPDATE fps.notification_run_summary SET
                capssummarystatus         = @capssummarystatus,
                capssummaryfailuremessage = @capssummaryfailuremessage,
                capssummarysentatutc      = @capssummarysentatutc,
                updatedatutc              = now()
            WHERE notificationrunsummaryid = @id";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", runSummaryId);
        cmd.Parameters.AddWithValue("capssummarystatus", capsStatus);
        AddNullableString(cmd, "capssummaryfailuremessage", capsFailureMessage);
        AddNullableTimestampTz(cmd, "capssummarysentatutc", capsSentAtUtc);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    // -----------------------------------------------------------------------
    // Three-outcome business-key check
    // -----------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<ExistingDeliveryAttempt?> GetExistingAttemptAsync(
        NotificationDeliveryKey key,
        CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        // Ordered by createdatutc DESC so forceResend reruns return the most-recent attempt,
        // and a Sending row from a crashed execution is found even when a later Sent row also
        // exists (by checking the most-recent row, not any Sent row).
        const string sql = @"
            SELECT notificationdeliveryid, deliverystatus, jobqueueid
            FROM fps.notification_delivery
            WHERE notificationtype = @notificationtype
              AND fpsyear          = @fpsyear
              AND monthnumber      = @monthnumber
              AND recipientid      = @recipientid
            ORDER BY createdatutc DESC
            LIMIT 1";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("notificationtype", key.NotificationType);
        cmd.Parameters.AddWithValue("fpsyear", key.FpsYear);
        cmd.Parameters.AddWithValue("monthnumber", key.MonthNumber);
        cmd.Parameters.AddWithValue("recipientid", key.RecipientId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new ExistingDeliveryAttempt(
            NotificationDeliveryId: reader.GetGuid(0),
            DeliveryStatus: reader.GetString(1),
            JobQueueId: reader.GetGuid(2));
    }

    /// <inheritdoc />
    public async Task TransitionToOutcomeUnknownAsync(
        Guid notificationDeliveryId,
        CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        // notification_delivery has no updatedatutc column per schema; failuremessage tracks reason
        const string parentSqlFull = @"
            UPDATE fps.notification_delivery
            SET deliverystatus  = 'OutcomeUnknown',
                failuremessage  = 'Prior execution crashed between send call and outcome confirmation.'
            WHERE notificationdeliveryid = @id AND deliverystatus = 'Sending'";

        await using var parentCmd = new NpgsqlCommand(parentSqlFull, conn, tx);
        parentCmd.Parameters.AddWithValue("id", notificationDeliveryId);
        await parentCmd.ExecuteNonQueryAsync(cancellationToken);

        const string childSql = @"
            UPDATE fps.notification_delivery_project
            SET deliverystatus = 'OutcomeUnknown'
            WHERE notificationdeliveryid = @id AND deliverystatus = 'Pending'";

        await using var childCmd = new NpgsqlCommand(childSql, conn, tx);
        childCmd.Parameters.AddWithValue("id", notificationDeliveryId);
        await childCmd.ExecuteNonQueryAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);

        _logger.LogWarning(
            "Transitioned prior Sending delivery to OutcomeUnknown | DeliveryId={DeliveryId}",
            notificationDeliveryId);
    }

    // -----------------------------------------------------------------------
    // Delivery row lifecycle (write order)
    // -----------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<Guid> InsertPendingDeliveryAsync(
        Guid jobQueueId,
        NotificationDeliveryKey key,
        string? durablePersonId,
        string recipientName,
        string? recipientEmail,
        bool isForceResend,
        string templateVersion,
        IReadOnlyList<(string ProjectCode, int FpsYear, string ChildStatus, string? ChildOutcomeReason)> children,
        CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        const string parentSql = @"
            INSERT INTO fps.notification_delivery (
                jobqueueid, notificationtype, fpsyear, monthnumber,
                recipientid, durablepersonid, recipientname, recipientemail,
                deliverystatus, isforceresend, templateversion, createdatutc
            ) VALUES (
                @jobqueueid, @notificationtype, @fpsyear, @monthnumber,
                @recipientid, @durablepersonid, @recipientname, @recipientemail,
                'Pending', @isforceresend, @templateversion, now()
            )
            RETURNING notificationdeliveryid";

        await using var parentCmd = new NpgsqlCommand(parentSql, conn, tx);
        parentCmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
        parentCmd.Parameters.AddWithValue("notificationtype", key.NotificationType);
        parentCmd.Parameters.AddWithValue("fpsyear", key.FpsYear);
        parentCmd.Parameters.AddWithValue("monthnumber", key.MonthNumber);
        parentCmd.Parameters.AddWithValue("recipientid", key.RecipientId);
        AddNullableString(parentCmd, "durablepersonid", durablePersonId);
        parentCmd.Parameters.AddWithValue("recipientname", recipientName);
        AddNullableString(parentCmd, "recipientemail", recipientEmail);
        parentCmd.Parameters.AddWithValue("isforceresend", isForceResend);
        parentCmd.Parameters.AddWithValue("templateversion", templateVersion);

        var deliveryId = (Guid)(await parentCmd.ExecuteScalarAsync(cancellationToken))!;

        const string childSql = @"
            INSERT INTO fps.notification_delivery_project (
                notificationdeliveryid, fpsyear, projectcode, deliverystatus, outcomereason
            ) VALUES (@deliveryid, @fpsyear, @projectcode, @deliverystatus, @outcomereason)";

        foreach (var child in children)
        {
            await using var childCmd = new NpgsqlCommand(childSql, conn, tx);
            childCmd.Parameters.AddWithValue("deliveryid", deliveryId);
            childCmd.Parameters.AddWithValue("fpsyear", child.FpsYear);
            childCmd.Parameters.AddWithValue("projectcode", child.ProjectCode);
            childCmd.Parameters.AddWithValue("deliverystatus", child.ChildStatus);
            AddNullableString(childCmd, "outcomereason", child.ChildOutcomeReason);
            await childCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
        return deliveryId;
    }

    /// <inheritdoc />
    public async Task UpdateDeliveryToSendingAsync(
        Guid notificationDeliveryId,
        CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        const string sql = @"
            UPDATE fps.notification_delivery
            SET deliverystatus = 'Sending'
            WHERE notificationdeliveryid = @id AND deliverystatus = 'Pending'";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", notificationDeliveryId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateDeliveryOutcomeAsync(
        Guid notificationDeliveryId,
        string deliveryStatus,
        string? failureMessage,
        DateTime? sentAtUtc,
        CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        const string parentSql = @"
            UPDATE fps.notification_delivery
            SET deliverystatus = @status,
                failuremessage = @failuremessage,
                sentatutc      = @sentatutc
            WHERE notificationdeliveryid = @id";

        await using var parentCmd = new NpgsqlCommand(parentSql, conn, tx);
        parentCmd.Parameters.AddWithValue("id", notificationDeliveryId);
        parentCmd.Parameters.AddWithValue("status", deliveryStatus);
        AddNullableString(parentCmd, "failuremessage", failureMessage);
        AddNullableTimestampTz(parentCmd, "sentatutc", sentAtUtc);
        await parentCmd.ExecuteNonQueryAsync(cancellationToken);

        // Mirror outcome onto Pending children only — Skipped children retain their status.
        const string childSql = @"
            UPDATE fps.notification_delivery_project
            SET deliverystatus = @status
            WHERE notificationdeliveryid = @id AND deliverystatus = 'Pending'";

        await using var childCmd = new NpgsqlCommand(childSql, conn, tx);
        childCmd.Parameters.AddWithValue("id", notificationDeliveryId);
        childCmd.Parameters.AddWithValue("status", deliveryStatus);
        await childCmd.ExecuteNonQueryAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task InsertSkippedDeliveryAsync(
        Guid jobQueueId,
        NotificationDeliveryKey key,
        string? durablePersonId,
        string recipientName,
        string? recipientEmail,
        string outcomeReason,
        string templateVersion,
        IReadOnlyList<(string ProjectCode, int FpsYear)> projects,
        CancellationToken cancellationToken)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);

        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        const string parentSql = @"
            INSERT INTO fps.notification_delivery (
                jobqueueid, notificationtype, fpsyear, monthnumber,
                recipientid, durablepersonid, recipientname, recipientemail,
                deliverystatus, outcomereason, isforceresend, templateversion, createdatutc
            ) VALUES (
                @jobqueueid, @notificationtype, @fpsyear, @monthnumber,
                @recipientid, @durablepersonid, @recipientname, @recipientemail,
                'Skipped', @outcomereason, false, @templateversion, now()
            )
            RETURNING notificationdeliveryid";

        await using var parentCmd = new NpgsqlCommand(parentSql, conn, tx);
        parentCmd.Parameters.AddWithValue("jobqueueid", jobQueueId);
        parentCmd.Parameters.AddWithValue("notificationtype", key.NotificationType);
        parentCmd.Parameters.AddWithValue("fpsyear", key.FpsYear);
        parentCmd.Parameters.AddWithValue("monthnumber", key.MonthNumber);
        parentCmd.Parameters.AddWithValue("recipientid", key.RecipientId);
        AddNullableString(parentCmd, "durablepersonid", durablePersonId);
        parentCmd.Parameters.AddWithValue("recipientname", recipientName);
        AddNullableString(parentCmd, "recipientemail", recipientEmail);
        parentCmd.Parameters.AddWithValue("outcomereason", outcomeReason);
        parentCmd.Parameters.AddWithValue("templateversion", templateVersion);

        var deliveryId = (Guid)(await parentCmd.ExecuteScalarAsync(cancellationToken))!;

        const string childSql = @"
            INSERT INTO fps.notification_delivery_project (
                notificationdeliveryid, fpsyear, projectcode, deliverystatus, outcomereason
            ) VALUES (@deliveryid, @fpsyear, @projectcode, 'Skipped', @outcomereason)";

        foreach (var project in projects)
        {
            await using var childCmd = new NpgsqlCommand(childSql, conn, tx);
            childCmd.Parameters.AddWithValue("deliveryid", deliveryId);
            childCmd.Parameters.AddWithValue("fpsyear", project.FpsYear);
            childCmd.Parameters.AddWithValue("projectcode", project.ProjectCode);
            childCmd.Parameters.AddWithValue("outcomereason", outcomeReason);
            await childCmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static void AddNullableString(NpgsqlCommand cmd, string name, string? value)
    {
        if (value is null)
            cmd.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Text) { Value = DBNull.Value });
        else
            cmd.Parameters.AddWithValue(name, value);
    }

    private static void AddNullableInt(NpgsqlCommand cmd, string name, int? value)
    {
        if (value is null)
            cmd.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Integer) { Value = DBNull.Value });
        else
            cmd.Parameters.AddWithValue(name, value.Value);
    }

    private static void AddNullableTimestampTz(NpgsqlCommand cmd, string name, DateTime? value)
    {
        if (value is null)
            cmd.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.TimestampTz) { Value = DBNull.Value });
        else
            cmd.Parameters.AddWithValue(name, value.Value.ToUniversalTime());
    }
}
