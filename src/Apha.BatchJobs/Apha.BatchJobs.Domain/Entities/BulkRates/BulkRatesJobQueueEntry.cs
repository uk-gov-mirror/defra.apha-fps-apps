namespace Apha.BatchJobs.Domain.Entities.BulkRates;

/// <summary>
/// Represents the fps.job_queue row for a BulkRates request as read by the worker.
/// Loaded by jobexecutionid; must be in Approved status before execution proceeds.
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
    DateTime? ApprovedAtUtc);
