namespace Apha.BatchJobs.Domain.Entities.MilestoneUpdateNotifications;

/// <summary>
/// The business-key uniquely identifying a recipient for a given notification period.
/// Used for the three-outcome duplicate-prevention check.
/// </summary>
public sealed record NotificationDeliveryKey(
    string NotificationType,
    int FpsYear,
    int MonthNumber,
    string RecipientId);
