using Apha.BatchJobs.Worker.Execution;

namespace Apha.BatchJobs.Worker.Reporting;

/// <summary>Writes exactly one structured final summary per non-startup worker invocation.</summary>
public interface IBatchRunSummaryWriter
{
    void WriteSummary(BatchExecutionResult result, TimeSpan duration);
}
