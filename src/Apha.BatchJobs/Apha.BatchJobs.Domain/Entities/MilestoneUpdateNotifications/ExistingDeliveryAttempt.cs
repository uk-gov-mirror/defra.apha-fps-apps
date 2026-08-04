namespace Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

/// <summary>
/// Result of the three-outcome business-key check:
/// a prior delivery attempt found for a given (notificationtype, fpsyear, monthnumber, recipientid) key.
/// The status field drives which of the three branches the handler takes.
/// </summary>
public sealed record ExistingDeliveryAttempt(
    Guid NotificationDeliveryId,
    string DeliveryStatus,
    Guid JobQueueId);
