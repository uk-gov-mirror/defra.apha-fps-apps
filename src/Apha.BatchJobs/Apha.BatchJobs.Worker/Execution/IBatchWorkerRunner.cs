namespace Apha.BatchJobs.Worker.Execution;

/// <summary>Coordinates one worker invocation and returns the process exit code.</summary>
public interface IBatchWorkerRunner
{
    Task<int> RunAsync();
}
