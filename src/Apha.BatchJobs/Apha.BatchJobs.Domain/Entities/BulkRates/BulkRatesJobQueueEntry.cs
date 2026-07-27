namespace Apha.BatchJobs.Domain.Entities.BulkRates;

/// <summary>
/// Represents the fps.job_queue row for a BulkRates request as read by the worker.
/// Loaded by jobexecutionid while the row is in Running status (JobOrchestrator already
/// transitioned it from Approved); ApprovedBy/ApprovedAtUtc still carry the earlier approval.
/// </summary>
public sealed record BulkRatesJobQueueEntry(
    Guid JobQueueId,
    Guid JobExecutionId,
    int JobId,
    string JobName,
    string Status,
    int FpsYear,
    string RequestedBy,
    string? ApprovedBy,
    DateTime? ApprovedAtUtc,
    int? UploadVersion = null,
    int? ActiveDownloadVersion = null);
