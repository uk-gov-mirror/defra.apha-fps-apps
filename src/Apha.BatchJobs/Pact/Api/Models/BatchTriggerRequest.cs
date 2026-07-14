namespace Apha.BatchJobs.Pact.Api.Models;

/// <summary>Request body for POST /batch-jobs/trigger.</summary>
public sealed class BatchTriggerRequest
{
    /// <summary>Registered batch job name to run.</summary>
    public required string JobName { get; set; }

    /// <summary>Identity of the user or service requesting the trigger.</summary>
    public required string RequestedBy { get; set; }

    /// <summary>
    /// Optional JSON blob of job-specific parameters forwarded to the worker
    /// via the BATCH_JOB_PARAMETERS_JSON environment variable.
    /// </summary>
    public string? ParametersJson { get; set; }
}