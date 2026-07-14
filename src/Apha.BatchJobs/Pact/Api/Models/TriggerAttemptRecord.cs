namespace Apha.BatchJobs.Pact.Api.Models;

/// <summary>
/// A durable record of one trigger attempt, stored so that the response can be
/// reconstructed if the caller polls or if the process restarts.
/// </summary>
public sealed class TriggerAttemptRecord
{
    /// <summary>GUID string matching <c>fps.job_queue.jobexecutionid</c>.</summary>
    public required string JobExecutionId { get; set; }

    /// <summary>Registered batch job name (e.g. "BulkTestRatesUpdate").</summary>
    public required string JobName { get; set; }

    /// <summary>UTC time the trigger request was accepted by the API.</summary>
    public DateTime AcceptedAtUtc { get; set; }

    /// <summary>Opaque event / correlation identifier passed to the worker.</summary>
    public string? EventId { get; set; }

    /// <summary>True when the worker process was successfully started.</summary>
    public bool WorkerProcessLaunched { get; set; }

    /// <summary>Current lifecycle status string (e.g. "WorkerProcessStarted", "Completed").</summary>
    public required string Status { get; set; }

    /// <summary>Exit code returned by the worker process, if it has exited.</summary>
    public int? WorkerExitCode { get; set; }

    /// <summary>UTC time the record was persisted to the store.</summary>
    public DateTime StoredAtUtc { get; set; }
}
