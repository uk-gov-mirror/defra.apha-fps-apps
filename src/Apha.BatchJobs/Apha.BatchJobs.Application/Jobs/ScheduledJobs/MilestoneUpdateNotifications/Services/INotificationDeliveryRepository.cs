using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;

/// <summary>
/// Write-path repository for <c>fps.notification_delivery</c> (parent) +
/// <c>fps.notification_delivery_project</c> (child) and <c>fps.notification_run_summary</c>.
///
/// All methods that mutate delivery rows are expected to be called while <c>fps.job_lock</c>
/// is held by <c>JobOrchestrator</c>. The three-outcome check and the subsequent insert must
/// not be interleaved with other work — they are a single logical unit.
/// </summary>
public interface INotificationDeliveryRepository
{
    // -----------------------------------------------------------------------
    // Run summary (fps.notification_run_summary)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Idempotent by <c>jobqueueid</c>: creates the per-execution run summary row (
    /// <c>capssummarystatus = 'Pending'</c>, counters at zero) the first time it is called for a
    /// given <c>jobQueueId</c>, or returns the existing row's id on every subsequent call —
    /// including a whole-job retry that re-invokes the job with the same jobQueueId. Callers
    /// must not assume this always inserts a fresh row. Called once per attempt, after
    /// reporting-year resolution succeeds.
    /// </summary>
    Task<Guid> GetOrCreateRunSummaryAsync(
        Guid jobQueueId,
        string notificationType,
        int fpsYear,
        int monthNumber,
        CancellationToken cancellationToken);

    /// <summary>
    /// Overwrites all counter columns on the run summary row.
    /// Called after the send loop completes, before CAPS send.
    /// </summary>
    Task UpdateRunSummaryCountersAsync(
        Guid runSummaryId,
        NotificationRunSummaryCounters counters,
        CancellationToken cancellationToken);

    /// <summary>
    /// Sets <c>capssummarystatus</c> and the CAPS audit fields on the run summary row.
    /// Called after the CAPS email send attempt (or when the step is skipped).
    /// </summary>
    Task FinalizeRunSummaryAsync(
        Guid runSummaryId,
        string capsStatus,
        string? capsFailureMessage,
        DateTime? capsSentAtUtc,
        CancellationToken cancellationToken);

    // -----------------------------------------------------------------------
    // Three-outcome business-key check
    // -----------------------------------------------------------------------

    /// <summary>
    /// Queries <c>idx_notification_delivery_business_key</c> for the most recent attempt
    /// matching the given key. Returns null when no prior attempt exists.
    /// Must be called within the same scope that will (conditionally) insert a new row,
    /// while the job lock is held — never release the lock between this call and the insert.
    /// </summary>
    Task<ExistingDeliveryAttempt?> GetExistingAttemptAsync(
        NotificationDeliveryKey key,
        CancellationToken cancellationToken);

    /// <summary>
    /// Transitions a <c>Sending</c> parent row (and its <c>Pending</c> child rows) to
    /// <c>OutcomeUnknown</c> — evidence that a prior execution crashed after calling the
    /// email provider but before confirming the outcome.
    /// </summary>
    Task TransitionToOutcomeUnknownAsync(
        Guid notificationDeliveryId,
        CancellationToken cancellationToken);

    // -----------------------------------------------------------------------
    // Delivery row lifecycle (write order)
    // -----------------------------------------------------------------------

    /// <summary>
    /// Inserts the parent delivery row as <c>Pending</c> together with its child project rows
    /// (valid projects as <c>Pending</c>; excluded/skipped projects as <c>Skipped</c>).
    /// Returns the generated <c>notificationdeliveryid</c>.
    /// Call this immediately before transitioning to <c>Sending</c> and invoking the email send.
    /// </summary>
    Task<Guid> InsertPendingDeliveryAsync(
        Guid jobQueueId,
        NotificationDeliveryKey key,
        string? durablePersonId,
        string recipientName,
        string? recipientEmail,
        bool isForceResend,
        string templateVersion,
        IReadOnlyList<(string ProjectCode, int FpsYear, string ChildStatus, string? ChildOutcomeReason)> children,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates the parent row's <c>deliverystatus</c> from <c>Pending</c> to <c>Sending</c>.
    /// Call this immediately before invoking <c>IEmailService.SendAsync</c>.
    /// </summary>
    Task UpdateDeliveryToSendingAsync(
        Guid notificationDeliveryId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates the parent row to <c>Sent</c> or <c>Failed</c> and mirrors the same status onto
    /// every child row currently at <c>Pending</c> (preserving explicitly-set <c>Skipped</c>
    /// child rows unchanged). Also sets <c>sentatutc</c> / <c>failuremessage</c> on the parent.
    /// </summary>
    Task UpdateDeliveryOutcomeAsync(
        Guid notificationDeliveryId,
        string deliveryStatus,
        string? failureMessage,
        DateTime? sentAtUtc,
        CancellationToken cancellationToken);

    /// <summary>
    /// Inserts a parent row directly as <c>Skipped</c> (with its child project rows all set to
    /// <c>Skipped</c> too). Used for disabled recipients, missing-email recipients, and
    /// recipients with zero valid project links — groups that bypass the send attempt entirely.
    /// </summary>
    Task InsertSkippedDeliveryAsync(
        Guid jobQueueId,
        NotificationDeliveryKey key,
        string? durablePersonId,
        string recipientName,
        string? recipientEmail,
        string outcomeReason,
        string templateVersion,
        IReadOnlyList<(string ProjectCode, int FpsYear)> projects,
        CancellationToken cancellationToken);
}
