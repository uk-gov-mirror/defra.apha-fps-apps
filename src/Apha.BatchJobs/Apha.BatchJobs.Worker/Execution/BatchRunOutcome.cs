namespace Apha.BatchJobs.Worker.Execution;

/// <summary>Coarse top-level outcome of one worker invocation.</summary>
public enum BatchRunOutcome
{
    Success,
    Cancelled,
    Failure
}
