namespace Apha.BatchJobs.Infrastructure.Data;

/// <summary>
/// EF entity for fps.job_master.
/// </summary>
internal sealed class TblJobMaster
{
    public int JobId { get; set; }
    public required string JobName { get; set; }
    public string? Frequency { get; set; }
    public string? Note { get; set; }
    public int TimeToLive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// EF entity for fps.job_status.
/// </summary>
internal sealed class TblJobStatus
{
    public int StatusId { get; set; }
    public int JobId { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// EF entity for fps.job_queue.
/// </summary>
internal sealed class TblJobQueue
{
    public Guid JobQueueId { get; set; }
    public Guid JobExecutionId { get; set; }
    public int JobId { get; set; }
    public int StatusId { get; set; }
    public required string RequestedBy { get; set; }
    public DateTime? RequestedAtUtc { get; set; }
    public DateTime? StartDateTime { get; set; }   // Null when Initiated (not yet started)
    public DateTime? EndDateTime { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// EF entity for fps.job_queue_log.
/// </summary>
internal sealed class TblJobQueueLog
{
    public int JobQueueLogId { get; set; }
    public Guid JobQueueId { get; set; }
    public int StatusId { get; set; }
    public required string PerformedBy { get; set; }
    public DateTime LogTime { get; set; }
    public string? Note { get; set; }
}

