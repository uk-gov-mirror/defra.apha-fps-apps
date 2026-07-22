namespace Apha.BatchJobs.Domain.Exceptions;

/// <summary>
/// Thrown when the MilestoneUpdateNotifications settings preflight finds a required
/// <c>mabarchive.tbl_settings</c> row missing, duplicated, or blank. Without this row,
/// <c>vprojectreports_pmmail</c> silently collapses to zero candidates — indistinguishable
/// from a genuine empty period — so this must fail the run rather than proceed. A configuration
/// problem, distinct from <see cref="JobValidationException"/> (request/parameter validation).
/// </summary>
public sealed class NotificationSettingsConfigurationException : Exception
{
    public NotificationSettingsConfigurationException(string message) : base(message) { }

    public NotificationSettingsConfigurationException(string message, Exception innerException)
        : base(message, innerException) { }
}
