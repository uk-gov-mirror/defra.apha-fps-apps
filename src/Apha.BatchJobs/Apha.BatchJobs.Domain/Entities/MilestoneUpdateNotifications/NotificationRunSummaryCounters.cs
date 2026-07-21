namespace Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

/// <summary>
/// Counters accumulated during a single MilestoneUpdateNotifications run.
/// Passed to <c>INotificationDeliveryRepository.InsertRunSummaryAsync</c> once reporting-year
/// resolution succeeds and updated incrementally during the send loop.
/// Mirrors the <c>fps.notification_run_summary</c> schema (plan §11.4, CR055).
/// </summary>
public sealed class NotificationRunSummaryCounters
{
    public int CandidateCount { get; set; }
    public int CandidateProjectCount { get; set; }
    public int IdentifiedRecipientCount { get; set; }

    public int ManagerDeliveryCount { get; set; }
    public int ManagerEmailAttemptCount { get; set; }
    public int ManagerEmailSentCount { get; set; }
    public int ManagerEmailFailedCount { get; set; }
    public int ManagerEmailSkippedCount { get; set; }

    public int DisabledRecipientCount { get; set; }
    public int DisabledProjectCount { get; set; }
    public int MissingEmailRecipientCount { get; set; }
    public int MissingEmailProjectCount { get; set; }

    public int? UnresolvedRecipientCount { get; set; }   // null = diagnostic unavailable
    public int? UnresolvedProjectCount { get; set; }
    public bool DiagnosticAvailable { get; set; } = true;

    public int OutcomeUnknownRecipientCount { get; set; }
    public int DuplicateSkippedCount { get; set; }
}
