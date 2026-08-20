using Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;
using Apha.BatchJobs.Domain.Interfaces.MilestoneUpdateNotifications;

namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MilestoneUpdateNotifications.Services;

/// <summary>
/// Builds and sends the post-run CAPS completion email (spec §15) and
/// writes the outcome to <c>fps.notification_run_summary</c> via
/// <see cref="INotificationDeliveryRepository.FinalizeRunSummaryAsync"/>.
/// </summary>
public interface ICapsSummaryService
{
    /// <summary>
    /// Sends the CAPS completion email to the configured mailbox and writes the result
    /// back to the run summary row. Non-fatal by design — callers must log the failure
    /// but must not let a CAPS send failure propagate as a job failure.
    /// </summary>
    Task SendAndRecordAsync(
        Guid runSummaryId,
        Guid jobQueueId,
        int fpsYear,
        int monthNumber,
        NotificationRunSummaryCounters counters,
        CancellationToken cancellationToken);
}
