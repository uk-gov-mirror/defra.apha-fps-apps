namespace Apha.BatchJobs.Pact.Api.Models;

/// <summary>Request body for POST /batch-jobs/{jobName}/cancel.</summary>
public sealed class BatchCancelRequest
{
    /// <summary>The job execution GUID to cancel.</summary>
    public required string JobExecutionId { get; set; }

    /// <summary>Identity of the user or service requesting the cancellation.</summary>
    public required string RequestedBy { get; set; }
}
