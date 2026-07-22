using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;

namespace Apha.BatchJobs.Infrastructure.Context;

/// <summary>
/// Scoped holder for the current job execution's resolved identity and parameters.
/// See <see cref="ICurrentJobExecutionContext"/> for the population contract.
/// </summary>
public sealed class CurrentJobExecutionContext : ICurrentJobExecutionContext
{
    public Guid JobExecutionId { get; private set; }
    public Guid JobQueueId { get; private set; }
    public string JobName { get; private set; } = string.Empty;
    public RunMode RunMode { get; private set; }
    public string RequestedBy { get; private set; } = string.Empty;
    public string? ParametersJson { get; private set; }

    public void Initialize(
        Guid jobExecutionId,
        Guid jobQueueId,
        string jobName,
        RunMode runMode,
        string requestedBy,
        string? parametersJson)
    {
        JobExecutionId = jobExecutionId;
        JobQueueId = jobQueueId;
        JobName = jobName;
        RunMode = runMode;
        RequestedBy = requestedBy;
        ParametersJson = parametersJson;
    }
}
